using Datahub.Application.Services.Notification;
using Datahub.Application.Services.Security;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Onboarding;
using Microsoft.Extensions.Localization;
using MimeKit;
using Datahub.Core.Data;
using Datahub.Core.Model.Projects;
using Microsoft.Graph.Models;
using Datahub.Portal.Templates.Onboarding;
using Datahub.Portal.Templates.FileSharing;
using Datahub.Portal.Templates;

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

    private IStringLocalizer _localizer;

    private ILogger<PortalEmailService> _logger;


    private IServiceAuthManager _serviceAuthManager;
    private IEmailNotificationService _emailNotificationService;

    public PortalEmailService(
        IStringLocalizer localizer,
        IConfiguration config,
        ILogger<PortalEmailService> logger,
        IEmailNotificationService emailNotificationService,
        IServiceAuthManager serviceAuthManager
    )
    {
        _localizer = localizer;
    
        _logger = logger;        
        _serviceAuthManager = serviceAuthManager;
        _emailNotificationService = emailNotificationService;
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


   
    private Dictionary<string, object> BuildOnboardingParameters(OnboardingParameters parameters)
    {

        parameters.AppUrl = _emailNotificationService.BuildAppLink(parameters.AppUrl);
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

        var sharingDashboardLink = $"/w/{projectInfo.ProjectCode}/datasharing";

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

        var adminEmails = _serviceAuthManager.GetProjectAdminsEmails(RoleConstants.DATAHUB_ADMIN_PROJECT);

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

        var adminEmails = _serviceAuthManager.GetProjectAdminsEmails(RoleConstants.DATAHUB_ADMIN_PROJECT);

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

