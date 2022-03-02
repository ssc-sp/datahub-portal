using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using BlazorTemplater;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using MimeKit;
using Datahub.Core.Templates;
using System.Linq;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using Datahub.Core.Model.Onboarding;
using Datahub.Core.EFCore;
using Microsoft.Graph;
using System.Threading;
using Datahub.Core.Utils;

namespace Datahub.Core.Services
{
    public class EmailConfiguration
    {
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public string SenderName { get; set; }
        public string SenderAddress { get; set; }
        public string AppDomain { get; set; }
        public bool DevTestMode { get; set; }
        public string DevTestEmail { get; set; }
    }

    public class EmailNotificationService : IEmailNotificationService
    {

        public static readonly string EMAIL_CONFIGURATION_ROOT_KEY = "EmailNotification";

        public static readonly string USERNAME_TEMPLATE_KEY = "Username";
        public static readonly string SERVICE_TEMPLATE_KEY = "Service";
        public static readonly string DATA_PROJECT_TEMPLATE_KEY = "DataProject";

        private static readonly string DATAHUB_ADMIN_PROJECT_CODE = "DHPGLIST";

        private EmailConfiguration _config;

        private IStringLocalizer _localizer;

        private ILogger<EmailNotificationService> _logger;

        private IMSGraphService _graphService;

        private ServiceAuthManager _serviceAuthManager;

        public EmailNotificationService(
            IStringLocalizer localizer,
            IConfiguration config,
            ILogger<EmailNotificationService> logger,
            IMSGraphService graphService,
            ServiceAuthManager serviceAuthManager
            )
        {
            _localizer = localizer;
            _config = new EmailConfiguration();
            config.Bind(EMAIL_CONFIGURATION_ROOT_KEY, _config);
            if (_config.AppDomain is null)
            {
                logger.LogCritical("No Email Configuration available");
                _config = null;
            }
            _logger = logger;
            _graphService = graphService;
            _serviceAuthManager = serviceAuthManager;
        }

        public async Task<string> RenderTestTemplate()
        {
            string html = new ComponentRenderer<TestEmailTemplate>()
                .AddService<IStringLocalizer>(_localizer)
                .Render();
            return await Task.FromResult(html);
        }
        
        private async Task<MailboxAddress> BuildRecipient(string userIdOrAddress, string recipientName = null)
        {
            if (Guid.TryParse(userIdOrAddress, out var parsedGuid))
            {
                // for a valid guid, try to get the corresponding user and use that info
                var user = await _graphService.GetUserAsync(userIdOrAddress, CancellationToken.None);
                if (user != null)
                {
                    return CreateMailboxAddress(user.DisplayName, user.Mail);
                }
            }
            else if (userIdOrAddress.Contains('@'))
            {
                // if recipientName is provided, just use that along with the email address
                if (!string.IsNullOrEmpty(recipientName))
                {
                    return CreateMailboxAddress(recipientName, userIdOrAddress);
                }
                else
                {
                    // otherwise, try to lookup the user to get the name
                    var userId = await _graphService.GetUserIdFromEmailAsync(userIdOrAddress, CancellationToken.None);
                    // no need to null check userId, as GetUser has its own null check
                    var user = await _graphService.GetUserAsync(userId, CancellationToken.None);

                    // even if we don't get a user object, we can still try to send to the address
                    return CreateMailboxAddress(user?.DisplayName, userIdOrAddress);
                }
            }

            return null;
        }

        static MailboxAddress CreateMailboxAddress(string name, string address) => new(name, address?.ToLower());

        private async Task<IList<MailboxAddress>> BuildRecipientList(IList<string> userIdsOrAddresses)
        {

            List<MailboxAddress> mailList = new();
            foreach (var item in userIdsOrAddresses)
            {
                var s = await BuildRecipient(item);
                mailList.Add(s);
            }

            return mailList;

        }

        public async Task<string> RenderTemplate<T>(IDictionary<string, object> parameters = null) where T : Microsoft.AspNetCore.Components.IComponent
        {
            var templater = new Templater();
            templater.AddService<IStringLocalizer>(_localizer);
            var html = templater.RenderComponent<T>(parameters);
            return await Task.FromResult(html);
        }

