using System;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Tweet.Payment;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;

namespace TK.Twitter.Crawl.ConsoleApp.Test
{
    public class TestUserPlan : ITransientDependency
    {
        private readonly IRepository<UserPlanEntity, Guid> _userPlanRepository;
        private readonly IEmailSender _emailSender;
        private readonly PaddleAfterWebhookLogAddedHandler _paddleAfterWebhookLogAddedHandler;

        public TestUserPlan(IRepository<UserPlanEntity, Guid> userPlanRepository, IEmailSender emailSender, PaddleAfterWebhookLogAddedHandler paddleAfterWebhookLogAddedHandler)
        {
            _userPlanRepository = userPlanRepository;
            _emailSender = emailSender;
            _paddleAfterWebhookLogAddedHandler = paddleAfterWebhookLogAddedHandler;
        }

        public async Task RunAsync()
        {
            try
            {
                await _paddleAfterWebhookLogAddedHandler.SendEmailWelCome("hoangphihai93@gmail.com", stdPlan: false, resetPwdToken: null);
            }
            catch (Exception ex)
            {


            }
        }
    }
}
