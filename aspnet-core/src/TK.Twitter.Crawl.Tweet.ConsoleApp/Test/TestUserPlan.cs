using System;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;

namespace TK.Twitter.Crawl.ConsoleApp.Test
{
    public class TestUserPlan : ITransientDependency
    {
        private readonly IRepository<UserPlanEntity, Guid> _userPlanRepository;
        private readonly IEmailSender _emailSender;

        public TestUserPlan(IRepository<UserPlanEntity, Guid> userPlanRepository, IEmailSender emailSender)
        {
            _userPlanRepository = userPlanRepository;
            _emailSender = emailSender;
        }

        public async Task RunAsync()
        {
            try
            {
                await _emailSender.SendAsync("hoangphihai93@gmail.com",
                "[Lead3] You're Now a Premium Member",
                 $@"Hey,
<br />
<br />
Awesome news - you are now a Standard Member of Lead3! We&rsquo;re thrilled to have you on board.
<br />
<br />Here&rsquo;s what you need to do next:
<br />Set new password for your account: Follow this link to set password for your lead3 account hoangphihai93@gmail.com
<br />Link: <a href="""">Click here</a>
<br />Login to your account: After setting your password, login to your account using your new credentials
<br />Access the Lead Database: You'll find the leads database in your user page. Feel free to click on """"view larger version"""" if you want to use that data in Airtable.
<br /
><br />If you need any help using the list, have any questions, or wish to provide feedback and suggestions, please don&rsquo;t hesitate to reach out to our friendly Customer Service team.
<br />
<br />We&rsquo;ll send the product updates from this email address:
<br />contact @lead3.io
<br />
<br />To ensure our emails don't end up in your Spam folder, please reply with """"OK"""" to this email. It helps ensure you receive our updates. If this email ends up in your promotions or spam folder, kindly move it to your Primary inbox.
<br />
<br />Thank you for choosing Lead3. We're excited to be part of your journey in expanding your client and partner base.
<br />
<br />Your success is our success, and we encourage you to share your success stories with us if you win any deals from our leads.
<br />
<br />Best,
<br />The Lead3 team");
            }
            catch (Exception ex)
            {


            }
        }
    }
}
