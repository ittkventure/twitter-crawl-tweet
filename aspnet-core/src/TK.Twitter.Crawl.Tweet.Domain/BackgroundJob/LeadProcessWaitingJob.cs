using Medallion.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Tweet;
using TK.Twitter.Crawl.Tweet.AirTable;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
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

                    var signals = await _twitterUserSignalRepository.GetListAsync(x => queues.Select(q => q.UserId).Contains(x.UserId));
                    var leads = await _lead3Manager.GetLeadsAsync(userIds: queues.Select(q => q.UserId).Distinct().ToList());
                    var dbLeads = await _leadRepository.GetListAsync(x => queues.Select(q => q.UserId).Contains(x.UserId));

                    foreach (var item in queues)
                    {
                        try
                        {
                            string action;
                            if (!dbLeads.Any(x => x.UserId == item.UserId))
                            {
                                action = "CREATE";
                            }
                            else
                            {
                                action = "UPDATE";
                            }

                            var lead = leads.FirstOrDefault(x => x.UserId == item.UserId);
                            switch (action)
                            {
                                case "CREATE":
                                    var entity = await _leadRepository.InsertAsync(new LeadEntity()
                                    {
                                        UserId = lead.UserId,
                                        UserName = lead.UserName,
                                        UserScreenName = lead.UserScreenName,
                                        UserProfileUrl = "https://twitter.com/" + lead.UserScreenName,
                                        UserType = CrawlConsts.LeadType.LEADS,
                                        UserStatus = "NEW",
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
                                    }, autoSave: true);

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
                                    var dbLead = dbLeads.FirstOrDefault(x => x.UserId == item.UserId);
                                    dbLead.UserId = lead.UserId;
                                    dbLead.UserName = lead.UserName;
                                    dbLead.UserScreenName = lead.UserScreenName;
                                    dbLead.UserProfileUrl = "https://twitter.com/" + lead.UserScreenName;
                                    dbLead.UserType = CrawlConsts.LeadType.LEADS;
                                    dbLead.UserStatus = "NEW";
                                    dbLead.Signals = lead.Signals?.JoinAsString(",");
                                    dbLead.LastestTweetId = lead.LastestTweetId;
                                    dbLead.LastestSponsoredDate = lead.LastestSponsoredDate;
                                    dbLead.LastestSponsoredTweetUrl = lead.LastestSponsoredTweetUrl;
                                    dbLead.DuplicateUrlCount = lead.DuplicateUrlCount;
                                    dbLead.TweetDescription = lead.TweetDescription;
                                    dbLead.TweetOwnerUserId = lead.TweetOwnerUserId;
                                    dbLead.MediaMentioned = lead.MediaMentioned;
                                    dbLead.MediaMentionedProfileUrl = "https://twitter.com/" + lead.MediaMentioned;
                                    dbLead.NumberOfSponsoredTweets = lead.NumberOfSponsoredTweets;
                                    dbLead.HashTags = lead.HashTags?.JoinAsString(",");

                                    await _leadRepository.UpdateAsync(dbLead, autoSave: true);

                                    // Thêm vào queue đồng bộ lên airtable
                                    await _airTableWaitingProcessRepository.InsertAsync(new AirTableWaitingProcessEntity()
                                    {
                                        BatchKey = item.BatchKey,
                                        UserId = item.UserId,
                                        TweetId = item.TweetId,
                                        Action = action,
                                        LeadId = dbLead.Id,
                                        UserScreenName = lead.UserScreenName
                                    });

                                    break;
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

    }
}
