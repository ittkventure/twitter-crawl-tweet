using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TK.Twitter.Crawl.TwitterAPI.Dto
{
    public class TwitterAPILegacyDto
    {
        [JsonProperty("blocking")]
        public bool? Blocking { get; set; }

        [JsonProperty("can_dm")]
        public bool? CanDm { get; set; }

        [JsonProperty("can_media_tag")]
        public bool? CanMediaTag { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("default_profile")]
        public bool? DefaultProfile { get; set; }

        [JsonProperty("default_profile_image")]
        public bool? DefaultProfileImage { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("entities")]
        public TwitterAPIEntitiesDto Entities { get; set; }

        [JsonProperty("fast_followers_count")]
        public int? FastFollowersCount { get; set; }

        [JsonProperty("favourites_count")]
        public int? FavouritesCount { get; set; }

        [JsonProperty("followers_count")]
        public int? FollowersCount { get; set; }

        [JsonProperty("friends_count")]
        public int? FriendsCount { get; set; }

        [JsonProperty("has_custom_timelines")]
        public bool? HasCustomTimelines { get; set; }

        [JsonProperty("is_translator")]
        public bool? IsTranslator { get; set; }

        [JsonProperty("listed_count")]
        public int? ListedCount { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("media_count")]
        public int? MediaCount { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("normal_followers_count")]
        public int? NormalFollowersCount { get; set; }

        [JsonProperty("pinned_tweet_ids_str")]
        public List<string> PinnedTweetIdsStr { get; set; }

        [JsonProperty("possibly_sensitive")]
        public bool? PossiblySensitive { get; set; }

        [JsonProperty("profile_image_url_https")]
        public string ProfileImageUrlHttps { get; set; }

        [JsonProperty("profile_interstitial_type")]
        public string ProfileInterstitialType { get; set; }

        [JsonProperty("screen_name")]
        public string ScreenName { get; set; }

        [JsonProperty("statuses_count")]
        public int? StatusesCount { get; set; }

        [JsonProperty("translator_type")]
        public string TranslatorType { get; set; }

        [JsonProperty("verified")]
        public bool? Verified { get; set; }

        [JsonProperty("want_retweets")]
        public bool? WantRetweets { get; set; }

        [JsonProperty("withheld_in_countries")]
        public List<object> WithheldInCountries { get; set; }

        [JsonProperty("profile_banner_url")]
        public string ProfileBannerUrl { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("verified_type")]
        public string VerifiedType { get; set; }
    }


}
