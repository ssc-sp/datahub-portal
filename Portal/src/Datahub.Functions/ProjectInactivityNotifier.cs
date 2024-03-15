using System.Text.Json;
using Datahub.Application.Services;
using Datahub.Application.Services.Projects;
using Datahub.Core.Model.Datahub;
using Datahub.Functions.Providers;
using Datahub.Functions.Services;
using Datahub.Functions.Validators;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using Datahub.Shared.Entities;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions
{
	public class ProjectInactivityNotifier
	{
		private readonly IMediator _mediator;
		private readonly ILogger<ProjectUsageNotifier> _logger;
		private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
		private readonly IResourceMessagingService _resourceMessagingService;
		private readonly IProjectInactivityNotificationService _projectInactivityNotificationService;
		private readonly IEmailService _emailService;
		private readonly IDateProvider _dateProvider;
		private readonly AzureConfig _config;

		private readonly QueuePongService _pongService;
		private readonly EmailValidator _emailValidator;

		public ProjectInactivityNotifier(ILoggerFactory loggerFactory, IMediator mediator,
			IDbContextFactory<DatahubProjectDBContext> dbContextFactory, QueuePongService pongService,
			IProjectInactivityNotificationService projectInactivityNotificationService,
			IResourceMessagingService resourceMessagingService, EmailValidator emailValidator,
			IDateProvider dateProvider, AzureConfig config, IEmailService emailService)
		{
			_logger = loggerFactory.CreateLogger<ProjectUsageNotifier>();
			_mediator = mediator;
			_dbContextFactory = dbContextFactory;
			_pongService = pongService;
			_projectInactivityNotificationService = projectInactivityNotificationService;
			_resourceMessagingService = resourceMessagingService;
			_emailValidator = emailValidator;
			_dateProvider = dateProvider;
			_config = config;
			_emailService = emailService;
		}

		[Function("ProjectInactivityNotifier")]
		public async Task Run(
			[QueueTrigger("%QueueProjectInactivityNotification%", Connection = "DatahubStorageConnectionString")]
			string queueItem,
			CancellationToken ct)
		{
			// test for ping
			if (await _pongService.Pong(queueItem))
				return;

			// deserialize message
			var message = DeserializeQueueMessage(queueItem);

			// verify message 
			if (message is null)
			{
				throw new Exception($"Invalid queue message:\n{queueItem}");
			}

			using var ctx = await _dbContextFactory.CreateDbContextAsync(ct);

			// get project
			var project = await ctx.Projects.AsNoTracking().Where(x => x.Project_ID == message.ProjectId)
				.FirstOrDefaultAsync(ct);

			// get project info
			var lastLoginDate = project?.LastLoginDate ?? project.Last_Updated_DT;
			var daysSinceLastLogin = (_dateProvider.Today - lastLoginDate).Days;
			var daysUntilDeletion = _dateProvider.ProjectDeletionDay() - daysSinceLastLogin;
			var operationalWindow = project.OperationalWindow;
			var hasCostRecovery = project.HasCostRecovery;
			(var contacts, var acronym) = await GetProjectDetails(message.ProjectId, ct);

			// check if project to be notified
			var email = await CheckIfProjectToBeNotified(daysUntilDeletion, daysSinceLastLogin, operationalWindow,
				hasCostRecovery, acronym, contacts);

			// if email is not null, send email
			if (email != null)
			{
				await _mediator.Send(email, ct);

				// add notification to db
				var sentTo = string.Join(",", contacts);
				await _projectInactivityNotificationService.AddInactivityNotification(message.ProjectId,
					_dateProvider.Today, daysUntilDeletion, sentTo, ct);
			}

			// check if project to be deleted
			var workspaceDefinition = await CheckIfProjectToBeDeleted(daysSinceLastLogin, operationalWindow,
				hasCostRecovery, acronym);

			// if project to be deleted, send to terraform delete queue
			if (workspaceDefinition != null)
			{
				await _resourceMessagingService.SendToTerraformDeleteQueue(workspaceDefinition, project.Project_ID);
			}
		}

		public async Task<EmailRequestMessage?> CheckIfProjectToBeNotified(int daysUntilDeletion,
			int daysSinceLastLogin, DateTime? operationalWindow, bool hasCostRecovery, string acronym,
			List<string> contacts)
		{
			// check if we are past operational window or that it is null and that the project has no cost recovery and that
			if ((operationalWindow == null || operationalWindow < _dateProvider.Today) && !hasCostRecovery &&
				_dateProvider.ProjectNotificationDays().Contains(daysUntilDeletion))
			{
				return GetEmailRequestMessage(daysUntilDeletion, daysSinceLastLogin, acronym, contacts);
			}

			return null;
		}

		public async Task<WorkspaceDefinition?> CheckIfProjectToBeDeleted(int daysSinceLastLogin,
			DateTime? operationalWindow, bool hasCostRecovery, string acronym)
		{
			// check if we are past operational window or that it is null and that the project has no cost recovery and that we are at or are past the deletion day
			if ((operationalWindow == null || operationalWindow < _dateProvider.Today) &&
				daysSinceLastLogin >= _dateProvider.ProjectDeletionDay() &&
				!hasCostRecovery)
			{
				return await _resourceMessagingService.GetWorkspaceDefinition(acronym);
			}

			return null;
		}

		private async Task<(List<string>, string)> GetProjectDetails(int projectId, CancellationToken cancellationToken)
		{
			var ctx = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

			var project = await ctx.Projects
				.AsNoTracking()
				.Where(e => e.Project_ID == projectId)
				.FirstOrDefaultAsync(cancellationToken);

			if (project is null)
				return default;

			var contacts = project.Users?
				.Select(u => u.PortalUser.Email)
				.Where(_emailValidator.IsValidEmail)
				.ToList();

			return (contacts, project.Project_Acronym_CD);
		}

		public EmailRequestMessage GetEmailRequestMessage(int daysUntilDeletion, int daysSinceLastLogin,
			string acronym, List<string> contacts)
		{
			List<string> bcc = new() { GetNotificationCCAddress() };

			Dictionary<string, string> subjectArgs = new()
			{
				{ "{{ws}}", acronym }
			};

			Dictionary<string, string> bodyArgs = new()
			{
				{ "{ws}", acronym },
				{ "{inactive}", daysSinceLastLogin.ToString() },
				{ "{remaining}", daysUntilDeletion.ToString() }
			};

			var email = _emailService.BuildEmail("project_inactive_alert.html", contacts, bcc, bodyArgs,
				subjectArgs);

			return email;
		}

		private string GetNotificationCCAddress()
		{
			return _config.Email?.NotificationsCCAddress ?? "fsdh-notifications-dhsf-notifications@ssc-spc.gc.ca";
		}

		static ProjectInactivityNotificationMessage? DeserializeQueueMessage(string message)
		{
			return JsonSerializer.Deserialize<ProjectInactivityNotificationMessage>(message);
		}
	}
}