        public async Task SendEmailMessage(string subject, string body, string userIdOrAddress, string recipientName = null, bool isHtml = true)
        {
            if (_config is null)
            {
                _logger.LogCritical("Cannot send email - no configuration available");
                return;
            }

            var recipient = await BuildRecipient(userIdOrAddress, recipientName);
            var recipients = new List<MailboxAddress>() { recipient };
            await SendEmailMessage(subject, body, recipients, isHtml);
        }

        public async Task SendEmailMessage(string subject, string body, IList<string> userIdsOrAddresses, bool isHtml = true)
        {
            if (_config is null)
            {
                _logger.LogCritical("Cannot send email - no configuration available");
                return;
            }

            var recipients = await BuildRecipientList(userIdsOrAddresses);
            await SendEmailMessage(subject, body, recipients, isHtml);
        }

        private static string BuildTestEmailOriginalRecipientsList(IEnumerable<MailboxAddress> recipients, bool isHtml)
        {
            var sb = new StringBuilder();
            if (isHtml)
            {
                sb.Append("<hr />");
                sb.Append("<b>Original Recipients:</b>");
                sb.Append("<ul>");
                foreach(var recipient in recipients)
                {
                    sb.Append($"<li>{recipient.Name} - {recipient.Address}</li>");
                }
                sb.Append("</ul>");
            }
            else
            {
                const string nl = "\r\n";
                sb.Append(nl);
                sb.Append("-----");
                sb.Append(nl);
                sb.Append("Original recipients:");
                foreach(var recipient in recipients)
                {
                    sb.Append(nl);
                    sb.Append($"{recipient.Name} - {recipient.Address}");
                }
            }

            return sb.ToString();
        }

        private async Task SendEmailMessage(string subject, string body, IList<MailboxAddress> recipients, bool isHtml)
        {
            if (_config is null)
            {
                _logger.LogCritical("Cannot send email - no configuration available");
                return;
            }

            try
            {
                var validRecipients = recipients.Where(a => !string.IsNullOrWhiteSpace(a.Address)).ToHashSet();

                if (validRecipients.Count < 1)
                {
                    _logger.LogWarning($"Cannot send '{subject}' - no valid recipients");
                    return;
                }

                var msg = new MimeMessage();
                msg.From.Add(CreateMailboxAddress(_config.SenderName, _config.SenderAddress));

                if (IsDevTestMode())
                {
                    var devEmail = await BuildRecipient(_config.DevTestEmail);
                    msg.To.Add(devEmail);

                    body += BuildTestEmailOriginalRecipientsList(validRecipients, isHtml);
                }
                else
                {
                    msg.To.AddRange(validRecipients);
                }

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
            if (_config is null)
            {
                _logger.LogCritical("Cannot send email - no configuration available");
                return;
            }

            var parameters = BuildNotificationParameters(projectInfo, serviceName, username);

            var subject = $"[DataHub] New {serviceName} service request";

            var adminLink = BuildAppLink(ServiceCreationRequest.ADMIN_URL);
            parameters.Add(nameof(ServiceCreationRequest.AdminPageUrl), adminLink);

            var html = await RenderTemplate<ServiceCreationRequest>(parameters);

            await SendEmailMessage(subject, html, recipients);
        }

        public async Task SendServiceAccessRequestNotification(string username, string serviceName, DatahubProjectInfo projectInfo, IList<string> recipients)
        {
            if (_config is null)
            {
                _logger.LogCritical("Cannot send email - no configuration available");
                return;
            }

            var parameters = BuildNotificationParameters(projectInfo, serviceName, username);

            var subject = $"[DataHub] {serviceName} access request for project {projectInfo.ProjectNameEn} / demande d’accès pour le projet {projectInfo.ProjectNameFr}";

            var adminLink = BuildAppLink(ServiceAccessRequest.ADMIN_URL);
            parameters.Add(nameof(ServiceAccessRequest.AdminPageLink), adminLink);

            var html = await RenderTemplate<ServiceAccessRequest>(parameters);

            await SendEmailMessage(subject, html, recipients);
        }

        public async Task SendServiceAccessGrantedNotification(string serviceName, DatahubProjectInfo projectInfo, string recipientAddress, string recipientName = null)
        {
            if (_config is null)
            {
                _logger.LogCritical("Cannot send email - no configuration available");
                return;
            }

            var parameters = BuildNotificationParameters(projectInfo, serviceName);

            var subject = $"[DataHub] {serviceName} service access request approved / demande d’accès au service approuvée";

            var projectPagePath = $"{UrlPathSegment.PROJECTS}/{projectInfo.ProjectCode}";
            var projectPageLink = BuildAppLink(projectPagePath);
            parameters.Add(nameof(ServiceAccessRequestApproved.ProjectPageUrl), projectPageLink);

            var html = await RenderTemplate<ServiceAccessRequestApproved>(parameters);

            await SendEmailMessage(subject, html, recipientAddress, recipientName);
        }

        public async Task SendServiceCreationRequestApprovedIndividual(string serviceName, DatahubProjectInfo projectInfo, string recipientAddress, string recipientName = null)
        {
            if (_config is null)
            {
                _logger.LogCritical("Cannot send email - no configuration available");
                return;
            }

            var parameters = BuildNotificationParameters(projectInfo, serviceName);

            var subject = $"[DataHub] {serviceName} service request approved / demande de service approuvée";

            var projectPagePath = $"{UrlPathSegment.PROJECTS}/{projectInfo.ProjectCode}";
            var projectPageLink = BuildAppLink(projectPagePath);
            parameters.Add(nameof(ServiceRequestApproved.ProjectPageUrl), projectPageLink);

            var html = await RenderTemplate<ServiceRequestApproved>(parameters);

            await SendEmailMessage(subject, html, recipientAddress, recipientName);
        }

        public async Task SendServiceCreationGroupNotification(string serviceName, DatahubProjectInfo projectInfo, IList<string> recipients)
        {
            if (_config is null)
            {
                _logger.LogCritical("Cannot send email - no configuration available");
                return;
            }

            var parameters = BuildNotificationParameters(projectInfo, serviceName);

            var subject = $"[DataHub] {serviceName} service created / {serviceName} service créé ";

            var projectPagePath = $"{UrlPathSegment.PROJECTS}/{projectInfo.ProjectCode}";
            var projectPageLink = BuildAppLink(projectPagePath);
            parameters.Add(nameof(ServiceCreatedGroupNotification.ProjectPageUrl), projectPageLink);

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
                html = await RenderTemplate<LSUNotification>(parametersDict);
                await SendEmailMessage(subject, html, parameters.AdminEmailAddresses);

            }
            else
            {
                var subject = $"Language Training Request / Demande de formation linguistique  - {parameters.EmployeeName} – {parameters.TrainingType} - {parameters.ApplicationId} ";
                var html = await RenderTemplate<ManagerRequestDenied>(parametersDict);
                await SendEmailMessage(subject, html, parameters.EmployeeEmailAddress, parameters.EmployeeName);
            }

        }

