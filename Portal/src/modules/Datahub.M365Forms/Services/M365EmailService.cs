using Datahub.Application.Services.Notification;
using Datahub.Core.Services.Notification;
using Datahub.M365Forms.Data;
using Datahub.M365Forms.Templates;
using Microsoft.Extensions.Logging;

namespace Datahub.M365Forms.Services
{
    public class M365EmailService
    {
        private ILogger<M365EmailService> _logger;

        private IEmailNotificationService _emailNotificationService;

        public M365EmailService(
           ILogger<M365EmailService> logger,
           IEmailNotificationService emailNotificationService
        )
        {
            _logger = logger;
            _emailNotificationService = emailNotificationService;
        }

        public async Task SendM365FormsConfirmations(M365FormsParameters parameters)
        {
            if (parameters.AdminEmailAddresses is null)
            {
                _logger.LogError("Invalid parameters");
                return;
            }
            var parametersDict = BuildEmailParameteres(parameters);

            var subject = $"M365 Team Request – {parameters.TeamName}";
            var html = await _emailNotificationService.RenderTemplate<M365Notification>(parametersDict);
            await _emailNotificationService.SendEmailMessage(subject, html, parameters.AdminEmailAddresses);
        }

        private Dictionary<string, object> BuildEmailParameteres(M365FormsParameters parameters)
        {
            if (parameters.AppUrl is null)
            {
                _logger.LogError("Invalid parameters");
                return new Dictionary<string, object>();
            }
            parameters.AppUrl = _emailNotificationService.BuildAppLink(parameters.AppUrl);
            var parametersDict = new Dictionary<string, object>()
        {
            { "ApplicationParameters", parameters }

        };

            return parametersDict;
        }
    }
}
