using Datahub.Core.Services.Notification;
using Datahub.Core.Services.Security;
using Datahub.Core.Services.UserManagement;
using Datahub.Core.Utils;
using Datahub.ProjectTools.Templates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datahub.Application.Services.Notification;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Projects;

namespace Datahub.ProjectTools.Services
{
    public class ProjectToolsEmailService
    {
        public static readonly string EMAIL_CONFIGURATION_ROOT_KEY = "EmailNotification";

        public static readonly string USERNAME_TEMPLATE_KEY = "Username";
        public static readonly string SERVICE_TEMPLATE_KEY = "Service";
        public static readonly string DATA_PROJECT_TEMPLATE_KEY = "DataProject";

        private EmailConfiguration _config;

        private IStringLocalizer _localizer;

        private ILogger<ProjectToolsEmailService> _logger;

        private IMSGraphService _graphService;

        private ServiceAuthManager _serviceAuthManager;
        private IEmailNotificationService _emailNotificationService;

        public ProjectToolsEmailService(
            IStringLocalizer localizer,
            IConfiguration config,
            ILogger<ProjectToolsEmailService> logger,
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

            var projectPagePath = $"w/{projectInfo.ProjectCode}";
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

            var projectPagePath = $"w/{projectInfo.ProjectCode}";
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

            var projectPagePath = $"w/{projectInfo.ProjectCode}";
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


    }
}