        public async Task SendLanguageSchoolDecision(LanguageTrainingParameters parameters)
        {
            var parametersDict = BuildLanguageNotificationParameters(parameters);

            if (parameters.LanguageSchoolDecision == "Training Accepted")
            {
                var subject = $"Language Training Request – PLACEMENT ACCEPTED / Demande de formation linguistique APPROUVÉE - STAGE ACCEPTÉ - {parameters.EmployeeName} – {parameters.TrainingType} - {parameters.ApplicationId} ";
                var html = await RenderTemplate<LSUApproved>(parametersDict);
                await SendEmailMessage(subject, html, parameters.EmployeeEmailAddress, parameters.EmployeeName);
                await SendEmailMessage(subject, html, parameters.ManagerEmailAddress, parameters.ManagerName);
            }
            else if (parameters.LanguageSchoolDecision == "Requires LETP assessment")
            {
                var subject = $"Language Training Request – NEW LETP REQUIRED / Demande de formation linguistique - NOUVEAU ELFP REQUIS - {parameters.EmployeeName} – {parameters.TrainingType} - {parameters.ApplicationId} ";
                var html = await RenderTemplate<LSUNewLTPReq>(parametersDict);
                await SendEmailMessage(subject, html, parameters.EmployeeEmailAddress, parameters.EmployeeName);
                await SendEmailMessage(subject, html, parameters.ManagerEmailAddress, parameters.ManagerName);
            }
            else if (parameters.LanguageSchoolDecision == "Insufficient interest at level")
            {
                var subject = $"Language Training Request – INSUFFICIENT INTEREST / Demande de formation linguistique - INTÉRÊT INSUFFISANT - {parameters.EmployeeName} – {parameters.TrainingType} - {parameters.ApplicationId} ";
                var html = await RenderTemplate<LSUInsufficientInterest>(parametersDict);
                await SendEmailMessage(subject, html, parameters.EmployeeEmailAddress, parameters.EmployeeName);
                await SendEmailMessage(subject, html, parameters.ManagerEmailAddress, parameters.ManagerName);
            }
            else if (parameters.LanguageSchoolDecision == "Demand exceeds capacity")
            {
                var subject = $"Language Training Request – EXCESS IN DEMAND / Demande de formation linguistique - SURPLUS DE DEMANDE - {parameters.EmployeeName} – {parameters.TrainingType} - {parameters.ApplicationId} ";
                var html = await RenderTemplate<LSUExcessInDemand>(parametersDict);
                await SendEmailMessage(subject, html, parameters.EmployeeEmailAddress, parameters.EmployeeName);
                await SendEmailMessage(subject, html, parameters.ManagerEmailAddress, parameters.ManagerName);
            }

        }
        public async Task SendM365FormsConfirmations(M365FormsParameters parameters)
        {
            var parametersDict = BuildM365FormsParameters(parameters);

            var subject = $"M365 Team Request – {parameters.TeamName}";
            var html = await RenderTemplate<M365Notification>(parametersDict);
            await SendEmailMessage(subject, html, parameters.AdminEmailAddresses);
        }

