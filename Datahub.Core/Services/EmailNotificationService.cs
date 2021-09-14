using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using BlazorTemplater;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using MimeKit;
using NRCan.Datahub.Shared.Templates;
using System.Linq;
using Microsoft.Extensions.Logging;
using System;

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

    public class DatahubProjectInfo
    {
        public string ProjectNameEn { get; set; }
        public string ProjectNameFr { get; set; }
    }

    public class EmailNotificationService : IEmailNotificationService
    {

        public static readonly string EMAIL_CONFIGURATION_ROOT_KEY = "EmailNotification";

        public static readonly string USERNAME_TEMPLATE_KEY = "Username";
        public static readonly string SERVICE_TEMPLATE_KEY = "Service";
        public static readonly string DATA_PROJECT_TEMPLATE_KEY = "DataProject";

        private EmailConfiguration _config;

        private IStringLocalizer _localizer;

        private ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(
            IStringLocalizer localizer,
            IConfiguration config,
            ILogger<EmailNotificationService> logger
            )
        {
            _localizer = localizer;
            _config = new EmailConfiguration();
            config.Bind(EMAIL_CONFIGURATION_ROOT_KEY, _config);
            _logger = logger;
        }

        public async Task<string> RenderTestTemplate()
        {
            string html = new ComponentRenderer<TestEmailTemplate>()
                .AddService<IStringLocalizer>(_localizer)
                .Render();
            return html;
        }

        public async Task<string> RenderTemplate<T>(IDictionary<string, object> parameters = null) where T : Microsoft.AspNetCore.Components.IComponent
        {
            var templater = new Templater();
            templater.AddService<IStringLocalizer>(_localizer);
            var html = templater.RenderComponent<T>(parameters);
            return html;
        }

        public async Task SendEmailMessage(string subject, string body, string recipientAddress, string recipientName = null, bool isHtml = true)
        {
            var recipient = new MailboxAddress(recipientName, recipientAddress);
            var recipients = new List<MailboxAddress>() { recipient };
            await SendEmailMessage(subject, body, recipients, isHtml);
        }

        public async Task SendEmailMessage(string subject, string body, IList<string> recipientAddresses, bool isHtml = true)
        {
            var recipients = recipientAddresses.Select(addr =>
            {
                //TODO lookup username
                var name = (string)null;
                return new MailboxAddress(name, addr);
            }).ToList();
            await SendEmailMessage(subject, body, recipients, isHtml);
        }

        private async Task SendEmailMessage(string subject, string body, IList<MailboxAddress> recipients, bool isHtml)
        {
            try
            {
                var validRecipients = recipients.Where(a => !string.IsNullOrWhiteSpace(a.Address)).ToHashSet();

                if (validRecipients.Count < 1)
                {
                    _logger.LogWarning($"Cannot send '{subject}' - no valid recipients");
                    return;
                }

                var msg = new MimeMessage();
                msg.From.Add(new MailboxAddress(_config.SenderName, _config.SenderAddress));
                msg.To.AddRange(validRecipients);
                msg.Subject = subject;
                var bodyPart = new TextPart(isHtml ? "html" : "plain")
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
            catch (Exception ex)
            {
                foreach (var item in recipients)
                {
                    _logger.LogError(ex, $"Unable to send email to: {item.Name} with subject: {subject}.");
                }
            }
        }

        public bool IsDevTestMode()
        {
            return _config.DevTestMode;
        }

        private Dictionary<string, object> BuildNotificationParameters(DatahubProjectInfo projectInfo, string serviceName, string username = null)
        {
            var parameters = new Dictionary<string, object>()
            {
                { SERVICE_TEMPLATE_KEY, serviceName },
                { DATA_PROJECT_TEMPLATE_KEY, projectInfo }
            };

            if (username != null)
            {
                parameters.Add(USERNAME_TEMPLATE_KEY, username);
            }

            return parameters;
        }

        public async Task SendServiceCreationRequestNotification(string username, string serviceName, DatahubProjectInfo projectInfo, IList<string> recipients)
        {
            var parameters = BuildNotificationParameters(projectInfo, serviceName, username);

            var subject = $"[DataHub] New {serviceName} service request";

            var html = await RenderTemplate<ServiceCreationRequest>(parameters);

            await SendEmailMessage(subject, html, recipients);
        }

        public async Task SendServiceAccessRequestNotification(string username, string serviceName, DatahubProjectInfo projectInfo, IList<string> recipients)
        {
            var parameters = BuildNotificationParameters(projectInfo, serviceName, username);

            var subject = $"[DataHub] {serviceName} access request for project {projectInfo.ProjectNameEn} / demande d’accès pour le projet {projectInfo.ProjectNameFr}";

            var html = await RenderTemplate<ServiceAccessRequest>(parameters);

            await SendEmailMessage(subject, html, recipients);
        }

        public async Task SendServiceAccessGrantedNotification(string serviceName, DatahubProjectInfo projectInfo, string recipientAddress, string recipientName = null)
        {
            var parameters = BuildNotificationParameters(projectInfo, serviceName);

            var subject = $"[DataHub] {serviceName} service access request approved / demande d’accès au service approuvée";

            var html = await RenderTemplate<ServiceAccessRequestApproved>(parameters);

            await SendEmailMessage(subject, html, recipientAddress, recipientName);
        }

        public async Task SendServiceCreationRequestApprovedIndividual(string serviceName, DatahubProjectInfo projectInfo, string recipientAddress, string recipientName = null)
        {
            var parameters = BuildNotificationParameters(projectInfo, serviceName);

            var subject = $"[DataHub] {serviceName} service request approved / demande de service approuvée";

            var html = await RenderTemplate<ServiceRequestApproved>(parameters);

            await SendEmailMessage(subject, html, recipientAddress, recipientName);
        }

        public async Task SendServiceCreationGroupNotification(string serviceName, DatahubProjectInfo projectInfo, IList<string> recipients)
        {
            var parameters = BuildNotificationParameters(projectInfo, serviceName);

            var subject = $"[DataHub] {serviceName} service created / {serviceName} service créé ";

            var html = await RenderTemplate<ServiceCreatedGroupNotification>(parameters);

            await SendEmailMessage(subject, html, recipients);
        }

        public async Task SendAccessRevokedNotification(string serviceName, DatahubProjectInfo projectInfo, string recipientAddress, string recipientName = null)
        {
            var parameters = BuildNotificationParameters(projectInfo, serviceName);

            var subject = $"[DataHub] {serviceName} service access revoked / accès au service révoqué";

            var html = await RenderTemplate<ServiceAccessRevoked>(parameters);

            await SendEmailMessage(subject, html, recipientAddress, recipientName);
        }


        public async Task SendApplicationCompleteNotification(LanguageTrainingParameters parameters)
        {
            var parametersDict = BuildLanguageNotificationParameters(parameters);

            var subject = $"Language Training Request / Demande de formation linguistique - {parameters.EmployeeName} – {parameters.TrainingType} - {parameters.ApplicationId} ";

            var html = await RenderTemplate<ConfirmEmployeeRequest>(parametersDict);

            await SendEmailMessage(subject, html, parameters.EmployeeEmailAddress, parameters.EmployeeName);

            html = await RenderTemplate<RequestManagerApproval>(parametersDict);

            await SendEmailMessage(subject, html, parameters.ManagerEmailAddress, parameters.ManagerName);

        }


        public async Task SendManagerDecisionEmail(LanguageTrainingParameters parameters)
        {
            var parametersDict = BuildLanguageNotificationParameters(parameters);

            if (parameters.ManagerDecision == "Approved")
            {
                var subject = $"Language Training Request – MANAGER APPROVED / Demande de formation linguistique – APPROUVÉE PAR LA GESTION - {parameters.EmployeeName} – {parameters.TrainingType} - {parameters.ApplicationId} ";
                var html = await RenderTemplate<ManagerRequestApproved>(parametersDict);
                await SendEmailMessage(subject, html, parameters.EmployeeEmailAddress, parameters.EmployeeName);
                await SendEmailMessage(subject, html, parameters.ManagerEmailAddress, parameters.ManagerName);
            }
            else
            {
                var subject = $"Language Training Request / Demande de formation linguistique  - {parameters.EmployeeName} – {parameters.TrainingType} - {parameters.ApplicationId} ";
                var html = await RenderTemplate<ManagerRequestDenied>(parametersDict);
                await SendEmailMessage(subject, html, parameters.EmployeeEmailAddress, parameters.EmployeeName);
            }

        }

        private Dictionary<string, object> BuildLanguageNotificationParameters(LanguageTrainingParameters parameters)
        {
            var parametersDict = new Dictionary<string, object>()
            {
                { "ApplicationParameters", parameters }

            };

            return parametersDict;
        }
    }

    public class LanguageTrainingParameters
    {
        public string ApplicationId;
        public string EmployeeName;
        public string EmployeeEmailAddress;
        public string TrainingType;
        public string ManagerEmailAddress;
        public string ManagerName;
        public string LanguageSchoolEmailAddress;
        public string ManagerDecision;
    }
}