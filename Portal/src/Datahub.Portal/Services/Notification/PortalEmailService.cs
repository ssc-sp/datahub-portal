using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlazorTemplater;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Onboarding;
using Datahub.Core.Services.Notification;
using Datahub.Core.Services.Security;
using Datahub.Core.Services.UserManagement;
using Datahub.Portal.Templates;
using Datahub.Portal.Templates.FileSharing;
using Datahub.Portal.Templates.M365Forms;
using Datahub.Portal.Templates.Onboarding;
using Datahub.Portal.Templates.PowerBi;
using Datahub.Core.Utils;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using MimeKit;

namespace Datahub.Portal.Services.Notification;

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

public class PortalEmailService
{

    public static readonly string EMAIL_CONFIGURATION_ROOT_KEY = "EmailNotification";

    public static readonly string USERNAME_TEMPLATE_KEY = "Username";
    public static readonly string SERVICE_TEMPLATE_KEY = "Service";
    public static readonly string DATA_PROJECT_TEMPLATE_KEY = "DataProject";

    private static readonly string DATAHUB_ADMIN_PROJECT_CODE = "DHPGLIST";

    private EmailConfiguration _config;

    private IStringLocalizer _localizer;

    private ILogger<PortalEmailService> _logger;

    private IMSGraphService _graphService;

    private ServiceAuthManager _serviceAuthManager;
    private IEmailNotificationService _emailNotificationService;

    public PortalEmailService(
        IStringLocalizer localizer,
        IConfiguration config,
        ILogger<PortalEmailService> logger,
        IMSGraphService graphService,
        IEmailNotificationService emailNotificationService,
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
        _emailNotificationService = emailNotificationService;
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

        var adminLink = _emailNotificationService.BuildAppLink(ServiceCreationRequest.ADMIN_URL);
        parameters.Add(nameof(ServiceCreationRequest.AdminPageUrl), adminLink);

        var html = await _emailNotificationService.RenderTemplate<ServiceCreationRequest>(parameters);

        await _emailNotificationService.SendEmailMessage(subject, html, recipients);
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

        var adminLink = _emailNotificationService.BuildAppLink(ServiceAccessRequest.ADMIN_URL);
        parameters.Add(nameof(ServiceAccessRequest.AdminPageLink), adminLink);

        var html = await _emailNotificationService.RenderTemplate<ServiceAccessRequest>(parameters);

        await _emailNotificationService.SendEmailMessage(subject, html, recipients);
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
        var projectPageLink = _emailNotificationService.BuildAppLink(projectPagePath);
        parameters.Add(nameof(ServiceAccessRequestApproved.ProjectPageUrl), projectPageLink);

        var html = await _emailNotificationService.RenderTemplate<ServiceAccessRequestApproved>(parameters);

        await _emailNotificationService.SendEmailMessage(subject, html, recipientAddress, recipientName);
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
        var projectPageLink = _emailNotificationService.BuildAppLink(projectPagePath);
        parameters.Add(nameof(ServiceRequestApproved.ProjectPageUrl), projectPageLink);

        var html = await _emailNotificationService.RenderTemplate<ServiceRequestApproved>(parameters);

        await _emailNotificationService.SendEmailMessage(subject, html, recipientAddress, recipientName);
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
        var projectPageLink = _emailNotificationService.BuildAppLink(projectPagePath);
        parameters.Add(nameof(ServiceCreatedGroupNotification.ProjectPageUrl), projectPageLink);

        var html = await _emailNotificationService.RenderTemplate<ServiceCreatedGroupNotification>(parameters);

        await _emailNotificationService.SendEmailMessage(subject, html, recipients);
    }

    public async Task SendAccessRevokedNotification(string serviceName, DatahubProjectInfo projectInfo, string recipientAddress, string recipientName = null)
    {
        var parameters = BuildNotificationParameters(projectInfo, serviceName);

        var subject = $"[DataHub] {serviceName} service access revoked / accès au service révoqué";

        var html = await _emailNotificationService.RenderTemplate<ServiceAccessRevoked>(parameters);

        await _emailNotificationService.SendEmailMessage(subject, html, recipientAddress, recipientName);
    }



