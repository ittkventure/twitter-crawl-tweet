using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace TK.Twitter.Crawl.ConsoleApp.Test
{
    public class ExportTweetReport : ITransientDependency
    {
        private readonly IRepository<TwitterTweetEntity, long> _twitterTweetRepository;

        public ExportTweetReport(IRepository<TwitterTweetEntity, long> twitterTweetRepository)
        {
            _twitterTweetRepository = twitterTweetRepository;
        }

        public async Task Run()
        {
            try
            {
                var tweets = await _twitterTweetRepository.GetListAsync();

                var entries = new List<Entry>();
                foreach (var tweet in tweets)
                {
                    var entry = new Entry
                    {
                        UserId = tweet.UserId,
                        TweetId = tweet.TweetId,
                        ViewsCount = tweet.ViewsCount,
                        CreatedAt = tweet.CreatedAt,
                        FullText = tweet.FullText,
                        BookmarkCount = tweet.BookmarkCount,
                        ConversationId = tweet.ConversationId,
                        FavoriteCount = tweet.FavoriteCount,
                        InReplyToScreenName = tweet.InReplyToScreenName,
                        InReplyToStatusId = tweet.InReplyToStatusId,
                        InReplyToUserId = tweet.InReplyToUserId,
                        IsQuoteStatus = tweet.IsQuoteStatus,
                        Lang = tweet.Lang,
                        QuoteCount = tweet.QuoteCount,
                        ReplyCount = tweet.ReplyCount,
                        RetweetCount = tweet.RetweetCount,
                    };

                    entries.Add(entry);

                    //if (tweet.QuoteStatusResultAsJson.IsNotEmpty())
                    //{
                    //    JObject jObj = JObject.Parse(tweet.QuoteStatusResultAsJson);

                    //    entry.QuoteStatusResult_TweetId = jObj["result"]["rest_id"].ParseIfNotNull<string>();
                    //    entry.QuoteStatusResult_ViewCount = jObj["result"]["views"]["count"].ParseIfNotNull<int>();
                    //    entry.QuoteStatusResult_CreatedAt = jObj["result"]["legacy"]["created_at"].ParseIfNotNull<string>();
                    //    entry.QuoteStatusResult_BookmarkCount = jObj["result"]["legacy"]["bookmark_count"].ParseIfNotNull<int>();
                    //    entry.QuoteStatusResult_FavoriteCount = jObj["result"]["legacy"]["favorite_count"].ParseIfNotNull<int>();
                    //    entry.QuoteStatusResult_QuoteCount = jObj["result"]["legacy"]["quote_count"].ParseIfNotNull<int>();
                    //    entry.QuoteStatusResult_ReplyCount = jObj["result"]["legacy"]["reply_count"].ParseIfNotNull<int>();
                    //    entry.QuoteStatusResult_RetweetCount = jObj["result"]["legacy"]["retweet_count"].ParseIfNotNull<int>();
                    //    entry.QuoteStatusResult_FullText = jObj["result"]["legacy"]["full_text"].ParseIfNotNull<string>();
                    //    entry.QuoteStatusResult_IsQuoteStatus = jObj["result"]["legacy"]["is_quote_status"].ParseIfNotNull<bool>();
                    //    entry.QuoteStatusResult_Lang = jObj["result"]["legacy"]["lang"].ParseIfNotNull<string>();
                    //    entry.QuoteStatusResult_InReplyToScreenName = jObj["result"]["legacy"]["in_reply_to_screen_name"].ParseIfNotNull<string>();
                    //    entry.QuoteStatusResult_InReplyToStatusId = jObj["result"]["legacy"]["in_reply_to_status_id_str"].ParseIfNotNull<string>();
                    //    entry.QuoteStatusResult_InReplyToUserId = jObj["result"]["legacy"]["in_reply_to_user_id_str"].ParseIfNotNull<string>();
                    //    entry.QuoteStatusResult_ConversationId = jObj["result"]["legacy"]["conversation_id_str"].ParseIfNotNull<string>();

                    //    var userResult = jObj["result"]["core"]["user_results"]["result"];
                    //    entry.QuoteStatusResult_UserResult_UserId = userResult["rest_id"].ParseIfNotNull<string>();
                    //    entry.QuoteStatusResult_UserResult_Name = userResult["legacy"]["name"].ParseIfNotNull<string>();
                    //    entry.QuoteStatusResult_UserResult_ScreenName = userResult["legacy"]["screen_name"].ParseIfNotNull<string>();
                    //    entry.QuoteStatusResult_UserResult_CreatedAt = userResult["legacy"]["created_at"].ParseIfNotNull<string>();
                    //    entry.QuoteStatusResult_UserResult_ProfileBannerUrl = userResult["legacy"]["profile_banner_url"].ParseIfNotNull<string>();
                    //    entry.QuoteStatusResult_UserResult_ProfileImageUrl = userResult["legacy"]["profile_image_url_https"].ParseIfNotNull<string>();
                    //    entry.QuoteStatusResult_UserResult_FastFollowerCount = userResult["legacy"]["fast_followers_count"].ParseIfNotNull<int>();
                    //    entry.QuoteStatusResult_UserResult_FavouritesCount = userResult["legacy"]["favourites_count"].ParseIfNotNull<int>();
                    //    entry.QuoteStatusResult_UserResult_FollowersCount = userResult["legacy"]["followers_count"].ParseIfNotNull<int>();
                    //    entry.QuoteStatusResult_UserResult_MediaCount = userResult["legacy"]["media_count"].ParseIfNotNull<int>();
                    //    entry.QuoteStatusResult_UserResult_ListedCount = userResult["legacy"]["listed_count"].ParseIfNotNull<int>();
                    //    entry.QuoteStatusResult_UserResult_NormalFollowersCount = userResult["legacy"]["normal_followers_count"].ParseIfNotNull<int>();
                    //    entry.QuoteStatusResult_UserResult_StatusesCount = userResult["legacy"]["statuses_count"].ParseIfNotNull<int>();
                    //    entry.QuoteStatusResult_UserResult_Location = userResult["legacy"]["location"].ParseIfNotNull<string>();

                    //    entry.QuoteStatusResult_NoteTweet_Text = jObj["result"]?["note_tweet"]?["note_tweet_results"]?["result"]?["text"].ParseIfNotNull<string>();
                    //}

                    //if (tweet.UserResultAsJson.IsNotEmpty())
                    //{
                    //    JObject jObj = JObject.Parse(tweet.UserResultAsJson);

                    //    entry.UserResult_Name = jObj["name"].ParseIfNotNull<string>();
                    //    entry.UserResult_ScreenName = jObj["screen_name"].ParseIfNotNull<string>();
                    //    entry.UserResult_ProfileBannerUrl = jObj["profile_banner_url"].ParseIfNotNull<string>();
                    //    entry.UserResult_ProfileImageUrl = jObj["profile_image_url_https"].ParseIfNotNull<string>();
                    //    entry.UserResult_CreatedAt = jObj["created_at"].ParseIfNotNull<string>();
                    //    entry.UserResult_EntitiesJson = JsonHelper.Stringify(jObj["entities"]);
                    //    entry.UserResult_Location = jObj["location"].ParseIfNotNull<string>();

                    //    entry.UserResult_FastFollowerCount = jObj["fast_followers_count"].ParseIfNotNull<int>();
                    //    entry.UserResult_FavouritesCount = jObj["favourites_count"].ParseIfNotNull<int>();
                    //    entry.UserResult_FollowersCount = jObj["followers_count"].ParseIfNotNull<int>();
                    //    entry.UserResult_ListedCount = jObj["listed_count"].ParseIfNotNull<int>();
                    //    entry.UserResult_MediaCount = jObj["media_count"].ParseIfNotNull<int>();
                    //    entry.UserResult_NormalFollowersCount = jObj["normal_followers_count"].ParseIfNotNull<int>();
                    //    entry.UserResult_StatusesCount = jObj["statuses_count"].ParseIfNotNull<int>();
                    //}

                    //if (tweet.EntitiesAsJson.IsNotEmpty())
                    //{
                    //    JObject jObj = JObject.Parse(tweet.EntitiesAsJson);

                    //    if (jObj["media"] != null)
                    //    {
                    //        foreach (var item in jObj["media"])
                    //        {
                    //            var mediaUrl = item["media_url_https"].ParseIfNotNull<string>();
                    //            var type = item["type"].ParseIfNotNull<string>();
                    //            var expanded_url = item["expanded_url"].ParseIfNotNull<string>();
                    //            var display_url = item["display_url"].ParseIfNotNull<string>();
                    //            var url = item["url"].ParseIfNotNull<string>();

                    //            if (mediaUrl.IsNotEmpty())
                    //            {
                    //                entry.Entities_Media += $"Type: {type}. Media URL: {mediaUrl}" + Environment.NewLine;
                    //            }
                    //        }
                    //    }

                    //    if (jObj["user_mentions"] != null)
                    //    {
                    //        foreach (var item in jObj["user_mentions"])
                    //        {
                    //            var id_str = item["id_str"].ParseIfNotNull<string>();
                    //            var name = item["name"].ParseIfNotNull<string>();
                    //            var screen_name = item["screen_name"].ParseIfNotNull<string>();

                    //            entry.Entities_UserMention += $"ID: {id_str}. Name: {name}. Username: {screen_name}" + Environment.NewLine;
                    //        }
                    //    }

                    //    if (jObj["symbols"] != null)
                    //    {
                    //        foreach (var item in jObj["symbols"])
                    //        {
                    //            var text = item["text"].ParseIfNotNull<string>();
                    //            entry.Entities_UserMention += $"{text};";
                    //        }
                    //    }

                    //    if (jObj["hashtags"] != null)
                    //    {
                    //        foreach (var item in jObj["hashtags"])
                    //        {
                    //            var text = item["text"].ParseIfNotNull<string>();
                    //            entry.Entities_HashTags += $"{text};";
                    //        }
                    //    }

                    //    if (jObj["urls"] != null)
                    //    {
                    //        foreach (var item in jObj["urls"])
                    //        {
                    //            var display_url = item["display_url"].ParseIfNotNull<string>();
                    //            var expanded_url = item["expanded_url"].ParseIfNotNull<string>();
                    //            var url = item["url"].ParseIfNotNull<string>();
                    //            entry.Entities_Urls += $"Url: {url}" + Environment.NewLine;
                    //        }
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {

            }
        }

        private record Entry
        {
            public string UserId { get; set; }
            public string TweetId { get; set; }
            public int? ViewsCount { get; set; }
            public DateTime? CreatedAt { get; set; }
            public int BookmarkCount { get; set; }
            public int FavoriteCount { get; set; }
            public int QuoteCount { get; set; }
            public int ReplyCount { get; set; }
            public int RetweetCount { get; set; }
            public string FullText { get; set; }
            public bool IsQuoteStatus { get; set; }
            public string Lang { get; set; }
            public string InReplyToScreenName { get; set; }
            public string InReplyToStatusId { get; set; }
            public string InReplyToUserId { get; set; }
            public string ConversationId { get; set; }

            public string UserResult_Name { get; set; }
            public string UserResult_ProfileBannerUrl { get; set; }
            public string UserResult_ProfileImageUrl { get; set; }
            public string UserResult_ScreenName { get; set; }
            public string UserResult_CreatedAt { get; set; }
            public string UserResult_EntitiesJson { get; set; }
            public int UserResult_FastFollowerCount { get; set; }
            public int UserResult_FavouritesCount { get; set; }
            public int UserResult_FollowersCount { get; set; }
            public int UserResult_ListedCount { get; set; }
            public string UserResult_Location { get; set; }
            public int UserResult_MediaCount { get; set; }
            public int UserResult_NormalFollowersCount { get; set; }
            public int UserResult_StatusesCount { get; set; }

            public string Entities_Media { get; set; }
            public string Entities_UserMention { get; set; }
            public string Entities_Urls { get; set; }
            public string Entities_HashTags { get; set; }
            public string Entities_Symbols { get; set; }

            public string QuoteStatusResult_TweetId { get; set; }
            public string QuoteStatusResult_UserResult_UserId { get; set; }
            public string QuoteStatusResult_UserResult_Name { get; set; }
            public string QuoteStatusResult_UserResult_ProfileBannerUrl { get; set; }
            public string QuoteStatusResult_UserResult_ProfileImageUrl { get; set; }
            public string QuoteStatusResult_UserResult_ScreenName { get; set; }
            public string QuoteStatusResult_UserResult_CreatedAt { get; set; }
            public int QuoteStatusResult_UserResult_FastFollowerCount { get; set; }
            public int QuoteStatusResult_UserResult_FavouritesCount { get; set; }
            public int QuoteStatusResult_UserResult_FollowersCount { get; set; }
            public int QuoteStatusResult_UserResult_ListedCount { get; set; }
            public string QuoteStatusResult_UserResult_Location { get; set; }
            public int QuoteStatusResult_UserResult_MediaCount { get; set; }
            public int QuoteStatusResult_UserResult_NormalFollowersCount { get; set; }
            public int QuoteStatusResult_UserResult_StatusesCount { get; set; }
            public int? QuoteStatusResult_ViewCount { get; set; }
            public string QuoteStatusResult_NoteTweet_Text { get; set; }
            public string QuoteStatusResult_CreatedAt { get; set; }
            public int QuoteStatusResult_BookmarkCount { get; set; }
            public int QuoteStatusResult_FavoriteCount { get; set; }
            public int QuoteStatusResult_QuoteCount { get; set; }
            public int QuoteStatusResult_ReplyCount { get; set; }
            public int QuoteStatusResult_RetweetCount { get; set; }
            public string QuoteStatusResult_FullText { get; set; }
            public bool QuoteStatusResult_IsQuoteStatus { get; set; }
            public string QuoteStatusResult_Lang { get; set; }
            public string QuoteStatusResult_InReplyToScreenName { get; set; }
            public string QuoteStatusResult_InReplyToStatusId { get; set; }
            public string QuoteStatusResult_InReplyToUserId { get; set; }
            public string QuoteStatusResult_ConversationId { get; set; }
        }
    }
}
