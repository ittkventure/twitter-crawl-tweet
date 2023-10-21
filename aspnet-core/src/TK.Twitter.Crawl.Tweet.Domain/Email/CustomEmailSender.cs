using Microsoft.Extensions.Logging;
using System;
using System.Net.Mail;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing.Smtp;
using Volo.Abp.Emailing;
using System.Linq;
using System.Collections.Generic;

namespace TK.Twitter.Crawl.Tweet.Email
{
    public class CustomEmailSender : EmailSenderBase, IEmailSender
    {
        private readonly ISmtpEmailSender _smtpEmailSender;
        private readonly IRepository<EmailLogEntity, long> _emailLogRepository;

        public CustomEmailSender(IEmailSenderConfiguration configuration,
                                 IBackgroundJobManager backgroundJobManager,
                                 ISmtpEmailSender smtpEmailSender,
                                 IRepository<EmailLogEntity, long> emailLogRepository,
                                 ILogger<CustomEmailSender> logger) : base(configuration, backgroundJobManager)
        {
            _smtpEmailSender = smtpEmailSender;
            _emailLogRepository = emailLogRepository;
            Logger = logger;
        }

        public ILogger<CustomEmailSender> Logger { get; }

        protected override async Task SendEmailAsync(MailMessage mail)
        {
            var emailLog = await _emailLogRepository.InsertAsync(new EmailLogEntity()
            {
                To = mail.To.Select(x => x.Address).JoinAsString(";"),
                Bcc = mail.Bcc.Select(x => x.Address).JoinAsString(";"),
                Cc = mail.CC.Select(x => x.Address).JoinAsString(";"),
                Body = mail.Body,
                Subject = mail.Subject,
                IsBodyHtml = mail.IsBodyHtml,
            }, autoSave: true);

            emailLog.ProcessAttempt++;

            try
            {
                await _smtpEmailSender.SendAsync(mail);

                emailLog.Ended = true;
                emailLog.Succeeded = true;
            }
            catch (Exception ex)
            {
                emailLog.Note = ex.Message;
                Logger.LogError(ex, "An error occurred while sending email by SMTP");
            }

            await _emailLogRepository.UpdateAsync(emailLog);
        }
    }
}
