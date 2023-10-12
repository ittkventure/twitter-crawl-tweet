using Medallion.Threading;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Tweet;
using TK.Twitter.Crawl.Tweet.AirTable;
using TK.TwitterAccount.Domain.Dto;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using static System.Collections.Specialized.BitVector32;

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
        private readonly Lead3Manager _lead3Manager;
        private readonly AirTableService _airTableService;
        private readonly IClock _clock;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IDistributedLockProvider _distributedLockProvider;

        public LeadProcessWaitingJob(
            IBackgroundJobManager backgroundJobManager,
            IRepository<AirTableManualSourceEntity, long> airTableManualSourceRepository,
            IRepository<AirTableWaitingProcessEntity, long> airTableWaitingProcessRepository,
            IRepository<LeadWaitingProcessEntity, long> leadWaitingProcessRepository,
            IRepository<LeadEntity, long> LeadRepository,
            IRepository<TwitterUserSignalEntity, long> twitterUserSignalRepository,
            Lead3Manager lead3Manager,
            AirTableService airTableService,
            IClock clock,
            IUnitOfWorkManager unitOfWorkManager,
            IDistributedLockProvider distributedLockProvider)
        {
            _backgroundJobManager = backgroundJobManager;
            _airTableManualSourceRepository = airTableManualSourceRepository;
            _airTableWaitingProcessRepository = airTableWaitingProcessRepository;
            _leadWaitingProcessRepository = leadWaitingProcessRepository;
            _leadRepository = LeadRepository;
            _twitterUserSignalRepository = twitterUserSignalRepository;
            _lead3Manager = lead3Manager;
            _airTableService = airTableService;
            _clock = clock;
            _unitOfWorkManager = unitOfWorkManager;
            _distributedLockProvider = distributedLockProvider;
        }

        [UnitOfWork(IsDisabled = true)]
        public override async Task ExecuteAsync(LeadProcessWaitingJobArg args)
        {
            await using (var handle = await _distributedLockProvider.TryAcquireLockAsync($"LeadProcessWaitingJob"))
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
                                var addUserId = await ProcessManualSourceSource(item);
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

                    var entity = await _leadRepository.InsertAsync(new LeadEntity()
                    {
                        UserId = lead.UserId,
                        UserName = lead.UserName,
                        UserScreenName = lead.UserScreenName,
                        UserProfileUrl = "https://twitter.com/" + lead.UserScreenName,
                        UserType = CrawlConsts.LeadType.LEADS,
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

        public async Task<string> ProcessManualSourceSource(LeadWaitingProcessEntity item)
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

            switch (action)
            {
                case "CREATE":

                    var entity = await _leadRepository.InsertAsync(new LeadEntity()
                    {
                        UserId = record.UserId,
                        UserName = record.UserName,
                        UserScreenName = record.UserScreenName,
                        UserProfileUrl = "https://twitter.com/" + record.UserScreenName,
                        UserType = CrawlConsts.LeadType.LEADS,
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

        /// <summary>
        /// Có thể cập nhật từ nhiều nguồn
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task UpdateLead(LeadWaitingProcessEntity item)
        {
            var leadOfAt = await _leadRepository.FirstOrDefaultAsync(x => x.UserId == item.UserId);
            var leadsOfTweetService = await _lead3Manager.GetLeadsAsync(userIds: new List<string> { item.UserId });
            var leadOfTs = leadsOfTweetService.FirstOrDefault(x => x.UserId == item.UserId);

            var manualSource = await _airTableManualSourceRepository.FirstOrDefaultAsync(x => x.UserId == item.UserId);

            if (leadOfTs == null && manualSource == null)
            {
                return;
            }

            if (leadOfTs == null && manualSource != null) // TH chỉ có dữ liệu từ manual source và k có dữ liệu từ tweet
            {
                var otherSignal = await _lead3Manager.GetSignalDescription(new List<string> { item.UserId });
                bool b = otherSignal.TryGetValue(item.UserId, out string otherSignalValue);
                var signals = await _twitterUserSignalRepository.GetListAsync(x => x.UserId == item.UserId);

                var record = await _airTableManualSourceRepository.FirstOrDefaultAsync(x => x.RecordId == item.RecordId);
                leadOfAt.Signals = signals.Select(x => x.Signal).Distinct().JoinAsString(",");

                leadOfAt.LastestSponsoredDate = record.LastestSignalTime;
                leadOfAt.LastestSponsoredTweetUrl = record.LastestSignalUrl;
                leadOfAt.TweetDescription = record.LastestSignalDescription;
                leadOfAt.MediaMentioned = record.LastestSignalFrom;

                leadOfAt.SignalDescription = b ? otherSignalValue : null;
            }
            else if (leadOfTs != null && manualSource == null) // TH chỉ k có dữ liệu từ manual source và có dữ liệu từ tweet 
            {
                leadOfAt.UserId = leadOfTs.UserId;
                leadOfAt.UserName = leadOfTs.UserName;
                leadOfAt.UserScreenName = leadOfTs.UserScreenName;
                leadOfAt.UserProfileUrl = "https://twitter.com/" + leadOfTs.UserScreenName;
                leadOfAt.UserType = leadOfTs.UserType;
                leadOfAt.UserStatus = leadOfTs.UserStatus;
                leadOfAt.Signals = leadOfTs.Signals?.JoinAsString(",");
                leadOfAt.LastestTweetId = leadOfTs.LastestTweetId;
                leadOfAt.LastestSponsoredDate = leadOfTs.LastestSponsoredDate;
                leadOfAt.LastestSponsoredTweetUrl = leadOfTs.LastestSponsoredTweetUrl;
                leadOfAt.DuplicateUrlCount = leadOfTs.DuplicateUrlCount;
                leadOfAt.TweetDescription = leadOfTs.TweetDescription;
                leadOfAt.TweetOwnerUserId = leadOfTs.TweetOwnerUserId;
                leadOfAt.MediaMentioned = leadOfTs.MediaMentioned;
                leadOfAt.MediaMentionedProfileUrl = "https://twitter.com/" + leadOfTs.MediaMentioned;
                leadOfAt.NumberOfSponsoredTweets = leadOfTs.NumberOfSponsoredTweets;
                leadOfAt.HashTags = leadOfTs.HashTags?.JoinAsString(",");
                leadOfAt.SignalDescription = leadOfTs.SignalDescription;
            }
            else if (leadOfTs != null && manualSource != null) // TH có cả 2 dữ liệu thì sử dụng dữ liệu có date gần nhất để update
            {
                leadOfAt.UserId = leadOfTs.UserId;
                leadOfAt.UserName = leadOfTs.UserName;
                leadOfAt.UserScreenName = leadOfTs.UserScreenName;
                leadOfAt.UserProfileUrl = "https://twitter.com/" + leadOfTs.UserScreenName;
                leadOfAt.UserType = leadOfTs.UserType;
                leadOfAt.UserStatus = leadOfTs.UserStatus;

                var otherSignal = await _lead3Manager.GetSignalDescription(new List<string> { item.UserId });
                bool b = otherSignal.TryGetValue(item.UserId, out string otherSignalValue);
                if (b)
                {
                    leadOfAt.SignalDescription = otherSignalValue;
                }

                var signals = await _twitterUserSignalRepository.GetListAsync(x => x.UserId == item.UserId);
                leadOfAt.Signals = signals.Select(x => x.Signal).Distinct().JoinAsString(",");

                var record = await _airTableManualSourceRepository.FirstOrDefaultAsync(x => x.RecordId == item.RecordId);
                bool useManualSource = record.LastestSignalTime > leadOfTs.LastestSponsoredDate;
                if (useManualSource)
                {
                    leadOfAt.LastestSponsoredDate = record.LastestSignalTime;
                    leadOfAt.LastestSponsoredTweetUrl = record.LastestSignalUrl;
                    leadOfAt.TweetDescription = record.LastestSignalDescription;
                    leadOfAt.MediaMentioned = record.LastestSignalFrom;
                }
                else
                {
                    leadOfAt.LastestTweetId = leadOfTs.LastestTweetId;
                    leadOfAt.LastestSponsoredDate = leadOfTs.LastestSponsoredDate;
                    leadOfAt.LastestSponsoredTweetUrl = leadOfTs.LastestSponsoredTweetUrl;
                    leadOfAt.DuplicateUrlCount = leadOfTs.DuplicateUrlCount;
                    leadOfAt.TweetDescription = leadOfTs.TweetDescription;
                    leadOfAt.TweetOwnerUserId = leadOfTs.TweetOwnerUserId;
                    leadOfAt.MediaMentioned = leadOfTs.MediaMentioned;
                    leadOfAt.MediaMentionedProfileUrl = "https://twitter.com/" + leadOfTs.MediaMentioned;
                    leadOfAt.NumberOfSponsoredTweets = leadOfTs.NumberOfSponsoredTweets;
                    leadOfAt.HashTags = leadOfTs.HashTags?.JoinAsString(",");
                }
            }

            await _leadRepository.UpdateAsync(leadOfAt, autoSave: true);

            // Thêm vào queue đồng bộ lên airtable
            await _airTableWaitingProcessRepository.InsertAsync(new AirTableWaitingProcessEntity()
            {
                BatchKey = item.BatchKey,
                UserId = item.UserId,
                TweetId = item.TweetId,
                Action = "UPDATE",
                LeadId = leadOfAt.Id,
                UserScreenName = leadOfAt.UserScreenName
            });
        }
    }
}
