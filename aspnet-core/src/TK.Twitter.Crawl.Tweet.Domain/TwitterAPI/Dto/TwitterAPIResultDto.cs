using Newtonsoft.Json;

namespace TK.Twitter.Crawl.TwitterAPI.Dto
{
    public class TwitterAPIResultDto
    {
        [JsonProperty("__typename")]
        public string Typename { get; set; }

        [JsonProperty("timeline")]
        public TwitterAPITimelineDto Timeline { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("rest_id")]
        public string RestId { get; set; }

        [JsonProperty("affiliates_highlighted_label")]
        public TwitterAPIAffiliatesHighlightedLabelDto AffiliatesHighlightedLabel { get; set; }

        [JsonProperty("has_graduated_access")]
        public bool? HasGraduatedAccess { get; set; }

        [JsonProperty("is_blue_verified")]
        public bool? IsBlueVerified { get; set; }

        [JsonProperty("profile_image_shape")]
        public string ProfileImageShape { get; set; }

        [JsonProperty("legacy")]
        public TwitterAPILegacyDto Legacy { get; set; }

        [JsonProperty("professional")]
        public TwitterAPIProfessionalDto Professional { get; set; }

        [JsonProperty("has_nft_avatar")]
        public bool? HasNftAvatar { get; set; }
    }


}
