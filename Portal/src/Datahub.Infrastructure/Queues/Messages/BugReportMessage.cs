using Datahub.Shared.Messaging;
using MediatR;
using Newtonsoft.Json;

namespace Datahub.Infrastructure.Queues.Messages
{
    public enum BugReportTypes
    {
        SupportRequest = 1230,
        SystemError = 1700,
        InfrastructureError = 1699
    }
    public class BugReportMessage : IRequest, IQueueMessage
    {
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserOrganization { get; set; }
        public string? PortalLanguage { get; set; }
        public string? PreferredLanguage { get; set; }
        public string? Timezone { get; set; }
        public string? Workspaces { get; set; }
        public string? Topics { get; set; }
        public string? URL { get; set; }
        public string? UserAgent { get; set; }
        public string? Resolution { get; set; }
        public string? LocalStorage { get; set; }
        public BugReportTypes BugReportType { get; set; }
        public string? Description { get; set; }
        public string Content {  get => JsonConvert.SerializeObject(this); }
        public string ConfigPathOrQueueName { get => "bug-report"; }

        public BugReportMessage(string? userName, string? userEmail, string? userOrganization, string? portalLanguage, string? preferredLanguage, string? timezone, string? workspaces, string? topics, string? url, string? userAgent, string? resolution, string? localStorage, BugReportTypes bugReportType, string? description)
        {
            UserName = userName;
            UserEmail = userEmail;
            UserOrganization = userOrganization;
            PortalLanguage = portalLanguage;
            PreferredLanguage = preferredLanguage;
            Timezone = timezone;
            Workspaces = workspaces;
            Topics = topics;
            URL = url;
            UserAgent = userAgent;
            Resolution = resolution;
            LocalStorage = localStorage;
            BugReportType = bugReportType;
            Description = description;
        }
    }
}