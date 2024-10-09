using Castle.Components.DictionaryAdapter.Xml;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace TK.Twitter.Crawl.Tweet.AirTable
{
    public class AirTableLead3Service : AirTableBaseService, ITransientDependency
    {
        private static readonly string BASE_ID = "appiCJiPg7QJ9XnmT";
        protected override string BaseId => BASE_ID;
    }
}