        private Dictionary<string, object> BuildM365FormsParameters(M365FormsParameters parameters)
        {
            parameters.AppUrl = BuildAppLink(parameters.AppUrl);
            var parametersDict = new Dictionary<string, object>()
            {
                { "ApplicationParameters", parameters }

            };

            return parametersDict;
        }

        public async Task SendOnboardingConfirmations(OnboardingParameters parameters)
        {
            var parametersDict = BuildOnboardingParameters(parameters);

            var subject = $"Onboarding Request – {parameters.App.Project_Name}";
            var html = await RenderTemplate<OnboardingAdmin>(parametersDict);
            await SendEmailMessage(subject, html, parameters.AdminEmailAddresses);
            html = await RenderTemplate<OnboardingClient>(parametersDict);
            await SendEmailMessage(subject, html, parameters.App.Client_Email, parameters.App.Client_Contact_Name);
            if (!string.IsNullOrEmpty(parameters.App.Additional_Contact_Email_EMAIL))
                await SendEmailMessage(subject, html, parameters.App.Additional_Contact_Email_EMAIL, parameters.App.Additional_Contact_Email_EMAIL);

        }

        private Dictionary<string, object> BuildLanguageNotificationParameters(LanguageTrainingParameters parameters)
        {
            parameters.AppUrl = BuildAppLink(parameters.AppUrl);
            var parametersDict = new Dictionary<string, object>()
            {
                { "ApplicationParameters", parameters }

            };

            return parametersDict;
        }

        private Dictionary<string, object> BuildOnboardingParameters(OnboardingParameters parameters)
        {

            parameters.AppUrl = BuildAppLink(parameters.AppUrl);
            var parametersDict = new Dictionary<string, object>()
            {
                { "ApplicationParameters", parameters }

            };

            return parametersDict;
        }

        public async Task<IList<MailboxAddress>> TestUsernameEmailConversion(IList<(string address, string name)> recipients)
        {
            if (recipients == null)
            {
                _logger.LogError("List is null");
                return null;
            }
            else if (recipients.Count < 1)
            {
                _logger.LogWarning("List is empty");
                return new List<MailboxAddress>();
            }
            else if (recipients.Count == 1)
            {
                _logger.LogInformation("List has 1 item: single recipient method");
                var item = recipients.First();
                var recipient = await BuildRecipient(item.address, item.name);
                return new List<MailboxAddress>() { recipient }; 
            }
            else
            {
                _logger.LogInformation("List has more than one item: bulk conversion");
                var identifiers = recipients.Select(t => t.address).ToList();
                var result = await BuildRecipientList(identifiers);
                return result;
            }
        }

        public string BuildAppLink(string contextUrl)
        {
            var ub = new UriBuilder(_config.AppDomain);
            ub.Path = contextUrl;
            return ub.ToString();
        }

