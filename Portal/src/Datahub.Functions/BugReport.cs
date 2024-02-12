using System;
using System.Net.Http.Headers;
using System.Text;
using Azure.Storage.Queues.Models;
using Datahub.Application.Services.Security;
using Datahub.Core.Services.Security;
using Datahub.Functions.Providers;
using Datahub.Functions.Services;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services.Security;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Datahub.Functions
{
    public class BugReport
    {
        private readonly ILogger<BugReport> _logger;
        private readonly AzureConfig _config;
        private readonly IEmailService _emailService;
        private readonly IMediator _mediator;

        public BugReport(ILogger<BugReport> logger, AzureConfig config,
            IEmailService emailService, IMediator mediator)
        {
            _logger = logger;
            _config = config;
            _emailService = emailService;
            _mediator = mediator;
        }

        [Function("BugReport")]
        public async Task Run(
            [QueueTrigger("bug-report", Connection = "DatahubStorageConnectionString")]
            QueueMessage queueItem)
        {
            _logger.LogInformation($"Bug report queue triggered: {queueItem.MessageText}");

            // We deserialize the queue message into a BugReportMessage object
            var bug = JsonSerializer.Deserialize<BugReportMessage>(queueItem.MessageText);

            if (bug != null)
            {
                // We set up the title and description to be submitted in the issue.


                // Retrieve ADO information
                string devOpsUrl = _config.AdoConfig.URL;

                // Preemptively build the post url
                var url = devOpsUrl.Replace("{organization}", _config.AdoConfig.OrgName).Replace("{project}", _config.AdoConfig.ProjectName)
                    .Replace("{workItemTypeName}", "Issue");

                // Build the ADO issue
                var issue = await CreateIssue(bug);

                // Post the issue to ADO and parse the response
                var workItem = await PostIssue(issue, url);

                // Build the email
                var email = BuildEmail(bug, workItem);

                if (email is not null)
                {
                    await _mediator.Send(email);
                }
                else
                {
                    _logger.LogError("Unable to build email.");
                }
            }
            else
            {
                _logger.LogError(message: "Bug report queue triggered but unable to deserialize message.");
            }
        }

        public EmailRequestMessage? BuildEmail(BugReportMessage bug, WorkItem workItem)
        {
            var sendTo = new List<string> { _config.Email.AdminEmail };
            var url = workItem.Url ?? ""; 
            var id = workItem.Id.ToString() ?? "";

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

            return _emailService.BuildEmail("bug_report.html", sendTo, new List<string>(), bodyArgs, subjectArgs);
        }


        public async Task<JsonPatchDocument> CreateIssue(BugReportMessage bug)
        {
            var organization = _config.AdoConfig.OrgName;
            var project = _config.AdoConfig.ProjectName;
            
            var title = $"{bug.Topics} in {bug.Workspaces}";
            var description =
                $"<b>Issue submitted by:</b> {bug.UserName}<br /><b>Contact email:</b> {bug.UserEmail}<br /><b>Organization:</b> {bug.UserOrganization}<br /><b>Preferred Language:</b> {bug.PreferredLanguage} <br /><b>Time Zone:</b> {bug.Timezone}<br /><br /><b>Topics:</b> {bug.Topics}<br /><b>Workspace:</b> {bug.Workspaces}<br /><b>Description:</b> {bug.Description}<br /><br /><b>Portal Language:</b> {bug.PortalLanguage}<br /><b>Active URL:</b> {bug.URL}<br /><b>User Agent:</b> {bug.UserAgent}<br /><b>Resolution:</b> {bug.Resolution}<br /><b>Local Storage:</b><br />{bug.LocalStorage}";

            // Content of the issue. Possible additions: New tags (topics?), AssignedTo, State, Reason.
            var body = new JsonPatchDocument
            {
                new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.Title", Value = title },
                new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.Description", Value = description },
                new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.AreaPath", Value = $"{project}\\FSDH Support Team" },
                new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.IterationPath", Value = $"{project}\\POC 2" },
                new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.Tags", Value = $"UserSubmitted; {bug.UserName.Replace(",", " ")};{bug.Topics}" },
                new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/Submitted By", Value = bug.UserName },
                new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/Email", Value = bug.UserEmail },
                new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/Workspaces", Value = bug.Workspaces },
                new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/Organization", Value = bug.UserOrganization },
                new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/Timezone", Value = bug.Timezone },
                new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/Preferred Language", Value = bug.PreferredLanguage },
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/relations/-",
                    Value = new
                    {
                        rel = "System.LinkTypes.Hierarchy-Reverse",
                        url = $"https://dev.azure.com/{organization}/{project}/_apis/wit/workItems/{(int)bug.BugReportType}",
                        attributes = new { comment = "Parent work item for user generated issue" }
                    }
                }
            };

            return body;
        }

        public async Task<WorkItem> PostIssue(JsonPatchDocument body, string postUrl)
        {
            var clientProvider = new AdoClientProvider(_config);
            var client = await clientProvider.GetWorkItemClient();
            var workItem = await client.CreateWorkItemAsync(body, _config.AdoConfig.ProjectName, "Issue");

            if (workItem is null)
            {
                _logger.LogError("Unable to create issue in DevOps.");
            }
            else
            {
                _logger.LogInformation("Successfully created issue in DevOps.");
            }

            return workItem;
        }
    }
}