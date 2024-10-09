using AirtableApiClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;

namespace TK.Twitter.Crawl.Tweet.AirTable
{
    public abstract class AirTableBaseService
    {
        private static readonly string API_KEY = "pataLMlaFRRtgJWWB.a8c1fb86baccd465acd5b91cbdd40c1756dd298b8696a541fa08524f2929cd47";
        protected virtual string BaseId { get; set; }

        private readonly AirtableBase airTableBase;

        public AirTableBaseService()
        {
            airTableBase = new AirtableBase(API_KEY, BaseId);
        }

        public async Task<List<AirtableRecord<T>>> GetAll<T>(string tableIdOrName, IEnumerable<string> fields = null) where T : new()
        {
            var result = new List<AirtableRecord<T>>();
            string offset = null;
            string errorMessage = null;
            do
            {
                var response = await GetPaged<T>(tableIdOrName, offset, fields: fields, pageSize: 100);
                if (response.Success)
                {
                    result.AddRange(response.Records);
                    offset = response.Offset;
                }
                else if (response.AirtableApiError is AirtableApiException)
                {
                    errorMessage = response.AirtableApiError.ErrorMessage;
                    if (response.AirtableApiError is AirtableInvalidRequestException)
                    {
                        errorMessage += response.AirtableApiError.DetailedErrorMessage;
                    }
                    break;
                }
                else
                {
                    errorMessage = "Unknown error";
                    break;
                }
            }
            while (offset != null);

            if (errorMessage.IsNotEmpty())
            {
                throw new BusinessException(CrawlDomainErrorCodes.InsideLogicError, errorMessage);
            }

            return result;
        }

        public async Task<AirtableListRecordsResponse<T>> GetPaged<T>(
            string tableIdOrName,
            string offset = null,
            IEnumerable<string> fields = null,
            string filterByFormula = null,
            int? maxRecords = null,
            int? pageSize = null,
            IEnumerable<Sort> sort = null,
            string view = null,
            string cellFormat = null,
            string timeZone = null,
            string userLocale = null,
            bool returnFieldsByFieldId = false,
            bool? includeCommentCount = null) where T : new()
        {
            var response = await airTableBase.ListRecords<T>(tableIdOrName, offset, fields, filterByFormula, maxRecords, pageSize, sort, view, cellFormat, timeZone, userLocale, returnFieldsByFieldId, includeCommentCount);
            return response;
        }

        public async Task<AirtableCreateUpdateReplaceMultipleRecordsResponse> CreateMultipleRecords(string tableIdOrName, Fields[] fields, bool typecast = false)
        {
            var response = await airTableBase.CreateMultipleRecords(tableIdOrName, fields, typecast);
            return response;
        }

        public async Task<AirtableCreateUpdateReplaceMultipleRecordsResponse> UpdateMultipleRecords(string tableIdOrName, IdFields[] idFields, bool typecast = false, bool returnFieldsByFieldId = false, PerformUpsert performUpsert = null)
        {
            var response = await airTableBase.UpdateMultipleRecords(tableIdOrName, idFields, typecast, returnFieldsByFieldId, performUpsert);
            return response;
        }

        public async Task<AirtableDeleteRecordResponse> DeleteAsync(string tableIdOrName, string recordId)
        {
            var response = await airTableBase.DeleteRecord(tableIdOrName, recordId);
            return response;
        }
    }
}
