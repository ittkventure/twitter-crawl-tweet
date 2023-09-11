using Newtonsoft.Json;
using System.Collections.Generic;

namespace TK.Twitter.Crawl.TwitterAPI.Dto
{
    public class TwitterAPIProfessionalDto
    {
        [JsonProperty("rest_id")]
        public string RestId { get; set; }

        [JsonProperty("professional_type")]
        public string ProfessionalType { get; set; }

        [JsonProperty("category")]
        public List<TwitterAPICategoryDto> Category { get; set; }
    }
}
