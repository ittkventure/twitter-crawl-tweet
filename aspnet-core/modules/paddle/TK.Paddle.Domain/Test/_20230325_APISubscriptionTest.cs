using System.Threading.Tasks;
using TK.Paddle.Client.APIService.Subscription;
using Volo.Abp.Domain.Services;

namespace TK.Paddle.Domain.Test
{
    public class _20230325_APISubscriptionTest : DomainService
    {
        private readonly IPaddleSubscriptionAPIService _paddleSubscriptionAPIService;

        public _20230325_APISubscriptionTest(IPaddleSubscriptionAPIService paddleSubscriptionAPIService)
        {
            _paddleSubscriptionAPIService = paddleSubscriptionAPIService;
        }

        public async Task Test()
        {
            try
            {
                var response1 = await _paddleSubscriptionAPIService.ListPlansAsync(planId: 820318);
                var response2 = await _paddleSubscriptionAPIService.ListUsersAsync();

                var user = response2.Response[0];

                var response3 = await _paddleSubscriptionAPIService.UpdateUserAsync(user.SubscriptionId, currency: "USD", recurringPrice: 14M, planId: user.PlanId);
            }
            catch (System.Exception ex)
            {

            }
        }
    }
}
