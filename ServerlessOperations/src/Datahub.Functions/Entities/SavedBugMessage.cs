using Azure;
using Azure.Data.Tables;
using Datahub.Infrastructure.Queues.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Functions.Entities
{
    public class SavedBugMessage : ITableEntity
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
        public int BugReportType { get; set; }
        public string Description { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
