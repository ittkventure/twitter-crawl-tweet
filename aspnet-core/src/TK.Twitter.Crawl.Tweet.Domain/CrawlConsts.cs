namespace TK.Twitter.Crawl;

public static class CrawlConsts
{
    public const string DbTablePrefix = "App";

    public const string DbSchema = null;

    public static class ChangeLogType
    {
        public const string USER_URL_ADDED = "USER_URL_ADDED";
        public const string USER_URL_REMOVED = "USER_URL_REMOVED";
    }

    public static class UrlType
    {
        public const string INFO_URL = "INFO_URL";
        public const string DESCIPTION_URL = "DESCIPTION_URL";
    }

    public static class DataChangeType
    {
        public const string TWITTER_USER_NAME = "Name";
        public const string TWITTER_USER_SCREEN_NAME = "ScreenName";
        public const string TWITTER_USER_DESCRIPTION = "Description";
        public const string TWITTER_USER_PROFILE_IMAGE_URL = "ProfileImageUrl";
        public const string TWITTER_USER_URL = "Urls";
        public const string TWITTER_USER_STATUS = "Status";
    }
}
