using System.Threading.Tasks;

namespace TK.Twitter.Crawl.Tweet
{
    public interface IAirTableWebhookAppService
    {
        Task<string> HandleAlert(string raw);
    }
}