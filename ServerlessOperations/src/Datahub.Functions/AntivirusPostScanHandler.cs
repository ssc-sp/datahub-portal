using Azure.Messaging.ServiceBus;
using Datahub.Core.Data;
using Datahub.Functions.Entities;
using Datahub.Functions.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Application.Services;
using Azure.Data.Tables;

namespace Datahub.Functions;

public class AntivirusPostScanHandler(ILoggerFactory loggerFactory, IProjectStorageConfigurationService projectStorageConfigurationService)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<AntivirusPostScanHandler>();

    public readonly string FILE_UPLOAD_RECORD_TABLE_NAME = "FileUploadRecords";

    /// <summary>
    /// Azure Function that consumes messages from the antivirus post-scan queue.
    /// </summary>
    /// <param name="message">The ServiceBusReceivedMessage containing the antivirus scan results.</param>
    /// <returns>An IActionResult containing the processing result.</returns>
    [Function("AntivirusPostScanQueueHandler")]
    public async Task<IActionResult> RunAntivirusPostScanQueue(
        [ServiceBusTrigger(QueueConstants.AntivirusPostScanQueueName,
            Connection = "DatahubServiceBus:ConnectionString")]
        ServiceBusReceivedMessage message)
    {
        _logger.LogInformation($"C# Queue trigger function processed: {message.Body}");

        var request = await message.DeserializeAndUnwrapMessageAsync<AntivirusPostScanMessage>();

        return await ProcessRequest(request);
    }

    /// <summary>
    /// Azure Function that can be called via HTTP for debugging antivirus post-scan processing.
    /// </summary>
    /// <param name="req">The HTTP request containing the antivirus scan results.</param>
    /// <returns>An IActionResult containing the processing result.</returns>
    [Function("AntivirusPostScanDebugHttp")]
    public async Task<IActionResult> RunAntivirusPostScanHttp(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var request = System.Text.Json.JsonSerializer.Deserialize<AntivirusPostScanMessage>(requestBody);

        return await ProcessRequest(request);
    }

    /// <summary>
    /// Processes the antivirus post-scan message.
    /// </summary>
    /// <param name="request">The deserialized antivirus post-scan message.</param>
    /// <returns>An IActionResult indicating the processing status.</returns>
    public async Task<IActionResult> ProcessRequest(AntivirusPostScanMessage? request)
    {
        if (request is null)
        {
            _logger.LogError("Request could not be deserialized from message.");
            return new BadRequestResult();
        }

        try
        {
            _logger.LogInformation(
                "Processing antivirus scan result. Workspace: {WorkspaceAcronym}, BatchId: {UploadBatchId}, User: {User}, Result: {Result}, Timestamp: {Timestamp}",
                request.WorkspaceAcronym,
                request.UploadBatchId,
                request.UploadUser,
                request.Result,
                request.Timestamp);

            return request.Result switch
            {
                AntivirusScanStatus.Success => await HandleSuccessfulScanAsync(request),
                AntivirusScanStatus.Virus => await HandleVirusDetectedAsync(request),
                AntivirusScanStatus.ScanError => await HandleScanErrorAsync(request),
                AntivirusScanStatus.Unscanned => HandleUnexpectedStatus(request),
                AntivirusScanStatus.Scanning => HandleUnexpectedStatus(request),
                _ => HandleUnexpectedStatus(request)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing antivirus post-scan message for workspace {WorkspaceAcronym}, batch {UploadBatchId}",
                request.WorkspaceAcronym, request.UploadBatchId);
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    private async Task<IActionResult> HandleSuccessfulScanAsync(AntivirusPostScanMessage request)
    {
        _logger.LogInformation("Handling successful scan for workspace {WorkspaceAcronym}, batch {UploadBatchId}",
            request.WorkspaceAcronym, request.UploadBatchId);

        var tableClient = await CreateTableClient(request.WorkspaceAcronym, FILE_UPLOAD_RECORD_TABLE_NAME);
        var storageManager = await GetAzureCloudStorageManagerAsync(request.WorkspaceAcronym);

        var records = await GetUploadRecordsForBatchAsync(tableClient, request.UploadUser, request.UploadBatchId);

        if (records.Count == 0)
        {
            _logger.LogWarning("No upload records found for workspace {WorkspaceAcronym}, user {User}, batch {UploadBatchId}",
                request.WorkspaceAcronym, request.UploadUser, request.UploadBatchId);
            return new NotFoundObjectResult("No upload records found for the specified batch");
        }

        await MoveFilesFromTriageToTargetAsync(storageManager, records);
        await UpdateRecordStatusAsync(tableClient, records, AntivirusScanStatus.Success);

        _logger.LogInformation("Successfully processed {Count} files for workspace {WorkspaceAcronym}, batch {UploadBatchId}",
            records.Count, request.WorkspaceAcronym, request.UploadBatchId);

        return CreateSuccessResult("Files successfully scanned and moved to target location", request, records);
    }

    private async Task<IActionResult> HandleVirusDetectedAsync(AntivirusPostScanMessage request)
    {
        _logger.LogWarning("Virus detected for workspace {WorkspaceAcronym}, batch {UploadBatchId}, user {User}",
            request.WorkspaceAcronym, request.UploadBatchId, request.UploadUser);

        var tableClient = await CreateTableClient(request.WorkspaceAcronym, FILE_UPLOAD_RECORD_TABLE_NAME);
        var storageManager = await GetAzureCloudStorageManagerAsync(request.WorkspaceAcronym);

        var records = await GetUploadRecordsForBatchAsync(tableClient, request.UploadUser, request.UploadBatchId);

        if (records.Count == 0)
        {
            _logger.LogWarning("No upload records found for workspace {WorkspaceAcronym}, user {User}, batch {UploadBatchId}",
                request.WorkspaceAcronym, request.UploadUser, request.UploadBatchId);
            return new NotFoundObjectResult("No upload records found for the specified batch");
        }

        await DeleteFilesFromTriageAsync(storageManager, records);
        await LockOutUserAsync(request.WorkspaceAcronym, request.UploadUser);
        await SendVirusNotificationAsync(request.WorkspaceAcronym, request.UploadUser, request.UploadBatchId);
        await UpdateRecordStatusAsync(tableClient, records, AntivirusScanStatus.Virus);

        _logger.LogInformation("Completed virus handling for workspace {WorkspaceAcronym}, batch {UploadBatchId}",
            request.WorkspaceAcronym, request.UploadBatchId);

        return CreateSuccessResult("Virus detected. Files deleted and user notified.", request, records);
    }

    private async Task<IActionResult> HandleScanErrorAsync(AntivirusPostScanMessage request)
    {
        _logger.LogError("Scan error occurred for workspace {WorkspaceAcronym}, batch {UploadBatchId}",
            request.WorkspaceAcronym, request.UploadBatchId);

        var tableClient = await CreateTableClient(request.WorkspaceAcronym, FILE_UPLOAD_RECORD_TABLE_NAME);
        var storageManager = await GetAzureCloudStorageManagerAsync(request.WorkspaceAcronym);

        var records = await GetUploadRecordsForBatchAsync(tableClient, request.UploadUser, request.UploadBatchId);

        if (records.Count == 0)
        {
            _logger.LogWarning("No upload records found for workspace {WorkspaceAcronym}, user {User}, batch {UploadBatchId}",
                request.WorkspaceAcronym, request.UploadUser, request.UploadBatchId);
            return new NotFoundObjectResult("No upload records found for the specified batch");
        }

        await DeleteFilesFromTriageAsync(storageManager, records);
        await UpdateRecordStatusAsync(tableClient, records, AntivirusScanStatus.ScanError);

        _logger.LogInformation("Completed scan error handling for workspace {WorkspaceAcronym}, batch {UploadBatchId}",
            request.WorkspaceAcronym, request.UploadBatchId);

        return CreateSuccessResult("Scan error occurred. Files deleted from triage.", request, records);
    }

    private BadRequestObjectResult HandleUnexpectedStatus(AntivirusPostScanMessage request)
    {
        string statusName = request.Result.ToString();

        _logger.LogWarning("Received unexpected scan status '{Status}' for workspace {WorkspaceAcronym}, batch {UploadBatchId}",
            statusName, request.WorkspaceAcronym, request.UploadBatchId);

        return new(new
        {
            Message = $"Unexpected scan status: {statusName}",
            WorkspaceAcronym = request.WorkspaceAcronym,
            UploadBatchId = request.UploadBatchId
        });
    }

    private async Task<List<AzureUploadedFileRecord>> GetUploadRecordsForBatchAsync(TableClient tableClient, string uploadUser, string uploadBatchId)
    {
        var records = new List<AzureUploadedFileRecord>();

        await foreach (var entity in tableClient.QueryAsync<AzureUploadedFileRecord>(
            filter: $"PartitionKey eq '{uploadUser}' and UploadBatchId eq '{uploadBatchId}'"))
        {
            records.Add(entity);
        }

        _logger.LogInformation("Found {Count} upload records for user {User}, batch {UploadBatchId}",
            records.Count, uploadUser, uploadBatchId);

        return records;
    }

    private async Task UpdateRecordStatusAsync(TableClient tableClient, List<AzureUploadedFileRecord> records, AntivirusScanStatus status)
    {
        foreach (var record in records)
        {
            record.ScanStatus = status;
            await tableClient.UpdateEntityAsync(record, record.ETag);
        }

        _logger.LogInformation("Updated {Count} records to status {Status}",
            records.Count, status);
    }

    private async Task MoveFilesFromTriageToTargetAsync(AzureCloudStorageManager storageManager, List<AzureUploadedFileRecord> records)
    {
        foreach (var record in records)
        {
            _logger.LogInformation("Moving file from {TriageContainer}/{TriagePath} to {TargetContainer}/{TargetPath}",
                record.TriageContainer, record.TriageFilePath, record.TargetContainer, record.TargetFilePath);

            var success = await storageManager.MoveFileBetweenContainersAsync(
                record.TriageContainer,
                record.TriageFilePath,
                record.TargetContainer,
                record.TargetFilePath);

            if (!success)
            {
                _logger.LogError("Failed to move file from {TriageContainer}/{TriagePath} to {TargetContainer}/{TargetPath}",
                    record.TriageContainer, record.TriageFilePath, record.TargetContainer, record.TargetFilePath);
                throw new InvalidOperationException($"Failed to move file: {record.TriageFilePath}");
            }
        }

        _logger.LogInformation("Moved {Count} files from triage to target location", records.Count);
    }

    private async Task DeleteFilesFromTriageAsync(AzureCloudStorageManager storageManager, List<AzureUploadedFileRecord> records)
    {
        foreach (var record in records)
        {
            _logger.LogInformation("Deleting file from triage: {TriageContainer}/{TriagePath}",
                record.TriageContainer, record.TriageFilePath);

            var success = await storageManager.DeleteFileAsync(record.TriageContainer, record.TriageFilePath);

            if (!success)
            {
                _logger.LogWarning("Failed to delete file from triage: {TriageContainer}/{TriagePath}",
                    record.TriageContainer, record.TriageFilePath);
            }
        }

        _logger.LogInformation("Deleted {Count} files from triage storage", records.Count);
    }

    private Task SendVirusNotificationAsync(string workspaceAcronym, string uploadUser, string uploadBatchId)
    {
        // TODO: Implement actual notification logic
        // This should send notifications to:
        // 1. The user who uploaded the files
        // 2. The workspace owner/administrators
        _logger.LogInformation("TODO: Send virus notification for workspace {WorkspaceAcronym}, user {User}, batch {UploadBatchId}",
            workspaceAcronym, uploadUser, uploadBatchId);

        return Task.CompletedTask;
    }

    private Task LockOutUserAsync(string workspaceAcronym, string uploadUser)
    {
        // TODO: Implement actual user lockout logic
        // This should temporarily prevent the user from uploading files
        _logger.LogInformation("TODO: Lock out user {User} from workspace {WorkspaceAcronym}",
            uploadUser, workspaceAcronym);

        return Task.CompletedTask;
    }

    private static OkObjectResult CreateSuccessResult(string message, AntivirusPostScanMessage request, List<AzureUploadedFileRecord> records) =>
    new(new
    {
        Message = message,
        WorkspaceAcronym = request.WorkspaceAcronym,
        UploadBatchId = request.UploadBatchId,
        FilesProcessed = records.Count
    });

    private async Task<(string accountName, string accountKey)> GetWorkspaceStorageCredentialsAsync(string workspaceAcronym)
    {
        string accountName = projectStorageConfigurationService.GetProjectStorageAccountName(workspaceAcronym);
        string accountKey = await projectStorageConfigurationService.GetProjectStorageAccountKey(workspaceAcronym);
        return (accountName, accountKey);
    }

    private async Task<AzureCloudStorageManager> GetAzureCloudStorageManagerAsync(string workspaceAcronym)
    {
        var (accountName, accountKey) = await GetWorkspaceStorageCredentialsAsync(workspaceAcronym);
        return new AzureCloudStorageManager(accountName, accountKey);
    }

    private async Task<TableClient> CreateTableClient(string workspaceAcronym, string tableName)
    {
        var (accountName, accountKey) = await GetWorkspaceStorageCredentialsAsync(workspaceAcronym);
        var connString = AzureStorageUtils.BuildAzureStorageConnectionString(accountName, accountKey);
        var client = new TableClient(connString, tableName);
        await client.CreateIfNotExistsAsync();
        return client;
    }
}
