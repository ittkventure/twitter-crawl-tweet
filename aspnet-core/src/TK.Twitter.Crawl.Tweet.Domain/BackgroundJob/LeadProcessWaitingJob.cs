using Medallion.Threading;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Tweet;
using TK.Twitter.Crawl.Tweet.AirTable;
using TK.Twitter.Crawl.Tweet.MemoryLock;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.TenantManagement;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace TK.Twitter.Crawl.Jobs
{
    public class LeadProcessWaitingJobArg
    {

    }

    public class LeadProcessWaitingJob : AsyncBackgroundJob<LeadProcessWaitingJobArg>, ITransientDependency
    {
        private const string LOG_PREFIX = "[LeadProcessWaitingJob] ";
        public const int BATCH_SIZE = 1;

        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<AirTableManualSourceEntity, long> _airTableManualSourceRepository;
        private readonly IRepository<AirTableWaitingProcessEntity, long> _airTableWaitingProcessRepository;
        private readonly IRepository<LeadWaitingProcessEntity, long> _leadWaitingProcessRepository;
        private readonly IRepository<LeadEntity, long> _leadRepository;
        private readonly IRepository<TwitterUserSignalEntity, long> _twitterUserSignalRepository;
        private readonly IRepository<TwitterUserEntity, long> _twitterUserRepository;
        private readonly IRepository<CoinGeckoCoinEntity, long> _coinGeckoCoinRepository;
        private readonly IRepository<LeadAnotherSourceEntity, long> _leadAnotherSourceRepository;
        private readonly Lead3Manager _lead3Manager;
        private readonly AirTableService _airTableService;
        private readonly IClock _clock;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IDistributedLockProvider _distributedLockProvider;
        private readonly MemoryLockProvider _memoryLockProvider;

        public LeadProcessWaitingJob(
            IBackgroundJobManager backgroundJobManager,
            IRepository<AirTableManualSourceEntity, long> airTableManualSourceRepository,
            IRepository<AirTableWaitingProcessEntity, long> airTableWaitingProcessRepository,
            IRepository<LeadWaitingProcessEntity, long> leadWaitingProcessRepository,
            IRepository<LeadEntity, long> LeadRepository,
            IRepository<TwitterUserSignalEntity, long> twitterUserSignalRepository,
            IRepository<TwitterUserEntity, long> twitterUserRepository,
            IRepository<CoinGeckoCoinEntity, long> coinGeckoCoinRepository,
            IRepository<LeadAnotherSourceEntity, long> leadAnotherSourceRepository,
            Lead3Manager lead3Manager,
            AirTableService airTableService,
            IClock clock,
            IUnitOfWorkManager unitOfWorkManager,
            IDistributedLockProvider distributedLockProvider,
            MemoryLockProvider memoryLockProvider)
        {
            _backgroundJobManager = backgroundJobManager;
            _airTableManualSourceRepository = airTableManualSourceRepository;
            _airTableWaitingProcessRepository = airTableWaitingProcessRepository;
            _leadWaitingProcessRepository = leadWaitingProcessRepository;
            _leadRepository = LeadRepository;
            _twitterUserSignalRepository = twitterUserSignalRepository;
            _twitterUserRepository = twitterUserRepository;
            _coinGeckoCoinRepository = coinGeckoCoinRepository;
            _leadAnotherSourceRepository = leadAnotherSourceRepository;
            _lead3Manager = lead3Manager;
            _airTableService = airTableService;
            _clock = clock;
            _unitOfWorkManager = unitOfWorkManager;
            _distributedLockProvider = distributedLockProvider;
            _memoryLockProvider = memoryLockProvider;
        }

        [UnitOfWork(IsDisabled = true)]
        public override async Task ExecuteAsync(LeadProcessWaitingJobArg args)
        {
            using (var handle = _memoryLockProvider.TryAcquireLock($"LeadProcessWaitingJob"))
            {
                if (handle == null)
                {
                    Logger.LogInformation(LOG_PREFIX + "Some process is running!!!");
                    return;
                }

                using var uow = _unitOfWorkManager.Begin();
                try
                {
                    var asyncExecuter = _leadWaitingProcessRepository.AsyncExecuter;

                    var queueQuery = from input in await _leadWaitingProcessRepository.GetQueryableAsync()
                                     where input.Ended == false
                                     select input;

                    // xử lý theo chiều oldest -> lastest
                    queueQuery = queueQuery.OrderBy(x => x.CreationTime).Take(BATCH_SIZE);

                    var queues = await asyncExecuter.ToListAsync(queueQuery);
                    if (queues.IsEmpty())
                    {
                        return;
                    }

                    var addUserIds = new List<string>();
                    foreach (var item in queues)
                    {
                        try
                        {
                            if (item.Source == CrawlConsts.Signal.Source.TWITTER_TWEET)
                            {
                                var addUserId = await ProcessTweetSource(item);
                                if (addUserId.IsNotEmpty())
                                {
                                    addUserIds.Add(addUserId);
                                }
                            }
                            else if (item.Source == CrawlConsts.Signal.Source.AIR_TABLE_MANUAL_SOURCE)
                            {
                                var addUserId = await ProcessManualSource(item);
                                if (addUserId.IsNotEmpty())
                                {
                                    addUserIds.Add(addUserId);
                                }
                            }
                            else if (item.Source == CrawlConsts.Signal.Source.COIN_GECKO)
                            {
                                var addUserId = await ProcessCoinGeckoSource(item);
                                if (addUserId.IsNotEmpty())
                                {
                                    addUserIds.Add(addUserId);
                                }
                            }

                            item.Succeed = true;
                            item.Note = null;
                        }
                        catch (Exception ex)
                        {
                            item.Note = ex.ToString();
                        }

                        item.Ended = true;
                        await _leadWaitingProcessRepository.UpdateAsync(item);
                    }

                    await _backgroundJobManager.EnqueueAsync(new TwitterAddUserJobArg()
                    {
                        UserIds = addUserIds
                    });

                    await uow.SaveChangesAsync();
                    await uow.CompleteAsync();
                }
                catch (Exception ex)
                {
                    await uow.RollbackAsync();
                    Logger.LogError(ex, LOG_PREFIX + "An error occurred while create/update lead");
                }
            }

            await _backgroundJobManager.EnqueueAsync(args);
        }

        public async Task<string> ProcessTweetSource(LeadWaitingProcessEntity item)
        {
            var airTableLeads = await _leadRepository.GetListAsync(x => x.UserId == item.UserId);

            string addUserId = null;
            string action;
            if (!airTableLeads.Any(x => x.UserId == item.UserId))
            {
                action = "CREATE";
            }
            else
            {
                action = "UPDATE";
            }

            switch (action)
            {
                case "CREATE":
                    var signals = await _twitterUserSignalRepository.GetListAsync(x => x.UserId == item.UserId);
                    var leads = await _lead3Manager.GetLeadsAsync(userIds: new List<string> { item.UserId });
                    var lead = leads.FirstOrDefault(x => x.UserId == item.UserId);

                    string userType = null;
                    if (IsLead(signals))
                    {
                        userType = CrawlConsts.LeadType.LEADS;
                    }

                    var entity = await _leadRepository.InsertAsync(new LeadEntity()
                    {
                        UserId = lead.UserId,
                        UserName = lead.UserName,
                        UserScreenName = lead.UserScreenName,
                        UserProfileUrl = "https://twitter.com/" + lead.UserScreenName,
                        UserType = userType,
                        UserStatus = "New",
                        Signals = lead.Signals?.JoinAsString(","),
                        LastestTweetId = lead.LastestTweetId,
                        LastestSponsoredDate = lead.LastestSponsoredDate,
                        LastestSponsoredTweetUrl = lead.LastestSponsoredTweetUrl,
                        DuplicateUrlCount = lead.DuplicateUrlCount,
                        TweetDescription = lead.TweetDescription,
                        TweetOwnerUserId = lead.TweetOwnerUserId,
                        MediaMentioned = lead.MediaMentioned,
                        MediaMentionedProfileUrl = "https://twitter.com/" + lead.MediaMentioned,
                        NumberOfSponsoredTweets = lead.NumberOfSponsoredTweets,
                        HashTags = lead.HashTags?.JoinAsString(","),
                        SignalDescription = lead.SignalDescription
                    }, autoSave: true);

                    addUserId = lead.UserId;

                    // Thêm vào queue đồng bộ lên airtable
                    await _airTableWaitingProcessRepository.InsertAsync(new AirTableWaitingProcessEntity()
                    {
                        BatchKey = item.BatchKey,
                        UserId = item.UserId,
                        TweetId = item.TweetId,
                        Action = action,
                        LeadId = entity.Id,
                        UserScreenName = lead.UserScreenName
                    });
                    break;
                case "UPDATE":
                    await UpdateLead(item);
                    break;
            }

            return addUserId;
        }

        public async Task<string> ProcessManualSource(LeadWaitingProcessEntity item)
        {
            var record = await _airTableManualSourceRepository.FirstOrDefaultAsync(x => x.RecordId == item.RecordId);
            if (record == null)
            {
                throw new Exception("Record not found");
            }

            var lead = await _leadRepository.FirstOrDefaultAsync(x => x.UserId == item.UserId);

            string addUserId = null;
            string action;
            if (lead == null)
            {
                action = "CREATE";
            }
            else
            {
                action = "UPDATE";
            }

            var otherSignal = await _lead3Manager.GetSignalDescription(new List<string> { item.UserId });
            bool b = otherSignal.TryGetValue(item.UserId, out string otherSignalValue);
            var signals = await _twitterUserSignalRepository.GetListAsync(x => x.UserId == item.UserId);

            string userType = null;
            if (IsLead(signals))
            {
                userType = CrawlConsts.LeadType.LEADS;
            }

            switch (action)
            {
                case "CREATE":

                    var entity = await _leadRepository.InsertAsync(new LeadEntity()
                    {
                        UserId = record.UserId,
                        UserName = record.UserName,
                        UserScreenName = record.UserScreenName,
                        UserProfileUrl = "https://twitter.com/" + record.UserScreenName,
                        UserType = userType,
                        UserStatus = "New",
                        Signals = signals.Select(x => x.Signal).Distinct().JoinAsString(","),

                        LastestSponsoredDate = record.LastestSignalTime,
                        LastestSponsoredTweetUrl = record.LastestSignalUrl,
                        TweetDescription = record.LastestSignalDescription,
                        MediaMentioned = record.LastestSignalFrom,

                        SignalDescription = b ? otherSignalValue : null,
                    }, autoSave: true);

                    addUserId = record.UserId;

                    // Thêm vào queue đồng bộ lên airtable
                    await _airTableWaitingProcessRepository.InsertAsync(new AirTableWaitingProcessEntity()
                    {
                        BatchKey = item.BatchKey,
                        UserId = item.UserId,
                        Action = action,
                        LeadId = entity.Id,
                        UserScreenName = record.UserScreenName
                    });
                    break;

                case "UPDATE":
                    await UpdateLead(item);
                    break;
            }

            return addUserId;
        }

        public async Task<string> ProcessCoinGeckoSource(LeadWaitingProcessEntity item)
        {
            var coin = await _coinGeckoCoinRepository.FirstOrDefaultAsync(x => x.CoinId == item.RecordId) ?? throw new Exception("Coin not found");
            var user = await _twitterUserRepository.FirstOrDefaultAsync(x => x.UserId == item.UserId) ?? throw new Exception("User not found");
            var lead = await _leadRepository.FirstOrDefaultAsync(x => x.UserId == item.UserId);

            string addUserId = null;
            string action;
            if (lead == null)
            {
                action = "CREATE";
            }
            else
            {
                action = "UPDATE";
            }

            var otherSignal = await _lead3Manager.GetSignalDescription(new List<string> { item.UserId });
            bool b = otherSignal.TryGetValue(item.UserId, out string otherSignalValue);
            var signals = await _twitterUserSignalRepository.GetListAsync(x => x.UserId == item.UserId);

            string userType = null;
            if (IsLead(signals))
            {
                userType = CrawlConsts.LeadType.LEADS;
            }

            var jObject = JObject.Parse(coin.JsonContent);
            var coinLastUpdated = jObject["last_updated"].ParseIfNotNull<DateTime?>();

            switch (action)
            {
                case "CREATE":
                    var entity = await _leadRepository.InsertAsync(new LeadEntity()
                    {
                        UserId = user.UserId,
                        UserName = user.Name,
                        UserScreenName = user.ScreenName,
                        UserProfileUrl = "https://twitter.com/" + user.ScreenName,
                        UserType = userType,
                        UserStatus = "New",
                        Signals = signals.Select(x => x.Signal).Distinct().JoinAsString(","),

                        LastestSponsoredDate = coinLastUpdated.HasValue ? coinLastUpdated.Value : coin.CreationTime,
                        LastestSponsoredTweetUrl = "https://www.coingecko.com/en/new-cryptocurrencies",
                        TweetDescription = "Just listed in Coingecko",
                        MediaMentioned = "Coingecko",

                        SignalDescription = b ? otherSignalValue : null,
                    }, autoSave: true);

                    addUserId = entity.UserId;

                    // Thêm vào queue đồng bộ lên airtable
                    await _airTableWaitingProcessRepository.InsertAsync(new AirTableWaitingProcessEntity()
                    {
                        BatchKey = item.BatchKey,
                        UserId = item.UserId,
                        Action = action,
                        LeadId = entity.Id,
                        UserScreenName = entity.UserScreenName
                    });
                    break;

                case "UPDATE":
                    await UpdateLead(item);
                    break;
            }

            return addUserId;
        }

        /// <summary>
        /// Có thể cập nhật từ nhiều nguồn
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task UpdateLead(LeadWaitingProcessEntity item)
        {
            var leadTwitterSources = await _lead3Manager.GetLeadsAsync(userIds: new List<string> { item.UserId });

            var leadCurrent = await _leadRepository.FirstOrDefaultAsync(x => x.UserId == item.UserId);
            var leadTwitterSource = leadTwitterSources.FirstOrDefault(x => x.UserId == item.UserId);
            var leadAnotherSources = await _leadAnotherSourceRepository.GetListAsync(x => x.UserId == item.UserId && x.UpdatedAt >= leadTwitterSource.LastestSponsoredDate.Value);

            if (leadTwitterSource == null && leadAnotherSources.IsEmpty())
            {
                return;
            }

            if (leadTwitterSource == null && leadAnotherSources.IsNotEmpty()) // TH chỉ có dữ liệu từ another source và k có dữ liệu từ tweet
            {
                var otherSignal = await _lead3Manager.GetSignalDescription(new List<string> { item.UserId });
                bool b = otherSignal.TryGetValue(item.UserId, out string otherSignalValue);
                var signals = await _twitterUserSignalRepository.GetListAsync(x => x.UserId == item.UserId);

                var lastest = leadAnotherSources.OrderByDescending(x => x.UpdatedAt).First();

                leadCurrent.Signals = signals.Select(x => x.Signal).Distinct().JoinAsString(",");
                leadCurrent.LastestSponsoredDate = lastest.UpdatedAt;
                leadCurrent.LastestSponsoredTweetUrl = lastest.SignalUrl;
                leadCurrent.TweetDescription = lastest.Description;
                leadCurrent.MediaMentioned = lastest.MediaMentioned;
                leadCurrent.SignalDescription = b ? otherSignalValue : null;
            }
            else if (leadTwitterSource != null && leadAnotherSources.IsEmpty())
            {
                leadCurrent.UserId = leadTwitterSource.UserId;
                leadCurrent.UserName = leadTwitterSource.UserName;
                leadCurrent.UserScreenName = leadTwitterSource.UserScreenName;
                leadCurrent.UserProfileUrl = "https://twitter.com/" + leadTwitterSource.UserScreenName;
                leadCurrent.UserType = leadTwitterSource.UserType;
                leadCurrent.UserStatus = leadTwitterSource.UserStatus;
                leadCurrent.Signals = leadTwitterSource.Signals?.JoinAsString(",");
                leadCurrent.LastestTweetId = leadTwitterSource.LastestTweetId;
                leadCurrent.LastestSponsoredDate = leadTwitterSource.LastestSponsoredDate;
                leadCurrent.LastestSponsoredTweetUrl = leadTwitterSource.LastestSponsoredTweetUrl;
                leadCurrent.DuplicateUrlCount = leadTwitterSource.DuplicateUrlCount;
                leadCurrent.TweetDescription = leadTwitterSource.TweetDescription;
                leadCurrent.TweetOwnerUserId = leadTwitterSource.TweetOwnerUserId;
                leadCurrent.MediaMentioned = leadTwitterSource.MediaMentioned;
                leadCurrent.MediaMentionedProfileUrl = "https://twitter.com/" + leadTwitterSource.MediaMentioned;
                leadCurrent.NumberOfSponsoredTweets = leadTwitterSource.NumberOfSponsoredTweets;
                leadCurrent.HashTags = leadTwitterSource.HashTags?.JoinAsString(",");
                leadCurrent.SignalDescription = leadTwitterSource.SignalDescription;
            }
            else if (leadTwitterSource != null && leadAnotherSources.IsNotEmpty())
            {
                leadCurrent.UserId = leadTwitterSource.UserId;
                leadCurrent.UserName = leadTwitterSource.UserName;
                leadCurrent.UserScreenName = leadTwitterSource.UserScreenName;
                leadCurrent.UserProfileUrl = "https://twitter.com/" + leadTwitterSource.UserScreenName;
                leadCurrent.UserType = leadTwitterSource.UserType;
                leadCurrent.UserStatus = leadTwitterSource.UserStatus;

                var otherSignal = await _lead3Manager.GetSignalDescription(new List<string> { item.UserId });
                bool b = otherSignal.TryGetValue(item.UserId, out string otherSignalValue);
                if (b)
                {
                    leadCurrent.SignalDescription = otherSignalValue;
                }

                var signals = await _twitterUserSignalRepository.GetListAsync(x => x.UserId == item.UserId);
                leadCurrent.Signals = signals.Select(x => x.Signal).Distinct().JoinAsString(",");

                bool userAnotherSourceToUpdate;
                var lastest = leadAnotherSources.OrderByDescending(x => x.UpdatedAt).First();
                if (lastest == null)
                {
                    userAnotherSourceToUpdate = false;
                }
                else
                {
                    userAnotherSourceToUpdate = lastest.UpdatedAt > leadTwitterSource.LastestSponsoredDate;
                }

                if (userAnotherSourceToUpdate)
                {
                    leadCurrent.LastestSponsoredDate = lastest.UpdatedAt;
                    leadCurrent.LastestSponsoredTweetUrl = lastest.SignalUrl;
                    leadCurrent.TweetDescription = lastest.Description;
                    leadCurrent.MediaMentioned = lastest.MediaMentioned;
                }
                else
                {
                    leadCurrent.LastestTweetId = leadTwitterSource.LastestTweetId;
                    leadCurrent.LastestSponsoredDate = leadTwitterSource.LastestSponsoredDate;
                    leadCurrent.LastestSponsoredTweetUrl = leadTwitterSource.LastestSponsoredTweetUrl;
                    leadCurrent.DuplicateUrlCount = leadTwitterSource.DuplicateUrlCount;
                    leadCurrent.TweetDescription = leadTwitterSource.TweetDescription;
                    leadCurrent.TweetOwnerUserId = leadTwitterSource.TweetOwnerUserId;
                    leadCurrent.MediaMentioned = leadTwitterSource.MediaMentioned;
                    leadCurrent.MediaMentionedProfileUrl = "https://twitter.com/" + leadTwitterSource.MediaMentioned;
                    leadCurrent.NumberOfSponsoredTweets = leadTwitterSource.NumberOfSponsoredTweets;
                    leadCurrent.HashTags = leadTwitterSource.HashTags?.JoinAsString(",");
                }
            }

            await _leadRepository.UpdateAsync(leadCurrent, autoSave: true);

            // Thêm vào queue đồng bộ lên airtable
            await _airTableWaitingProcessRepository.InsertAsync(new AirTableWaitingProcessEntity()
            {
                BatchKey = item.BatchKey,
                UserId = item.UserId,
                TweetId = item.TweetId,
                Action = "UPDATE",
                LeadId = leadCurrent.Id,
                UserScreenName = leadCurrent.UserScreenName
            });
        }

        /// <summary>
        /// ref: https://app.asana.com/0/1204183084369636/1205723036911441
        /// Nếu nguồn từ manual source hoặc là signal listing cgk hoặc cmc thì là lead
        /// </summary>
        /// <param name="signals"></param>
        /// <returns></returns>
        public static bool IsLead(List<TwitterUserSignalEntity> signals)
        {
            return signals.Any(s => s.Source == CrawlConsts.Signal.Source.AIR_TABLE_MANUAL_SOURCE
            || IsLeadBySignalCode(signals.Select(x => x.Signal)));
        }

        public static bool IsLeadBySignalCode(IEnumerable<string> signals)
        {
            return signals.Any(s => s == CrawlConsts.Signal.JUST_LISTED_IN_COINMARKETCAP || s == CrawlConsts.Signal.JUST_LISTED_IN_COINGECKO);
        }
    }
}
