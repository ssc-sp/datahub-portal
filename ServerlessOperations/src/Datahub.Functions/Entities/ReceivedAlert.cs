using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Functions.Entities
{
    public class ReceivedAlert : ITableEntity
    {
        public string ReportIdentifier { get; set; }
        public string SavedBugMessageRowKey { get; set; }
        public bool EmailSent { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
