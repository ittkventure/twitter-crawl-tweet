using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;

namespace TK.Twitter.Crawl.Tweet.User
{
    [EventName("User.RegisterUser")]
    public class RegisterUserEto
    {
        public Guid UserId { get; set; }
    }

    public class RegisterUserHandler : IDistributedEventHandler<RegisterUserEto>, ITransientDependency
    {
        private readonly UserPlanManager _userPlanManager;

        public RegisterUserHandler(UserPlanManager userPlanManager)
        {
            _userPlanManager = userPlanManager;
        }

        public async Task HandleEventAsync(RegisterUserEto eventData)
        {
            await _userPlanManager.AddDefaultAsync(eventData.UserId);
        }
    }
}
