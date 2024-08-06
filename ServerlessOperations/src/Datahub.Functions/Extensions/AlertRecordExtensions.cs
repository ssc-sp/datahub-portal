using Datahub.Functions.Entities;
using Datahub.Infrastructure.Queues.Messages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Functions.Extensions
{
    public static class AlertRecordExtensions
    {
        public static SavedBugMessage CreateSavedBugMessage(this BugReportMessage bugReportMessage) => new()
        {
            BugReportType = (int)bugReportMessage.BugReportType,
            Description = bugReportMessage.Description,
            LocalStorage = bugReportMessage.LocalStorage,
            PortalLanguage = bugReportMessage.PortalLanguage,
            PreferredLanguage = bugReportMessage.PreferredLanguage,
            Resolution = bugReportMessage.Resolution,
            Timezone = bugReportMessage.Timezone,
            Topics = bugReportMessage.Topics,
            URL = bugReportMessage.URL,
            UserAgent = bugReportMessage.UserAgent,
            UserEmail = bugReportMessage.UserEmail,
            UserName = bugReportMessage.UserName,
            UserOrganization = bugReportMessage.UserOrganization,
            Workspaces = bugReportMessage.Workspaces,
            PartitionKey = bugReportMessage.BugReportType.ToString(),
            RowKey = Guid.NewGuid().ToString()
        };

        public static BugReportMessage DecodeBugReportMessage(this SavedBugMessage savedBugMessage) => 
            new(savedBugMessage.UserName, savedBugMessage.UserEmail, savedBugMessage.UserOrganization, 
                savedBugMessage.PortalLanguage, savedBugMessage.PreferredLanguage, savedBugMessage.Timezone, 
                savedBugMessage.Workspaces, savedBugMessage.Topics, savedBugMessage.URL, savedBugMessage.UserAgent, 
                savedBugMessage.Resolution, savedBugMessage.LocalStorage, 
                (BugReportTypes)savedBugMessage.BugReportType, savedBugMessage.Description);

        private static string GenerateReportId(string? topic, string? workspace) => $"{topic}.{workspace}";

        public static string GenerateInfrastructureReportIdentifier(this SavedBugMessage savedBugMessage) => GenerateReportId(savedBugMessage.Topics, savedBugMessage.Workspaces);

        public static string GenerateInfrastructureReportIdentifier(this BugReportMessage bugReportMessage) => GenerateReportId(bugReportMessage.Topics, bugReportMessage.Workspaces);

        public static ReceivedAlert GenerateAlertRecord(this SavedBugMessage savedBugMessage) => new()
        {
            ReportIdentifier = GenerateInfrastructureReportIdentifier(savedBugMessage),
            SavedBugMessageRowKey = savedBugMessage.RowKey,
            PartitionKey = savedBugMessage.PartitionKey,
            RowKey = Guid.NewGuid().ToString()
        };
    }
}
