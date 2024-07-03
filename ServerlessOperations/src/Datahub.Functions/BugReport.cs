using Azure.Messaging.ServiceBus;
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
        IAlertRecordService alertRecordService)
    {
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

                // Build the ADO issue
                var issue = CreateIssue(bug);

                // Post the issue to ADO and parse the response
                var workItem = await PostIssue(issue);

                // Build the email
                var email = BuildEmail(bug, workItem);

                if (email is not null)
                {
                    try
                    {
                        await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.EmailNotificationQueueName,
                            email);

                        await alertRecordService.RecordReceivedAlert(bug);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Unable to send email.");

                        await alertRecordService.RecordReceivedAlert(bug, false);
                    }
                }
                else
                {
                    logger.LogError("Unable to build email.");
                }
            }
            else
            {
                logger.LogError(message: "Bug report queue triggered but unable to deserialize message.");
            }
        }

        private EmailRequestMessage? BuildEmail(BugReportMessage bug, WorkItem? workItem)
        {
            var sendTo = new List<string>
                { config.Email.AdminEmail ?? throw new MissingMemberException("Admin email not set.") };
            var url = workItem?.Url ?? "";
            var id = workItem?.Id.ToString() ?? "";

            Dictionary<string, string> subjectArgs = new()
            {
                { "{title}", $"{bug.Topics} in {bug.Workspaces}" }
            };

            Dictionary<string, string> bodyArgs = new()
            {
                { "{id}", id },
                { "{url}", url },
                { "{description}", bug.Description }
            };

            return emailService.BuildEmail("bug_report.html", sendTo, new List<string>(), bodyArgs, subjectArgs);
        }


        public JsonPatchDocument CreateIssue(BugReportMessage bug)
        {
            var organization = config.AzureDevOpsConfiguration.OrganizationName;
            var project = config.AzureDevOpsConfiguration.ProjectName;

            var title = $"{bug.Topics} in {bug.Workspaces}";
            var description =
                $"<b>Issue submitted by:</b> {bug.UserName}<br /><b>Contact email:</b> {bug.UserEmail}<br /><b>Organization:</b> {bug.UserOrganization}<br /><b>Preferred Language:</b> {bug.PreferredLanguage} <br /><b>Time Zone:</b> {bug.Timezone}<br /><br /><b>Topics:</b> {bug.Topics}<br /><b>Workspace:</b> {bug.Workspaces}<br /><b>Description:</b> {bug.Description}<br /><br /><b>Portal Language:</b> {bug.PortalLanguage}<br /><b>Active URL:</b> {bug.URL}<br /><b>User Agent:</b> {bug.UserAgent}<br /><b>Resolution:</b> {bug.Resolution}<br /><b>Local Storage:</b><br />{bug.LocalStorage}";

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