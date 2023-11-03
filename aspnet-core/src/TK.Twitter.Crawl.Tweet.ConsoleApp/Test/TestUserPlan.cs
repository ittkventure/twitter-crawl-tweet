using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Jobs;
using TK.Twitter.Crawl.Tweet.Payment;
using TK.Twitter.Crawl.Tweet.TwitterAPI.Dto.FollowingCrawl;
using TK.TwitterAccount.Domain.Entities;
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
        private readonly AirTableManualSourceProcessWaitingJob _airTableManualSourceProcessWaitingJob;
        private readonly IRepository<TwitterAccountEntity, Guid> _twitterAccountRepository;

        public TestUserPlan(IRepository<UserPlanEntity, Guid> userPlanRepository, IEmailSender emailSender, PaddleAfterWebhookLogAddedHandler paddleAfterWebhookLogAddedHandler, AirTableManualSourceProcessWaitingJob airTableManualSourceProcessWaitingJob,
            IRepository<TwitterAccountEntity, Guid> twitterAccountRepository)
        {
            _userPlanRepository = userPlanRepository;
            _emailSender = emailSender;
            _paddleAfterWebhookLogAddedHandler = paddleAfterWebhookLogAddedHandler;
            _airTableManualSourceProcessWaitingJob = airTableManualSourceProcessWaitingJob;
            _twitterAccountRepository = twitterAccountRepository;
        }

        public async Task RunAsync()
        {
            try
            {
                var urls = GetUrls();
                var currentAcc = await _twitterAccountRepository.FirstOrDefaultAsync(x => x.Enabled == true);
                var list = new List<TwitterUserDto>();

                // https://twitter.com/qiaoyunzi1/status/1658420894946181120?s=20
                // https://twitter.com/traderrocko/status/1658108709644402699

                foreach (var item in urls)
                {
                    var data = await _airTableManualSourceProcessWaitingJob.GetTwitterUserAsync(item, currentAcc);
                    if (data != null)
                    {
                        list.Add(data);
                    }
                    else
                    {

                    }
                }


                var list1 = list.Select(x => new { Id = $"'{x.Id}", Name = x.Name, ScreenName = x.ScreenName }).ToList();

            }
            catch (Exception ex)
            {


            }
        }

        public async Task RunAsync_SendEmailWelcome()
        {
            try
            {
                await _paddleAfterWebhookLogAddedHandler.SendEmailWelCome("hoangphihai93@gmail.com", stdPlan: false, resetPwdToken: null);
            }
            catch (Exception ex)
            {


            }
        }

        private static List<string> GetUrls()
        {
            return new List<string>() {
                "https://twitter.com/jasmy_BNB","https://twitter.com/wolongkm","https://twitter.com/BddVenture","https://twitter.com/FungiAlpha","https://twitter.com/thedefiedge","https://twitter.com/doomsdart","https://twitter.com/Wuhuoqiu","https://twitter.com/qiaoyunzi1","https://twitter.com/huahuayjy","https://twitter.com/traderrocko","https://twitter.com/ViktorDefi","https://twitter.com/FatManWithGems","https://twitter.com/0xKofi","https://twitter.com/korpi87","https://twitter.com/thewolfofdefi","https://twitter.com/corleonescrypto","https://twitter.com/MauLu5630739070","https://twitter.com/peach_1340","https://twitter.com/tztokchad","https://twitter.com/CryptoDamus411","https://twitter.com/ZoomerOracle","https://twitter.com/CryptoGideon_","https://twitter.com/myBlockBrain","https://twitter.com/563defi","https://twitter.com/Hercules_Defi","https://twitter.com/francescoweb3","https://twitter.com/CryptoNikyous","https://twitter.com/PaikCapital","https://twitter.com/notnotstorm","https://twitter.com/quant_arb","https://twitter.com/HubertX13","https://twitter.com/RuggedWojak","https://twitter.com/apes_prologue","https://twitter.com/DefiIgnas","https://twitter.com/AlphaSeeker21","https://twitter.com/Gemhunter9000","https://twitter.com/alphascan_xyz","https://twitter.com/DegenSensei","https://twitter.com/0xSami_","https://twitter.com/IamMusteee","https://twitter.com/ChrisRomanoC","https://twitter.com/EskManso","https://twitter.com/QuantMeta","https://twitter.com/CryptoYusaku"
            };
        }
    }
}
