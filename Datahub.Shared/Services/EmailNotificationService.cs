using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using BlazorTemplater;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using MimeKit;
using NRCan.Datahub.Shared.Templates;

namespace NRCan.Datahub.Shared.Services
{
    public class EmailConfiguration
    {
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public string SenderName { get; set; }
        public string SenderAddress { get; set; }
        public bool DevTestMode { get; set; }
    }

    public class EmailNotificationService: IEmailNotificationService
    {

        public static readonly string EMAIL_CONFIGURATION_ROOT_KEY = "EmailNotification";

        private EmailConfiguration _config;

        private IStringLocalizer<SharedResources> _localizer;

        public EmailNotificationService(
            IStringLocalizer<SharedResources> localizer,
            IConfiguration config
            )
        {
            _localizer = localizer;
            _config = new EmailConfiguration();
            config.Bind(EMAIL_CONFIGURATION_ROOT_KEY, _config);
        }

        public async Task<string> RenderTestTemplate()
        {
            string html = new ComponentRenderer<TestEmailTemplate>()
                .AddService<IStringLocalizer<SharedResources>>(_localizer)
                .Render();
            return html;
        }

        public async Task<string> RenderTemplate<T>(IDictionary<string, object> parameters = null) where T: Microsoft.AspNetCore.Components.IComponent
        {
            var templater = new Templater();
            templater.AddService<IStringLocalizer<SharedResources>>(_localizer);
            var html = templater.RenderComponent<T>(parameters);
            return html;
        }

        public async Task SendEmailMessage(string subject, string body, string recipientName, string recipientAddress, bool isHtml = true)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_config.SenderName, _config.SenderAddress));
            msg.To.Add(new MailboxAddress(recipientName, recipientAddress));
            msg.Subject = subject;
            var bodyPart = new TextPart(isHtml? "html": "plain") 
            {
                Text = body
            };
            msg.Body = bodyPart;

            using (var smtpClient = new SmtpClient())
            {
                await smtpClient.ConnectAsync(_config.SmtpHost, _config.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(new NetworkCredential(_config.SmtpUsername, _config.SmtpPassword));

                await smtpClient.SendAsync(msg);
                await smtpClient.DisconnectAsync(true);
            }
        }

        public bool IsDevTestMode()
        {
            return _config.DevTestMode;
        }
    }
}