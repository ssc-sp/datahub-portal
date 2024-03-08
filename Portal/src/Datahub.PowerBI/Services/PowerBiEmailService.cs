using Datahub.Core.Services.Notification;
using Datahub.Core.Utils;
using Datahub.PowerBI.Data;
using Datahub.PowerBI.Templates;
using Datahub.ProjectTools.Templates;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datahub.Application.Services.Notification;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Datahub.PowerBI.Services
{
	public class PowerBiEmailService
	{
		private ILogger<PowerBiEmailService> _logger;

		private IEmailNotificationService _emailNotificationService;

		public PowerBiEmailService(
		   ILogger<PowerBiEmailService> logger,
		   IEmailNotificationService emailNotificationService
		)
		{
			_logger = logger;
			_emailNotificationService = emailNotificationService;
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

		public async Task SendExternalPowerBiCreationRequested(PowerBiExternalReportParameters parameters)
		{
			var parametersDict = BuildPowerBiExternalReportParameters(parameters);

			var subject = $"External Power Bi Report Requested";

			var html = await _emailNotificationService.RenderTemplate<ExternalPowerBiCreation>(parametersDict);

			await _emailNotificationService.SendEmailMessage(subject, html, parameters.AdminEmailAddresses);
		}

	}
}