        public async Task SendFileSharingApprovalRequest(string username, string filename, DatahubProjectInfo projectInfo, IList<string> recipients)
        {
            var subject = "[DataHub] Public file sharing request";

            var sharingDashboardLink = $"/{UrlPathSegment.PROJECTS}/{projectInfo.ProjectCode}/datasharing";

            var parameters = new Dictionary<string, object>
            {
                { nameof(PublicUrlApprovalRequest.DataProject), projectInfo },
                { nameof(PublicUrlApprovalRequest.Username), username },
                { nameof(PublicUrlApprovalRequest.Filename), filename },
                { nameof(PublicUrlApprovalRequest.SharingDashboardLink), BuildAppLink(sharingDashboardLink) }
            };

            var html = await RenderTemplate<PublicUrlApprovalRequest>(parameters);

            await SendEmailMessage(subject, html, recipients);
        }

        public async Task SendFileSharingApproved(SharedDataFile sharedFileInfo, DatahubProjectInfo projectInfo, string publicUrlLink, string recipient)
        {
            var subject = "[DataHub] Public file sharing request approved";

            var sharingStatusLink = $"/share/public/{sharedFileInfo.File_ID}";

            var parameters = new Dictionary<string, object>()
            {
                { nameof(PublicUrlShareApproved.Filename), sharedFileInfo.Filename_TXT },
                { nameof(PublicUrlShareApproved.DataProject), projectInfo },
                { nameof(PublicUrlShareApproved.FileSharingStatusLink), BuildAppLink(sharingStatusLink) },
                { nameof(PublicUrlShareApproved.PublicUrlLink), publicUrlLink }
            };

            var now = DateTime.UtcNow;
            if (sharedFileInfo.PublicationDate_DT > now)
            {
                parameters.Add(nameof(PublicUrlShareApproved.PublicationDate), sharedFileInfo.PublicationDate_DT);
            }

            var html = await RenderTemplate<PublicUrlShareApproved>(parameters);

            await SendEmailMessage(subject, html, recipient);
        }

        public async Task SendStorageCostEstimate(User estimatingUser, Dictionary<string, object> parameters)
        {
            var subject = "[DataHub] Your Storage Cost Estimate";
            var adminSubject = "[DataHub] New Storage Cost Estimate";

            var html = await RenderTemplate<StorageCostEstimate>(parameters);
            parameters.Add(nameof(StorageCostEstimateAdmin.UserEmail), estimatingUser.Mail);
            var adminHtml = await RenderTemplate<StorageCostEstimateAdmin>(parameters);

            var adminEmails = _serviceAuthManager.GetProjectAdminsEmails(DATAHUB_ADMIN_PROJECT_CODE);

            var tasks = new List<Task>()
            {
                SendEmailMessage(subject, html, estimatingUser.Mail),
                SendEmailMessage(adminSubject, adminHtml, adminEmails)
            };

            await Task.WhenAll(tasks);
        }

        public async Task SendComputeCostEstimate(User estimatingUser, Dictionary<string, object> parameters)
        {
            var subject = "[DataHub] Your Databricks Compute Cost Estimate";
            var adminSubject = "[DataHub] New Databricks Compute Cost Estimate";

            var html = await RenderTemplate<ComputeCostEstimate>(parameters);

            parameters.Add(nameof(ComputeCostEstimate.UserEmail), estimatingUser.Mail);
            parameters.Add(nameof(ComputeCostEstimate.AdminVersion), true);

            var adminHtml = await RenderTemplate<ComputeCostEstimate>(parameters);

            var adminEmails = _serviceAuthManager.GetProjectAdminsEmails(DATAHUB_ADMIN_PROJECT_CODE);

            var tasks = new List<Task>()
            {
                SendEmailMessage(subject, html, estimatingUser.Mail),
                SendEmailMessage(adminSubject, adminHtml, adminEmails)
            };

            await Task.WhenAll(tasks);
        }
    }

    public class LanguageTrainingParameters
    {
        public string ApplicationId;
        public string AppUrl;
        public string EmployeeName;
        public string EmployeeEmailAddress;
        public string TrainingType;
        public string Session;
        public string Class;
        public string ManagerEmailAddress;
        public string ManagerName;
        public string LanguageSchoolEmailAddress;
        public string ManagerDecision;
        public string LanguageSchoolDecision;
        public string FormUrl;
        public List<string> AdminEmailAddresses;
    }

    public class OnboardingParameters
    {
        public OnboardingApp App;
        public string AppUrl;
        public List<string> AdminEmailAddresses;
    }

    public class M365FormsParameters
    {
        public string TeamName;
        public string BusinessOwner;
        public string SubmitterEmaill;
        public int AppId;
        public string AppUrl;
        public List<string> AdminEmailAddresses;
    }

}