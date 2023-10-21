namespace TK.Paddle.Application.Contracts
{
    public interface IPaddleWebhookAppService
    {
        Task<string> HandleAlert(long alertId, string alertName, string raw);
    }
}