using AirtableApiClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Tweet.AirTable;
using TK.Twitter.Crawl.Tweet.SerpApi;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TK.Twitter.Crawl.Tweet.GoogleNews
{
    public class GoogleNewsManager : DomainService
    {
        private readonly SerpApiClient _serpApiClient;
        private readonly AirTableGoogleNewsTrackerService _airTableService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<GoogleNewsRecordEntity, long> _googleNewsRecordRepository;
        private readonly IRepository<GoogleNewsAirTableRecordEntity, long> _googleNewsAirTableRecordRepository;
        private readonly IRepository<GoogleNewsWaitingProcessEntity, long> _googleNewsWaitingProcessRepository;

        public GoogleNewsManager(
            SerpApiClient serpApiClient,
            AirTableGoogleNewsTrackerService airTableService,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<GoogleNewsRecordEntity, long> googleNewsRecordRepository,
            IRepository<GoogleNewsAirTableRecordEntity, long> googleNewsAirTableRecordRepository,
            IRepository<GoogleNewsWaitingProcessEntity, long> googleNewsWaitingProcessRepository)
        {
            _serpApiClient = serpApiClient;
            _airTableService = airTableService;
            _unitOfWorkManager = unitOfWorkManager;
            _googleNewsRecordRepository = googleNewsRecordRepository;
            _googleNewsAirTableRecordRepository = googleNewsAirTableRecordRepository;
            _googleNewsWaitingProcessRepository = googleNewsWaitingProcessRepository;
        }

        public const string KeywordTableName = "Keyword";
        public const string NewsTableName = "News Database";

        public async Task<List<string>> GetKeywordsAsync()
        {
            var keywords = await _airTableService.GetAll<Dictionary<string, string>>(KeywordTableName, fields: new List<string> { "Name" });
            if (keywords.Count == 0)
            {
                return null;
            }

            return keywords.Select(rc => rc.Fields["Name"]).ToList();
        }

        public async Task CrawlAsync()
        {
            var keywords = await GetKeywordsAsync();

            if (keywords == null)
            {
                return;
            }

            foreach (var keyword in keywords)
            {
                for (var start = 1; start <= 10; start++)
                {
                    var (_, articles) = await _serpApiClient.SearchByKeywordAsync(keyword, start);
                    if (articles == null)
                    {
                        break;
                    }

                    var uow1 = _unitOfWorkManager.Begin(requiresNew: true);
                    try
                    {
                        foreach (var article in articles)
                        {
                            if (await _googleNewsRecordRepository.AnyAsync(x => x.Link == article.Link))
                            {
                                continue;
                            }

                            var googleNewsRecord = new GoogleNewsRecordEntity
                            {
                                Link = article.Link,
                                Title = article.Title,
                                Source = article.Source,
                                Date = article.Date,
                                DateValue = article.DateValue,
                                Snippet = article.Snippet,
                                Thumbnail = article.Thumbnail,
                                CreatedAt = DateTime.Now
                            };

                            await _googleNewsRecordRepository.InsertAsync(googleNewsRecord);
                            await _googleNewsWaitingProcessRepository.InsertAsync(new GoogleNewsWaitingProcessEntity
                            {
                                SourceName = googleNewsRecord.Source,
                                StatusId = 0
                            });
                        }

                        await uow1.CompleteAsync();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex);
                    }
                }
            }
        }

        public async Task SyncAirTableAsync()
        {
            var waitingProcesses = await AsyncExecuter.ToListAsync(

                (from p in await _googleNewsWaitingProcessRepository.GetQueryableAsync()
                 where p.StatusId == 0
                 orderby p.CreationTime
                 select p).Take(50)

                );

            if (waitingProcesses.IsEmpty())
            {
                return;
            }

            var newRecords = new List<Fields>();
            var updateRecords = new List<IdFields>();
            foreach (var process in waitingProcesses)
            {
                var query = from r in await _googleNewsRecordRepository.GetQueryableAsync()
                            where r.Source == process.SourceName
                            orderby r.CreatedAt descending
                            select r;
                process.Attempt++;
                try
                {
                    var lastest = await AsyncExecuter.FirstOrDefaultAsync(query);
                    if (lastest == null)
                    {
                        process.StatusId = -1;
                        await _googleNewsWaitingProcessRepository.UpdateAsync(process);
                        continue;
                    }

                    var record = await _googleNewsAirTableRecordRepository.FirstOrDefaultAsync(x => x.Source == process.SourceName);
                    if (record == null)
                    {
                        var fields = new Fields();
                        fields.FieldsCollection = new Dictionary<string, object>
                        {
                            { "Source Name", lastest.Source },
                            { "Link", lastest.Link },
                            { "Title", lastest.Title },
                            { "Keyword", lastest.Snippet },
                            { "Crawling Date", lastest.CreationTime },
                            { "Total Articles", await AsyncExecuter.CountAsync(query) },
                        };

                        newRecords.Add(fields);
                    }
                    else
                    {
                        var fields = new IdFields(record.RecordId);
                        fields.FieldsCollection = new Dictionary<string, object>
                        {
                            { "Source Name", lastest.Source },
                            { "Link", lastest.Link },
                            { "Title", lastest.Title },
                            { "Keyword", lastest.Snippet },
                            { "Crawling Date", lastest.CreationTime },
                            { "Total Articles", await AsyncExecuter.CountAsync(query) },
                        };
                    }

                    process.StatusId = 1;
                }
                catch (Exception ex)
                {
                    process.Note = ex.Message;
                    Logger.LogException(ex);
                }

                if (process.Attempt == 2)
                {
                    process.StatusId = -1;
                }

                await _googleNewsWaitingProcessRepository.UpdateAsync(process);
            }

            if (newRecords.IsNotEmpty())
            {
                for (int skip = 0; skip < newRecords.Count; skip += 10)
                {
                    var fields = newRecords.Skip(skip).Take(10);
                    try
                    {
                        var response = await _airTableService.CreateMultipleRecords(NewsTableName, fields.ToArray(), typecast: true);
                        if (response.Success)
                        {
                            var newMappings = response.Records.Select(x => new GoogleNewsAirTableRecordEntity()
                            {
                                RecordId = x.Id,
                                Source = x.GetField("Source Name").ToString()
                            });

                            await _googleNewsAirTableRecordRepository.InsertManyAsync(newMappings);
                        }
                    }
                    catch { }
                }
            }

            if (updateRecords.IsNotEmpty())
            {
                try
                {
                    for (int skip = 0; skip < updateRecords.Count; skip += 10)
                    {
                        var fields = updateRecords.Skip(skip).Take(10);
                        await _airTableService.UpdateMultipleRecords(NewsTableName, fields.ToArray(), typecast: true);
                    }
                }
                catch { }
            }
        }
    }
}
