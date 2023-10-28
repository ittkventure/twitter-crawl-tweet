using System.Threading.Tasks;

namespace TK.Twitter.Crawl.Tweet
{
    public interface ICoinBaseWebhookAppService
    {
        Task<string> HandleAlert(string raw);
    }
}