﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Tweet.AirTable;
using TK.Twitter.Crawl.Tweet.MemoryLock;
using TK.Twitter.Crawl.Tweet.TwitterAPI.Dto.FollowingCrawl;
using TK.Twitter.Crawl.TwitterAPI;
using TK.Twitter.Crawl.TwitterAPI.Dto;
using TK.TwitterAccount.Domain.Entities;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace TK.Twitter.Crawl.Jobs
{
    public class AirTableManualSourceProcessWaitingJobArg
    {

    }

    public class AirTableManualSourceProcessWaitingJob : AsyncBackgroundJob<AirTableManualSourceProcessWaitingJobArg>, ITransientDependency
    {
        private const string LOG_PREFIX = "[AirTableManualSourceProcessWaitingJob] ";
        public const int BATCH_SIZE = 10;

        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<AirTableWaitingProcessEntity, long> _airTableWaitingProcessRepository;
        private readonly IRepository<TwitterUserSignalEntity, long> _twitterUserSignalRepository;
        private readonly IRepository<AirTableManualSourceWaitingProcessEntity, long> _airTableManualSourceWaitingProcessRepository;
        private readonly IRepository<LeadWaitingProcessEntity, long> _leadWaitingProcessRepository;
        private readonly IRepository<AirTableManualSourceEntity, long> _airTableManualSourceRepository;
        private readonly IRepository<TwitterUserTypeEntity, long> _twitterUserTypeRepository;
        private readonly IRepository<TwitterUserStatusEntity, long> _twitterUserStatusRepository;
        private readonly IRepository<TwitterAccountEntity, Guid> _twitterAccountRepository;
        private readonly TwitterAPIUserService _twitterAPIUserService;
        private readonly AirTableLead3Manager _airTableManager;
        private readonly IClock _clock;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly MemoryLockProvider _memoryLockProvider;
        private readonly IRepository<LeadAnotherSourceEntity, long> _leadAnotherSourceRepository;

        public AirTableManualSourceProcessWaitingJob(
            IBackgroundJobManager backgroundJobManager,
            IRepository<AirTableWaitingProcessEntity, long> airTableWaitingProcessRepository,
            IRepository<TwitterUserSignalEntity, long> twitterUserSignalRepository,
            IRepository<AirTableManualSourceWaitingProcessEntity, long> airTableManualSourceWaitingProcessRepository,
            IRepository<LeadWaitingProcessEntity, long> leadWaitingProcessRepository,
            IRepository<AirTableManualSourceEntity, long> airTableManualSourceRepository,
            IRepository<TwitterUserTypeEntity, long> twitterUserTypeRepository,
            IRepository<TwitterUserStatusEntity, long> twitterUserStatusRepository,
            IRepository<TwitterAccountEntity, Guid> twitterAccountRepository,
            TwitterAPIUserService twitterAPIUserService,
            AirTableLead3Manager airTableManager,
            IClock clock,
            IUnitOfWorkManager unitOfWorkManager,
            MemoryLockProvider memoryLockProvider,
            IRepository<LeadAnotherSourceEntity, long> leadAnotherSourceRepository)
        {
            _backgroundJobManager = backgroundJobManager;
            _airTableWaitingProcessRepository = airTableWaitingProcessRepository;
            _twitterUserSignalRepository = twitterUserSignalRepository;
            _airTableManualSourceWaitingProcessRepository = airTableManualSourceWaitingProcessRepository;
            _leadWaitingProcessRepository = leadWaitingProcessRepository;
            _airTableManualSourceRepository = airTableManualSourceRepository;
            _twitterUserTypeRepository = twitterUserTypeRepository;
            _twitterUserStatusRepository = twitterUserStatusRepository;
            _twitterAccountRepository = twitterAccountRepository;
            _twitterAPIUserService = twitterAPIUserService;
            _airTableManager = airTableManager;
            _clock = clock;
            _unitOfWorkManager = unitOfWorkManager;
            _memoryLockProvider = memoryLockProvider;
            _leadAnotherSourceRepository = leadAnotherSourceRepository;
        }

        [UnitOfWork(IsDisabled = true)]
        public override async Task ExecuteAsync(AirTableManualSourceProcessWaitingJobArg args)
        {
            using (var handle = _memoryLockProvider.TryAcquireLock($"AirTableManualSourceProcessWaitingJob"))
            {
                if (handle == null)
                {
                    Logger.LogInformation(LOG_PREFIX + "Some process is running!!!");
                    return;
                }

                using var uow = _unitOfWorkManager.Begin();
                try
                {
                    var asyncExecuter = _airTableWaitingProcessRepository.AsyncExecuter;

                    var queueQuery = from input in await _airTableManualSourceWaitingProcessRepository.GetQueryableAsync()
                                     where input.Ended == false
                                     select input;

                    // xử lý theo chiều oldest -> lastest
                    queueQuery = queueQuery.OrderBy(x => x.CreationTime).Take(BATCH_SIZE);

                    var queues = await asyncExecuter.ToListAsync(queueQuery);
                    if (queues.IsEmpty())
                    {
                        return;
                    }

                    var currentAcc = await _twitterAccountRepository.FirstOrDefaultAsync(x => x.Enabled == true);

                    var records = await _airTableManualSourceRepository.GetListAsync(x => queues.Select(q => q.RecordId).Contains(x.RecordId));

                    foreach (var item in queues)
                    {
                        try
                        {
                            var record = records.FirstOrDefault(x => x.RecordId == item.RecordId);
                            if (record == null)
                            {
                                throw new Exception($"Record not found");
                            }

                            var recordSignals = record.Signals.Split(",").Distinct();

                            string signalCodes;

                            switch (item.Action)
                            {
                                case "CREATE":

                                    int count = 0;
                                    TwitterUserDto user = null;
                                    bool tooManyRequest = false;
                                    while (true)
                                    {
                                        count++;
                                        (tooManyRequest, user) = await GetTwitterUserAsync(record.ProjectTwitter, currentAcc);
                                 
                                        if (tooManyRequest)
                                        {
                                            currentAcc = await _twitterAccountRepository.FirstOrDefaultAsync(x => x.Enabled == true && x.AccountId != currentAcc.AccountId);
                                            (tooManyRequest, user) = await GetTwitterUserAsync(record.ProjectTwitter, currentAcc);
                                            if (!tooManyRequest)
                                            {
                                                if (user == null)
                                                {
                                                    throw new Exception($"Can not get User Id of {record.ProjectTwitter}");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (user == null)
                                            {
                                                throw new Exception($"Can not get User Id of {record.ProjectTwitter}");
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }

                                        if (count == 4)
                                        {
                                            break;
                                        }
                                    }

                                    if (user == null)
                                    {
                                        throw new Exception($"Can not get User Id of {record.ProjectTwitter}");
                                    }

                                    record.UserId = user.Id;
                                    record.UserName = user.Name;
                                    record.UserScreenName = user.ScreenName;

                                    await _airTableManualSourceRepository.UpdateAsync(record);

                                    signalCodes = string.Empty;
                                    foreach (var signal in recordSignals)
                                    {
                                        var signalCode = CrawlConsts.Signal.GetCode(signal);
                                        if (signalCode.IsEmpty())
                                        {
                                            continue;
                                        }

                                        if (signalCodes.IsNotEmpty())
                                        {
                                            signalCodes += ",";
                                        }

                                        signalCodes += signalCode;

                                        await _twitterUserSignalRepository.InsertAsync(new TwitterUserSignalEntity()
                                        {
                                            UserId = user.Id,
                                            Source = CrawlConsts.Signal.Source.AIR_TABLE_MANUAL_SOURCE,
                                            RefId = record.RecordId,
                                            Signal = signalCode
                                        });
                                    }

                                    // source từ manual source thì tự động sẽ là Lead. REF https://app.asana.com/0/1204183084369636/1205723036911441
                                    // nếu không có thì thêm mới
                                    var userType = await _twitterUserTypeRepository.FirstOrDefaultAsync(x => x.UserId == user.Id);
                                    if (userType == null)
                                    {
                                        await _twitterUserTypeRepository.InsertAsync(new TwitterUserTypeEntity()
                                        {
                                            UserId = user.Id,
                                            Type = CrawlConsts.LeadType.LEADS,
                                            IsUserSuppliedValue = false,
                                        }, autoSave: true);
                                    }

                                    // nếu không có thì thêm mới
                                    var userStatus = await _twitterUserStatusRepository.FirstOrDefaultAsync(x => x.UserId == user.Id);
                                    if (userStatus == null)
                                    {
                                        await _twitterUserStatusRepository.InsertAsync(new TwitterUserStatusEntity()
                                        {
                                            UserId = user.Id,
                                            Status = "New",
                                            IsUserSuppliedValue = false,
                                        }, autoSave: true);
                                    }

                                    await _leadAnotherSourceRepository.InsertAsync(new LeadAnotherSourceEntity()
                                    {
                                        RefId = item.RecordId,
                                        Source = CrawlConsts.Signal.Source.AIR_TABLE_MANUAL_SOURCE,
                                        UserId = user.Id,
                                        UserScreenName = user.ScreenName,
                                        UserName = user.Name,
                                        Signals = signalCodes,
                                        MediaMentioned = record.LastestSignalFrom,
                                        SignalUrl = record.LastestSignalUrl,
                                        Description = record.LastestSignalDescription,
                                        UpdatedAt = record.LastestSignalTime
                                    });

                                    await _leadWaitingProcessRepository.InsertAsync(new LeadWaitingProcessEntity()
                                    {
                                        BatchKey = "MANUAL_SOURCE",
                                        UserId = user.Id,
                                        RecordId = record.RecordId,
                                        Source = CrawlConsts.Signal.Source.AIR_TABLE_MANUAL_SOURCE
                                    });

                                    break;
                                case "UPDATE":

                                    // chỉ update signal, bỏ qua user type, user status

                                    signalCodes = string.Empty;

                                    var signals = await _twitterUserSignalRepository.GetListAsync(x => x.UserId == record.UserId && x.Source == CrawlConsts.Signal.Source.AIR_TABLE_MANUAL_SOURCE);
                                    foreach (var signal in recordSignals)
                                    {
                                        var signalCode = CrawlConsts.Signal.GetCode(signal);
                                        if (signalCode.IsEmpty())
                                        {
                                            continue;
                                        }

                                        if (signalCodes.IsNotEmpty())
                                        {
                                            signalCodes += ",";
                                        }

                                        signalCodes += signalCode;

                                        var alreadyExist = signals.Any(x => x.Signal == signalCode);
                                        if (alreadyExist)
                                        {
                                            continue;
                                        }

                                        await _twitterUserSignalRepository.InsertAsync(new TwitterUserSignalEntity()
                                        {
                                            UserId = record.UserId,
                                            Signal = signalCode,
                                            Source = CrawlConsts.Signal.Source.AIR_TABLE_MANUAL_SOURCE,
                                            RefId = record.RecordId,
                                        });
                                    }

                                    if (signalCodes.IsNotEmpty())
                                    {
                                        // chỉ update signal nên k cần thêm vào bảng another source

                                        //await _leadAnotherSourceRepository.InsertAsync(new LeadAnotherSourceEntity()
                                        //{
                                        //    RefId = item.RecordId,
                                        //    Source = CrawlConsts.Signal.Source.AIR_TABLE_MANUAL_SOURCE,
                                        //    UserId = record.UserId,
                                        //    UserScreenName = record.UserScreenName,
                                        //    UserName = record.UserName,
                                        //    Signals = signalCodes,
                                        //    MediaMentioned = record.LastestSignalFrom,
                                        //    SignalUrl = record.LastestSignalUrl,
                                        //    Description = record.LastestSignalDescription,
                                        //    UpdatedAt = record.LastestSignalTime
                                        //});
                                    }

                                    await _leadWaitingProcessRepository.InsertAsync(new LeadWaitingProcessEntity()
                                    {
                                        BatchKey = "MANUAL_SOURCE",
                                        UserId = record.UserId,
                                        RecordId = record.RecordId,
                                        Source = CrawlConsts.Signal.Source.AIR_TABLE_MANUAL_SOURCE
                                    });
                                    break;
                                default:
                                    break;
                            }

                            item.Succeed = true;
                        }
                        catch (Exception ex)
                        {
                            item.Note = ex.ToString();
                        }

                        item.Ended = true;
                        await _airTableManualSourceWaitingProcessRepository.UpdateAsync(item);
                    }

                    await uow.SaveChangesAsync();
                    await uow.CompleteAsync();
                }
                catch (Exception ex)
                {
                    await uow.RollbackAsync();
                    Logger.LogError(ex, LOG_PREFIX + "An error occurred while create/update data Airtable");
                }
            }

            await _backgroundJobManager.EnqueueAsync(args);
        }

        public async Task<(bool, TwitterUserDto)> GetTwitterUserAsync(string url, TwitterAccountEntity currentAcc)
        {
            bool tooManyRequest = false;
            TwitterUserDto user = null;

            string screenName;
            try
            {
                screenName = GetQueryParamAfterSlash(url);
            }
            catch
            {
                screenName = GetQueryParam(url);
            }

            if (screenName.IsEmpty())
            {
                return (tooManyRequest, user);
            }

            if (currentAcc == null)
            {
                return (tooManyRequest, user);
            }

            try
            {
                string jsonContentString;
                (tooManyRequest, jsonContentString) = await GetUserFromTwitterService(screenName, currentAcc.AccountId);
                if (tooManyRequest)
                {
                    return (tooManyRequest, user);
                }

                var jsonContent = JObject.Parse(jsonContentString);
                var data = jsonContent["data"];
                var userData = data["user"];

                if (userData == null)
                {
                    return (tooManyRequest, user);
                }

                if (userData["result"] == null)
                {
                    return (tooManyRequest, user);
                }

                if (userData["result"]["__typename"].ParseIfNotNull<string>() == "UserUnavailable")
                {
                    // User is suspended. Không lấy đc data trả về
                    return (tooManyRequest, user);
                }

                if (userData["result"]["legacy"] == null)
                {
                    return (tooManyRequest, user);
                }

                user = new TwitterUserDto()
                {
                    Id = userData["result"]["rest_id"].ParseIfNotNull<string>(),
                    Name = userData["result"]["legacy"]["name"].ParseIfNotNull<string>(),
                    ScreenName = userData["result"]["legacy"]["screen_name"].ParseIfNotNull<string>(),
                    Description = userData["result"]["legacy"]["description"].ParseIfNotNull<string>(),
                    ProfileImageUrl = userData["result"]["legacy"]["profile_image_url_https"].ParseIfNotNull<string>(),
                };

                string format = "ddd MMM dd HH:mm:ss zzzz yyyy";
                if (DateTime.TryParseExact(
                    userData["result"]["legacy"]["created_at"].ParseIfNotNull<string>(),
                    format,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime createdAt))
                {
                    user.CreatedAt = createdAt;
                }

                return (tooManyRequest, user);
            }
            catch
            {
                return (tooManyRequest, user);
            }
        }

        static string GetQueryParamAfterSlash(string url)
        {
            if (url.EndsWith("/"))
            {
                url = url.Substring(0, url.Length - 1);
            }

            Uri uri = new Uri(url);
            string path = uri.AbsolutePath;
            int slashIndex = path.LastIndexOf('/');
            if (slashIndex >= 0 && slashIndex < path.Length - 1)
            {
                string param = path.Substring(slashIndex + 1);
                return param;
            }
            else
            {
                return "";
            }
        }

        static string GetQueryParam(string url)
        {
            // Sử dụng Regex để lấy giá trị của query parameter
            // Bỏ qua giá trị đằng sau dấu ?
            Regex regex = new Regex(@"/([^/?]+)");
            Match match = regex.Match(url);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            // Trường hợp không tìm thấy
            return "Không tìm thấy";
        }

        private async Task<(bool, string)> GetUserFromTwitterService(string screenName, string accountId)
        {
            bool tooManyRequest = false;
            string jsonContent = null;

            Task delay(TimeSpan timeSpan)
            {
                return Task.Delay(timeSpan);
            }

            TwitterAPIUserGetUserResponse response = null;
            try
            {
                response = await _twitterAPIUserService.GetUserByScreenNameAsync(screenName, accountId);
                tooManyRequest = response.TooManyRequest;
                jsonContent = response.JsonContent;
                if (response.RateLimit > 0 || response.TooManyRequest)
                {
                    var subtract = response.RateLimitResetAt.Value.Subtract(_clock.Now);
                    if (response.RateLimitRemaining == 1)
                    {
                        Logger.LogInformation(LOG_PREFIX + "Delay in " + subtract);
                        await delay(subtract);
                    }
                }
            }
            catch (BusinessException ex)
            {
                if (ex.Code == CrawlDomainErrorCodes.TwitterAuthorizationError)
                {
                    // chỉ cho login lại 1 lần
                    response = await _twitterAPIUserService.GetUserByScreenNameAsync(screenName, accountId, requiredLogin: true);
                }
                else
                {
                    return (tooManyRequest, jsonContent);
                }
            }

            return (tooManyRequest, jsonContent);
        }
    }
}
