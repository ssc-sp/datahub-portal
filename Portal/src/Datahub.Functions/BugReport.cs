﻿using System;
using System.Net.Http.Headers;
using System.Text;
using Azure;
using Azure.Storage.Queues.Models;
using Datahub.Core.Services.Security;
using Datahub.Functions.Services;
using Datahub.Infrastructure.Queues.Messages;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Datahub.Functions
{
    public class BugReport
    {
        private readonly ILogger<BugReport> _logger;
        private readonly IKeyVaultService _keyVaultService;
        private readonly AzureConfig _config;
        private readonly IEmailService _emailService;
        private readonly IMediator _mediator;

        public BugReport(ILogger<BugReport> logger, IKeyVaultService keyVaultService, AzureConfig config,
            IEmailService emailService, IMediator mediator)
        {
            _logger = logger;
            _keyVaultService = keyVaultService;
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
                var credentials = await _keyVaultService.GetSecret(_config.AdoConfig.PatSecretName);
                string devOpsUrl = _config.AdoConfig.URL;

                // Preemptively build the post url
                var url = devOpsUrl.Replace("{organization}", _config.AdoConfig.OrgName).Replace("{project}", _config.AdoConfig.ProjectName)
                    .Replace("{workItemTypeName}", "Issue");

                // Build the ADO issue
                var issue = await CreateIssue(bug);

                // Post the issue to ADO and parse the response
                var content = await PostIssue(issue, credentials, url);
                var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);

                // Build the email
                var email = BuildEmail(bug, response);

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

        public EmailRequestMessage? BuildEmail(BugReportMessage bug, Dictionary<string, object> response)
        {
            var sendTo = new List<string> { _config.Email.AdminEmail };
            var url = response["url"].ToString() ?? "";
            var id = response["id"].ToString() ?? "";

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


        public async Task<object> CreateIssue(BugReportMessage bug)
        {
            var organization = _config.AdoConfig.OrgName;
            var project = _config.AdoConfig.ProjectName;
            
            var title = $"{bug.Topics} in {bug.Workspaces}";
            var description =
                $"<b>Issue submitted by:</b> {bug.UserName}<br /><b>Contact email:</b> {bug.UserEmail}<br /><b>Organization:</b> {bug.UserOrganization}<br /><b>Preferred Language:</b> {bug.PreferredLanguage} <br /><b>Time Zone:</b> {bug.Timezone}<br /><br /><b>Topics:</b> {bug.Topics}<br /><b>Workspace:</b> {bug.Workspaces}<br /><b>Description:</b> {bug.Description}<br /><br /><b>Portal Language:</b> {bug.PortalLanguage}<br /><b>Active URL:</b> {bug.URL}<br /><b>User Agent:</b> {bug.UserAgent}<br /><b>Resolution:</b> {bug.Resolution}<br /><b>Local Storage:</b><br />{bug.LocalStorage}";

            // Content of the issue. Possible additions: New tags (topics?), AssignedTo, State, Reason.
            var body = new object[]
            {
                new { op = "add", path = "/fields/System.Title", value = title },
                new { op = "add", path = "/fields/System.Description", value = description },
                new { op = "add", path = "/fields/System.AreaPath", value = $"{project}\\FSDH Support Team" },
                new { op = "add", path = "/fields/System.IterationPath", value = $"{project}\\POC 2" },
                new
                {
                    op = "add", path = "/fields/System.Tags",
                    value = $"UserSubmitted; {bug.UserName.Replace(",", " ")};{bug.Topics}"
                },
                new { op = "add", path = "/fields/Submitted By", value = bug.UserName },
                new { op = "add", path = "/fields/Email", value = bug.UserEmail },
                new { op = "add", path = "/fields/Workspaces", value = bug.Workspaces },
                new { op = "add", path = "/fields/Organization", value = bug.UserOrganization },
                new { op = "add", path = "/fields/Timezone", value = bug.Timezone },
                new { op = "add", path = "/fields/Preferred Language", value = bug.PreferredLanguage },
                new
                {
                    op = "add",
                    path = "/relations/-",
                    value = new
                    {
                        rel = "System.LinkTypes.Hierarchy-Reverse",
                        url = $"https://dev.azure.com/{organization}/{project}/_apis/wit/workItems/{(int)bug.BugReportType}",
                        attributes = new { comment = "Parent work item for user generated issue" }
                    }
                }
            };

            return body;
        }

        public async Task<string> PostIssue(object body, string personalAccessToken, string postUrl)
        {
            var client = new HttpClient();
            var jsonContent = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8,
                "application/json-patch+json");
            string credentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($":{personalAccessToken}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var response = await client.PostAsync(postUrl, jsonContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Unable to create issue in DevOps.");
            }
            else
            {
                _logger.LogInformation("Successfully created issue in DevOps.");
            }

            return responseContent;
        }
    }
}