    public async Task SendOnboardingConfirmations(OnboardingParameters parameters, bool isClientNotificationSent)
    {
        var parametersDict = BuildOnboardingParameters(parameters);

        var subject = $"Onboarding Request – {parameters.App.Product_Name}";
        var html = isClientNotificationSent ? await _emailNotificationService.RenderTemplate<OnboardingAdminUpdated>(parametersDict) : await _emailNotificationService.RenderTemplate<OnboardingAdmin>(parametersDict);
        await _emailNotificationService.SendEmailMessage(subject, html, parameters.AdminEmailAddresses);
        if (!isClientNotificationSent)
        {
            html = await _emailNotificationService.RenderTemplate<OnboardingClient>(parametersDict);
            await _emailNotificationService.SendEmailMessage(subject, html, parameters.App.Client_Email, parameters.App.Client_Contact_Name);
            if (!string.IsNullOrEmpty(parameters.App.Additional_Contact_Email_EMAIL))
                await _emailNotificationService.SendEmailMessage(subject, html, parameters.App.Additional_Contact_Email_EMAIL, parameters.App.Additional_Contact_Email_EMAIL);
        }

    }

    public async Task SendOnboardingMetadataEditRequest(OnboardingParameters parameters)
    {
        var parametersDict = BuildOnboardingParameters(parameters);
        var subject = $"Please complete the details for your DataHub Initiative";
        var html = await _emailNotificationService.RenderTemplate<OnBoardingMetadataRequest>(parametersDict);
        await _emailNotificationService.SendEmailMessage(subject, html, parameters.App.Client_Email, parameters.App.Client_Contact_Name);
    }

    public async Task SendExternalPowerBiCreationRequested(PowerBiExternalReportParameters parameters)
    {
        var parametersDict = BuildPowerBiExternalReportParameters(parameters);

        var subject = $"External Power Bi Report Requested";

        var html = await _emailNotificationService.RenderTemplate<ExternalPowerBiCreation>(parametersDict);

        await _emailNotificationService.SendEmailMessage(subject, html, parameters.AdminEmailAddresses);
    }

   
    private Dictionary<string, object> BuildOnboardingParameters(OnboardingParameters parameters)
    {

        parameters.AppUrl = _emailNotificationService.BuildAppLink(parameters.AppUrl);
        var parametersDict = new Dictionary<string, object>()
        {
            { "ApplicationParameters", parameters }

        };

        return parametersDict;
    }

    private Dictionary<string, object> BuildPowerBiExternalReportParameters(PowerBiExternalReportParameters parameters)
    {

        parameters.AppUrl = _emailNotificationService.BuildAppLink(parameters.AppUrl);
        var parametersDict = new Dictionary<string, object>()
        {
            { "ApplicationParameters", parameters }

        };

        return parametersDict;
    }

