using Azure;
using Azure.Data.Tables;
using Datahub.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Functions.Entities;

internal class AzureUploadedFileRecord : ITableEntity
{
    public string UploadBatchId { get; set; }
    public string TriageContainer { get; set; }
    public string TriageFilePath { get; set; }
    public string TargetContainer { get; set; }
    public string TargetFilePath { get; set; }
    public string UploadUser { get; set; }
    public AntivirusScanStatus ScanStatus { get; set; }

    public string PartitionKey { get => UploadUser; set => UploadUser = value; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
