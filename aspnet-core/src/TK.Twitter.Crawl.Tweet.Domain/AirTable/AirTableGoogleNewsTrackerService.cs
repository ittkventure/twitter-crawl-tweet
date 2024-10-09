using Volo.Abp.DependencyInjection;

namespace TK.Twitter.Crawl.Tweet.AirTable
{
    public class AirTableGoogleNewsTrackerService : AirTableBaseService, ITransientDependency
    {
        private static readonly string BASE_ID = "appKUsc4buZ1aISzY";
        protected override string BaseId => BASE_ID;
    }
}