    public async Task SendPowerBiExternalUrlEmail(PowerBiExternalReportParameters parameters)
    {
        var parametersDict = BuildPowerBiExternalReportParameters(parameters);

        var subject = $"External Power Bi Report Request";

        var html = await _emailNotificationService.RenderTemplate<ExternalPowerBiCreated>(parametersDict);

        await _emailNotificationService.SendEmailMessage(subject, html, parameters.App.RequestingUser, parameters.App.RequestingUser);

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
            var recipient = await _emailNotificationService.BuildRecipient(item.address, item.name);
            return new List<MailboxAddress>() { recipient };
        }
        else
        {
            _logger.LogInformation("List has more than one item: bulk conversion");
            var identifiers = recipients.Select(t => t.address).ToList();
            var result = await _emailNotificationService.BuildRecipientList(identifiers);
            return result;
        }
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
            { nameof(PublicUrlApprovalRequest.SharingDashboardLink), _emailNotificationService.BuildAppLink(sharingDashboardLink) }
        };

        var html = await _emailNotificationService.RenderTemplate<PublicUrlApprovalRequest>(parameters);

        await _emailNotificationService.SendEmailMessage(subject, html, recipients);
    }

    public async Task SendFileSharingApproved(SharedDataFile sharedFileInfo, DatahubProjectInfo projectInfo, string publicUrlLink, string recipient)
    {
        var subject = "[DataHub] Public file sharing request approved";

        var sharingStatusLink = $"/share/public/{sharedFileInfo.File_ID}";

        var parameters = new Dictionary<string, object>()
        {
            { nameof(PublicUrlShareApproved.Filename), sharedFileInfo.Filename_TXT },
            { nameof(PublicUrlShareApproved.DataProject), projectInfo },
            { nameof(PublicUrlShareApproved.FileSharingStatusLink), _emailNotificationService.BuildAppLink(sharingStatusLink) },
            { nameof(PublicUrlShareApproved.PublicUrlLink), publicUrlLink }
        };

        var now = DateTime.UtcNow;
        if (sharedFileInfo.PublicationDate_DT > now)
        {
            parameters.Add(nameof(PublicUrlShareApproved.PublicationDate), sharedFileInfo.PublicationDate_DT);
        }

        var html = await _emailNotificationService.RenderTemplate<PublicUrlShareApproved>(parameters);

        await _emailNotificationService.SendEmailMessage(subject, html, recipient);
    }

    public async Task SendStorageCostEstimate(User estimatingUser, Dictionary<string, object> parameters)
    {
        var subject = "[DataHub] Your Storage Cost Estimate";
        var adminSubject = "[DataHub] New Storage Cost Estimate";

        var html = await _emailNotificationService.RenderTemplate<StorageCostEstimate>(parameters);
        parameters.Add(nameof(StorageCostEstimateAdmin.UserEmail), estimatingUser.Mail);
        var adminHtml = await _emailNotificationService.RenderTemplate<StorageCostEstimateAdmin>(parameters);

        var adminEmails = _serviceAuthManager.GetProjectAdminsEmails(DATAHUB_ADMIN_PROJECT_CODE);

        var tasks = new List<Task>()
        {
            _emailNotificationService.SendEmailMessage(subject, html, estimatingUser.Mail),
            _emailNotificationService.SendEmailMessage(adminSubject, adminHtml, adminEmails)
        };

        await Task.WhenAll(tasks);
    }

    public async Task SendComputeCostEstimate(User estimatingUser, Dictionary<string, object> parameters)
    {
        var subject = "[DataHub] Your Databricks Compute Cost Estimate";
        var adminSubject = "[DataHub] New Databricks Compute Cost Estimate";

        var html = await _emailNotificationService.RenderTemplate<ComputeCostEstimate>(parameters);

        parameters.Add(nameof(ComputeCostEstimate.UserEmail), estimatingUser.Mail);
        parameters.Add(nameof(ComputeCostEstimate.AdminVersion), true);

        var adminHtml = await _emailNotificationService.RenderTemplate<ComputeCostEstimate>(parameters);

        var adminEmails = _serviceAuthManager.GetProjectAdminsEmails(DATAHUB_ADMIN_PROJECT_CODE);

        var tasks = new List<Task>()
        {
            _emailNotificationService.SendEmailMessage(subject, html, estimatingUser.Mail),
            _emailNotificationService.SendEmailMessage(adminSubject, adminHtml, adminEmails)
        };

        await Task.WhenAll(tasks);
    }


}

public class OnboardingParameters
{
    public OnboardingApp App;
    public string AppUrl;
    public List<string> AdminEmailAddresses;
}
public class PowerBiExternalReportParameters
{
    public ExternalPowerBiReport App;
    public string AppUrl;
    public List<string> AdminEmailAddresses;
}

