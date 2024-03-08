using Datahub.Core.Services.Notification;
using Datahub.Core.Services.Security;
using Datahub.Core.Services.UserManagement;
using Datahub.LanguageTraining.Data;
using Datahub.LanguageTraining.Templates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datahub.Application.Services.Notification;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Datahub.LanguageTraining.Services
{
	public class LanguageEmailService
	{
		private ILogger<LanguageEmailService> _logger;

		private IEmailNotificationService _emailNotificationService;

		public LanguageEmailService(
			ILogger<LanguageEmailService> logger,
			IEmailNotificationService emailNotificationService
		)
		{
			_logger = logger;
			_emailNotificationService = emailNotificationService;
		}

		public async Task SendApplicationCompleteNotification(LanguageTrainingParameters parameters)
		{
			if (parameters.ManagerEmailAddress is null || parameters.EmployeeEmailAddress is null ||
				parameters.EmployeeName is null || parameters.ManagerName is null)
			{
				_logger.LogError("Invalid parameters");
				return;
			}

			var parametersDict = BuildEmailParameteres(parameters);

			var subject =
				$"Language Training Request / Demande de formation linguistique - {parameters.EmployeeName} – {parameters.TrainingType} - {parameters.ApplicationId} ";

			var html = await _emailNotificationService.RenderTemplate<ConfirmEmployeeRequest>(parametersDict);

			await _emailNotificationService.SendEmailMessage(subject, html, parameters.EmployeeEmailAddress,
				parameters.EmployeeName);

			html = await _emailNotificationService.RenderTemplate<RequestManagerApproval>(parametersDict);

			await _emailNotificationService.SendEmailMessage(subject, html, parameters.ManagerEmailAddress,
				parameters.ManagerName);
		}

		public async Task SendManagerDecisionEmail(LanguageTrainingParameters parameters)
		{
			if (parameters.ManagerEmailAddress is null || parameters.EmployeeEmailAddress is null ||
				parameters.EmployeeName is null || parameters.ManagerName is null || parameters.AdminEmailAddresses is null)
			{
				_logger.LogError("Invalid parameters");
				return;
			}

			var parametersDict = BuildEmailParameteres(parameters);

			if (parameters.ManagerDecision == "Approved")
			{
				var subject =
					$"Language Training Request – MANAGER APPROVED / Demande de formation linguistique – APPROUVÉE PAR LA GESTION - {parameters.EmployeeName} – {parameters.TrainingType} - {parameters.ApplicationId} ";
				var html = await _emailNotificationService.RenderTemplate<ManagerRequestApproved>(parametersDict);

				await _emailNotificationService.SendEmailMessage(subject, html, new List<DatahubEmailRecipient>
				{
					new(parameters.EmployeeName, parameters.EmployeeEmailAddress),
					new(parameters.ManagerName, parameters.ManagerEmailAddress)
				});

				html = await _emailNotificationService.RenderTemplate<LSUNotification>(parametersDict);
				await _emailNotificationService.SendEmailMessage(subject, html, parameters.AdminEmailAddresses);
			}
			else
			{
				var subject =
					$"Language Training Request / Demande de formation linguistique  - {parameters.EmployeeName} – {parameters.TrainingType} - {parameters.ApplicationId} ";
				var html = await _emailNotificationService.RenderTemplate<ManagerRequestDenied>(parametersDict);
				await _emailNotificationService.SendEmailMessage(subject, html, parameters.EmployeeEmailAddress,
					parameters.EmployeeName);
			}
		}

		public async Task SendLanguageSchoolDecision(LanguageTrainingParameters parameters)
		{
			if (parameters.ManagerEmailAddress is null || parameters.EmployeeEmailAddress is null || parameters.EmployeeName is null || parameters.ManagerName is null)
			{
				_logger.LogError("Invalid parameters");
				return;
			}

			var parametersDict = BuildEmailParameteres(parameters);

			if (parameters.LanguageSchoolDecision == "Training accepted")
			{
				var subject =
					$"Language Training Request – PLACEMENT ACCEPTED / Demande de formation linguistique - PLACE APPROUVÉE - {parameters.EmployeeName} – {parameters.TrainingType} - {parameters.ApplicationId} ";
				var html = await _emailNotificationService.RenderTemplate<LSUApproved>(parametersDict);
				await _emailNotificationService.SendEmailMessage(subject, html, new List<DatahubEmailRecipient>
				{
					new(parameters.EmployeeName, parameters.EmployeeEmailAddress),
					new(parameters.ManagerName, parameters.ManagerEmailAddress)
				});
			}
			else if (parameters.LanguageSchoolDecision == "Requires LETP assessment")
			{
				var subject =
					$"Language Training Request – NEW LETP REQUIRED / Demande de formation linguistique - NOUVEAU ELPF REQUIS - {parameters.EmployeeName} – {parameters.TrainingType} - {parameters.ApplicationId} ";
				var html = await _emailNotificationService.RenderTemplate<LSUNewLTPReq>(parametersDict);
				await _emailNotificationService.SendEmailMessage(subject, html, new List<DatahubEmailRecipient>
				{
					new(parameters.EmployeeName, parameters.EmployeeEmailAddress),
					new(parameters.ManagerName, parameters.ManagerEmailAddress)
				});
			}
			else if (parameters.LanguageSchoolDecision == "Insufficient interest at level")
			{
				var subject =
					$"Language Training Request – INSUFFICIENT REGISTRATIONS / Demande de formation linguistique - INSCRIPTIONS INSUFFISANTES - {parameters.EmployeeName} – {parameters.TrainingType} - {parameters.ApplicationId} ";
				var html = await _emailNotificationService.RenderTemplate<LSUInsufficientInterest>(parametersDict);
				await _emailNotificationService.SendEmailMessage(subject, html, new List<DatahubEmailRecipient>
				{
					new(parameters.EmployeeName, parameters.EmployeeEmailAddress),
					new(parameters.ManagerName, parameters.ManagerEmailAddress)
				});
			}
			else if (parameters.LanguageSchoolDecision == "Demand exceeds capacity")
			{
				var subject =
					$"Language Training Request – EXCESS IN DEMAND / Demande de formation linguistique - SURPLUS DE DEMANDE - {parameters.EmployeeName} – {parameters.TrainingType} - {parameters.ApplicationId} ";
				var html = await _emailNotificationService.RenderTemplate<LSUExcessInDemand>(parametersDict);
				await _emailNotificationService.SendEmailMessage(subject, html, new List<DatahubEmailRecipient>
				{
					new(parameters.EmployeeName, parameters.EmployeeEmailAddress),
					new(parameters.ManagerName, parameters.ManagerEmailAddress)
				});
			}
			else if (parameters.LanguageSchoolDecision == "Late application")
			{
				var subject =
					$"Language Training Request – APPLICATION PERIOD CLOSED / Demande de formation linguistique - PÉRIODE D’INSCRIPTION FERMÉE -  {parameters.EmployeeName} – {parameters.TrainingType} - {parameters.ApplicationId} ";
				var html = await _emailNotificationService.RenderTemplate<LSUApplicationPeriodClosed>(parametersDict);
				await _emailNotificationService.SendEmailMessage(subject, html, new List<DatahubEmailRecipient>
				{
					new(parameters.EmployeeName, parameters.EmployeeEmailAddress),
					new(parameters.ManagerName, parameters.ManagerEmailAddress)
				});
			}
		}

		private Dictionary<string, object> BuildEmailParameteres(LanguageTrainingParameters parameters)
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