using Azure.Messaging.ServiceBus;
using Datahub.Functions.Entities;
using Datahub.Functions.Extensions;
using Datahub.Functions.Services;
using Datahub.Infrastructure.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Clients;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Datahub.Functions
{
    public class BugReport(
        ILogger<BugReport> logger,
        AzureConfig config,
        IEmailService emailService,
        ISendEndpointProvider sendEndpointProvider,
        IAlertRecordService alertRecordService,
        IHttpClientFactory httpClientFactory)
    {
        // TODO: enable configuration of these toggles
        private bool _postToTeams = true;
        private bool _sendEmailNotification = true;
        private bool _postToDevops = true;

        [Function("BugReport")]
        public async Task Run(
            [ServiceBusTrigger(QueueConstants.BugReportQueueName, Connection = "DatahubServiceBus:ConnectionString")]
            ServiceBusReceivedMessage message)
        {
            logger.LogInformation($"Bug report queue triggered: {message.Body}");

            // We deserialize the queue message into a BugReportMessage object
            var bug = await message.DeserializeAndUnwrapMessageAsync<BugReportMessage>();

            if (bug != null)
            {
                // check if alert has been sent out recently (InfrastructureError only - other alerts get sent regardless)
                if (bug.BugReportType == BugReportTypes.InfrastructureError)
                {
                    var recentAlert = await alertRecordService.GetRecentAlertForBugMessage(bug);
                    if (recentAlert?.EmailSent ?? false)
                    {
                        logger.LogInformation($"Infrastructure alert has already been sent recently: {recentAlert.ReportIdentifier}");
                        return;
                    }
                }

                var postedToTeams = await PostToTeams(bug);

                // Build the ADO issue
                var issue = CreateIssue(bug);

                // Post the issue to ADO and parse the response
                var workItem = await PostIssue(issue);
                var postedToDevops = workItem is not null;

                // Build the email
                var email = BuildEmail(bug, workItem);
                var emailSent = await SendEmailNotification(email);

                logger.LogDebug($"Posted to Teams: {postedToTeams}; posted to Devops: {postedToDevops}; sent email: {emailSent}");

                var alertSent = postedToDevops || postedToTeams || emailSent;

                await alertRecordService.RecordReceivedAlert(bug, alertSent);
            }
            else
            {
                logger.LogError(message: "Bug report queue triggered but unable to deserialize message.");
            }
        }

        private async Task<bool> SendEmailNotification(EmailRequestMessage? email)
        {
            if (email is not null)
            {
                try
                {
                    await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.EmailNotificationQueueName,
                        email);
                    return true;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Unable to send email.");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private async Task<bool> PostToTeams(BugReportMessage bug)
        {
            if (!_postToTeams)
            {
                logger.LogInformation("Posting to Teams is disabled.");
                return false;
            }

            if (string.IsNullOrEmpty(config.BugReportTeamsWebhookUrl))
            {
                logger.LogWarning("Teams Webhook is not configured - alert will not be posted to Teams. Please configure BugReportTeamsWebhookUrl setting.");
                return false;
            }

            var bugTeamsMessage = new TeamsWebhookMessage("Bug Report", bug.BugReportType.ToString());

            var details = bugTeamsMessage.Sections[0];
            details.AddNonEmptyFact(nameof(bug.UserName), bug.UserName);
            details.AddNonEmptyFact(nameof(bug.UserEmail), bug.UserEmail);
            details.AddNonEmptyFact(nameof(bug.UserOrganization), bug.UserOrganization);
            details.AddNonEmptyFact(nameof(bug.Description), bug.Description);
            details.AddNonEmptyFact(nameof(bug.Topics), bug.Topics);
            details.AddNonEmptyFact(nameof(bug.Workspaces), bug.Workspaces);
            details.AddNonEmptyFact(nameof(bug.URL), bug.URL);
            details.AddNonEmptyFact(nameof(bug.PortalLanguage), bug.PortalLanguage);
            details.AddNonEmptyFact(nameof(bug.PreferredLanguage), bug.PreferredLanguage);
            details.AddNonEmptyFact(nameof(bug.Timezone), bug.Timezone);
            details.AddNonEmptyFact(nameof(bug.UserAgent), bug.UserAgent);
            details.AddNonEmptyFact(nameof(bug.Resolution), bug.Resolution);
            details.AddNonEmptyFact(nameof(bug.LocalStorage), bug.LocalStorage);
            details.AddNonEmptyFact(nameof(bug.BugReportType), bug.BugReportType.ToString());
            
            try
            {
                using var httpClient = httpClientFactory.CreateClient();
                var response = await httpClient.PostAsJsonAsync(config.BugReportTeamsWebhookUrl, bugTeamsMessage);
                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation("Alert successfully posted to Teams");
                    return true;
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    logger.LogError($"Problem posting to Teams Webhook - status {response.StatusCode} - {content}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error posting alert to Teams");
                return false;
            }
        }

        private string GetIssueDescription(BugReportMessage bug)
        {
            return $"<strong>Issue submitted by:</strong> {bug.UserName}<br /><strong>Contact email:</strong> {bug.UserEmail}<br /><strong>Organization:</strong> {bug.UserOrganization}<br /><strong>Preferred Language:</strong> {bug.PreferredLanguage} <br /><strong>Time Zone:</strong> {bug.Timezone}<br /><br /><strong>Topics:</strong> {bug.Topics}<br /><strong>Workspace:</strong> {bug.Workspaces}<br /><strong>Description:</strong> {bug.Description}<br /><br /><strong>Portal Language:</strong> {bug.PortalLanguage}<br /><strong>Active URL:</strong> {bug.URL}<br /><strong>User Agent:</strong> {bug.UserAgent}<br /><strong>Resolution:</strong> {bug.Resolution}<br /><strong>Local Storage:</strong><br />{bug.LocalStorage}";
        }

        private EmailRequestMessage? BuildEmail(BugReportMessage bug, WorkItem? workItem)
        {
            if (!_sendEmailNotification)
            {
                logger.LogInformation("Email notifications are disabled.");
                return default;
            }

            try
            {
                var sendTo = new List<string>
                    { config.Email.AdminEmail ?? throw new MissingMemberException("Admin email not set.") };
                var url = workItem?.Url ?? "";
                var id = workItem?.Id.ToString() ?? "";

                Dictionary<string, string> subjectArgs = new()
                {
                    { "{title}", $"{bug.Topics} in {bug.Workspaces}" }
                };

                var description = GetIssueDescription(bug);

                Dictionary<string, string> bodyArgs = new()
                {
                    { "{id}", id },
                    { "{url}", url },
                    { "{description}", description }
                };

                var emailMessage = emailService.BuildEmail("bug_report.html", sendTo, new List<string>(), bodyArgs, subjectArgs);
                if (emailMessage is null)
                {
                    logger.LogError("Unable to build email notification.");
                }

                return emailMessage;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error building email notification.");
                return default;
            }
        }


        public JsonPatchDocument CreateIssue(BugReportMessage bug)
        {
            var organization = config.AzureDevOpsConfiguration.OrganizationName;
            var project = config.AzureDevOpsConfiguration.ProjectName;

            var title = $"{bug.Topics} in {bug.Workspaces}";
            var description = GetIssueDescription(bug);

            // Content of the issue. Possible additions: New tags (topics?), AssignedTo, State, Reason.
            var body = new JsonPatchDocument
            {
                new() { Operation = Operation.Add, Path = "/fields/System.Title", Value = title },
                new() { Operation = Operation.Add, Path = "/fields/System.Description", Value = description },
                new()
                {
                    Operation = Operation.Add, Path = "/fields/System.AreaPath", Value = $"{project}\\FSDH Support Team"
                },
                new() { Operation = Operation.Add, Path = "/fields/System.IterationPath", Value = $"{project}\\POC 2" },
                new()
                {
                    Operation = Operation.Add, Path = "/fields/System.Tags",
                    Value = $"UserSubmitted; {bug.UserName?.Replace(",", " ")};{bug.Topics}"
                },
                new() { Operation = Operation.Add, Path = "/fields/Submitted By", Value = bug.UserName },
                new() { Operation = Operation.Add, Path = "/fields/Email", Value = bug.UserEmail },
                new() { Operation = Operation.Add, Path = "/fields/Workspaces", Value = bug.Workspaces },
                new() { Operation = Operation.Add, Path = "/fields/Organization", Value = bug.UserOrganization },
                new() { Operation = Operation.Add, Path = "/fields/Timezone", Value = bug.Timezone },
                new() { Operation = Operation.Add, Path = "/fields/Preferred Language", Value = bug.PreferredLanguage },
                new()
                {
                    Operation = Operation.Add,
                    Path = "/relations/-",
                    Value = new
                    {
                        rel = "System.LinkTypes.Hierarchy-Reverse",
                        url =
                            $"https://dev.azure.com/{organization}/{project}/_apis/wit/workItems/{(int)bug.BugReportType}",
                        attributes = new { comment = "Parent work item for user generated issue" }
                    }
                }
            };

            return body;
        }

        private async Task<WorkItem?> PostIssue(JsonPatchDocument body)
        {
            if (!_postToDevops)
            {
                logger.LogInformation("Posting to DevOps is disabled.");
                return default;
            }

            var clientProvider = new AzureDevOpsClient(config.AzureDevOpsConfiguration);
            var client = await clientProvider.WorkItemClientAsync();
            var workItem = await client.CreateWorkItemAsync(body, config.AzureDevOpsConfiguration.ProjectName, "Issue");

            if (workItem is null)
            {
                logger.LogError("Unable to create issue in DevOps.");
            }
            else
            {
                logger.LogInformation("Successfully created issue in DevOps.");
            }

            return workItem;
        }
    }
}