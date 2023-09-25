using AirtableApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace TK.Twitter.Crawl.Tweet.AirTable
{
    public class AirTableLead3Manager : DomainService
    {
        private readonly AirTableService _airTableService;
        private readonly IRepository<AirTableLeadRecordMappingEntity, long> _airTableLeadRecordMappingRepository;
        private readonly IRepository<TwitterUserEntity, long> _twitterUserRepository;
        public const string LEAD_TABLE_NAME = "Database_Sync_From_System";

        public AirTableLead3Manager(
            AirTableService airTableService,
            IRepository<AirTableLeadRecordMappingEntity, long> airTableLeadRecordMappingRepository,
            IRepository<TwitterUserEntity, long> twitterUserRepository)
        {
            _airTableService = airTableService;
            _airTableLeadRecordMappingRepository = airTableLeadRecordMappingRepository;
            _twitterUserRepository = twitterUserRepository;
        }

        public async Task<(bool, string)> AddLeadAsync(LeadEntity lead, TwitterUserEntity user)
        {
            bool succeed = false;

            var fields = new Fields();
            fields.FieldsCollection = GetAirTableLeadFields(lead, user);

            var response = await _airTableService.CreateMultipleRecords(LEAD_TABLE_NAME, new Fields[] {
                fields
            }, typecast: true);

            if (!response.Success)
            {
                string error;
                if (response.AirtableApiError is AirtableApiException)
                {
                    error = response.AirtableApiError.ErrorMessage;
                    if (response.AirtableApiError is AirtableInvalidRequestException)
                    {
                        error += response.AirtableApiError.DetailedErrorMessage;
                    }
                }
                else
                {
                    error = "Unknown error";
                }

                return (false, error);
            }

            string recordId = response.Records[0].Id;
            await _airTableLeadRecordMappingRepository.InsertAsync(new AirTableLeadRecordMappingEntity()
            {
                AirTableRecordId = recordId,
                ProjectUserId = lead.UserId
            }, autoSave: true);

            succeed = true;
            return (succeed, recordId);
        }

        public async Task<(bool, string)> UpdateLeadAsync(string recordId, LeadEntity lead, TwitterUserEntity user)
        {
            var fields = new IdFields(recordId);
            fields.FieldsCollection = GetAirTableLeadFields(lead, user);

            var response = await _airTableService.UpdateMultipleRecords(LEAD_TABLE_NAME, new IdFields[] {
                fields
            }, typecast: true);

            if (!response.Success)
            {
                string error;
                if (response.AirtableApiError is AirtableApiException)
                {
                    error = response.AirtableApiError.ErrorMessage;
                    if (response.AirtableApiError is AirtableInvalidRequestException)
                    {
                        error += response.AirtableApiError.DetailedErrorMessage;
                    }
                }
                else
                {
                    error = "Unknown error";
                }

                return (false, error);
            }

            bool succeed = true;
            return (succeed, recordId);
        }

        public async Task<string> BulkInsertLeadAsync(List<LeadEntity> leads, List<TwitterUserEntity> users)
        {
            var fields = leads.Select(lead =>
            {
                var user = users.FirstOrDefault(x => x.UserId == lead.UserId);
                var f = new Fields();
                f.FieldsCollection = GetAirTableLeadFields(lead, user);
                return f;
            });

            string error = null;
            var response = await _airTableService.CreateMultipleRecords(LEAD_TABLE_NAME, fields.ToArray(), typecast: true);
            if (!response.Success)
            {
                if (response.AirtableApiError is AirtableApiException)
                {
                    error = response.AirtableApiError.ErrorMessage;
                    if (response.AirtableApiError is AirtableInvalidRequestException)
                    {
                        error += response.AirtableApiError.DetailedErrorMessage;
                    }
                }
                else
                {
                    error = "Unknown error";
                }

                return (error);
            }

            var mappings = new List<AirTableLeadRecordMappingEntity>();
            foreach (var record in response.Records)
            {
                string leadId = record.GetField("System Lead Id").ToString();
                mappings.Add(new AirTableLeadRecordMappingEntity()
                {
                    AirTableRecordId = record.Id,
                    ProjectUserId = leadId
                });
            }

            await _airTableLeadRecordMappingRepository.InsertManyAsync(mappings);
            return error;
        }

        public async Task<string> BulkUpdateLeadAsync(List<LeadEntity> leads)
        {
            var mappings = await _airTableLeadRecordMappingRepository.GetListAsync(x => leads.Select(l => l.UserId).Contains(x.ProjectUserId));
            var users = await _twitterUserRepository.GetListAsync(x => leads.Select(l => l.UserId).Contains(x.UserId));

            var fields = from l in leads.AsQueryable()
                         join m in mappings.AsQueryable() on l.UserId equals m.ProjectUserId
                         into temp
                         from t in temp.DefaultIfEmpty()
                         join u in users.AsQueryable() on l.UserId equals u.UserId
                         into temp2
                         from t2 in temp2.DefaultIfEmpty()
                         select new IdFields(t.AirTableRecordId)
                         {
                             FieldsCollection = GetAirTableLeadFields(l, t2)
                         };

            string error = null;
            int take = 10;
            for (int skip = 0; skip < leads.Count; skip += take)
            {
                var response = await _airTableService.UpdateMultipleRecords(LEAD_TABLE_NAME, fields.Skip(skip).Take(take).ToArray(), typecast: true);
                if (!response.Success)
                {
                    if (response.AirtableApiError is AirtableApiException)
                    {
                        //error = response.AirtableApiError.ErrorMessage;
                        if (response.AirtableApiError is AirtableInvalidRequestException)
                        {
                            error += response.AirtableApiError.DetailedErrorMessage;
                        }
                    }
                    else
                    {
                        error = "Unknown error";
                    }
                }
            }

            return error;
        }

        public static Dictionary<string, object> GetAirTableLeadFields(LeadEntity lead, TwitterUserEntity user)
        {
            var dict = new Dictionary<string, object>()
            {
                { "Latest Signal HashTags", lead.HashTags },
                { "Type", lead.UserType },
                { "Lastest Tweet Id", lead.LastestTweetId },
                { "Lastest Signal Date", lead.LastestSponsoredDate },
                { "Latest Signal Description", lead.TweetDescription },
                { "Media Mentioned User Id", lead.TweetOwnerUserId },
                { "Latest signal from", lead.MediaMentioned },
                { "Duplicate Url Count", lead.DuplicateUrlCount },
                { "Latest Signal URL", lead.LastestSponsoredTweetUrl },
                { "Other Signals", lead.NumberOfSponsoredTweets },
                { "Twitter URL", lead.UserProfileUrl },
                { "System Lead Id", lead.UserId },
            };

            if (user != null)
            {
                dict.Add("Mentioned Twitter Bio", user.Description);
            }

            if (lead.Signals.IsNotEmpty())
            {
                var airTableSignals = new List<string>();
                foreach (var signal in lead.Signals.Split(","))
                {
                    switch (signal)
                    {
                        case "LISTING_CEX":
                            airTableSignals.Add("Listing cex");
                            break;
                        case "JUST_AUDITED":
                            airTableSignals.Add("Audit is completed");
                            break;
                        case "SPONSORED_TWEETS":
                            airTableSignals.Add("Buying sponsored ads");
                            break;
                        default:
                            break;
                    }
                }

                dict.Add("Signals", airTableSignals);
            }

            return dict;
        }
    }
}
