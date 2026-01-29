using Azure.Messaging.ServiceBus;
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
    /// Azure Function that consumes messages from the antivirus post-scan storage queue.
    /// </summary>
    /// <param name="messageBody">The queue message body containing the antivirus scan results.</param>
    /// <returns>An IActionResult containing the processing result.</returns>
    [Function("AntivirusPostScanStorageQueueHandler")]
    public async Task<IActionResult> RunAntivirusPostScanStorageQueue(
        [QueueTrigger(QueueConstants.AntivirusPostScanQueueName,
            Connection = "AzureWebJobsStorage")]
        string messageBody)
    {
        _logger.LogInformation($"C# Storage Queue trigger function processed: {messageBody}");

        var request = System.Text.Json.JsonSerializer.Deserialize<AntivirusPostScanMessage>(messageBody);

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
                "Processing antivirus scan result. Workspace: {WorkspaceAcronym}, BatchId: {UploadBatchId}, Result: {Result}, Timestamp: {Timestamp}",
                request.WorkspaceAcronym,
                request.UploadBatchId,
                request.Result,
                request.Timestamp);

            await TestStorageAccess(request.WorkspaceAcronym);
            // TODO: Implement actual processing logic here
            // For example:
            // - Update database with scan results
            // - Send notifications if virus detected
            // - Update file metadata
            // - Trigger follow-up actions based on result

            return new OkObjectResult(new
            {
                Message = "Antivirus post-scan message processed successfully",
                WorkspaceAcronym = request.WorkspaceAcronym,
                UploadBatchId = request.UploadBatchId,
                Result = request.Result.ToString(),
                Timestamp = request.Timestamp
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing antivirus post-scan message");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    private async Task TestStorageAccess(string workspaceAcronym)
    {
        var storageManager = await GetAzureCloudStorageManagerAsync(workspaceAcronym);
        var containers = await storageManager.GetContainersAsync();
        _logger.LogInformation($"Storage containers for workspace {workspaceAcronym}: {string.Join(", ", containers)}");

        if (containers.Contains("datahub"))
        {
            var folders = await storageManager.ListFoldersAsync("datahub");
            _logger.LogInformation($"Folders in 'datahub' container: {string.Join(", ", folders)}");
        }
    }

    private async Task<(string accountName, string accountKey)> GetWorkspaceStorageCredentialsAsync(string workspaceAcronym)
    {
        string accountName = projectStorageConfigurationService.GetProjectStorageAccountName(workspaceAcronym);
        string accountKey = await projectStorageConfigurationService.GetProjectStorageAccountKey(workspaceAcronym);
        return (accountName, accountKey);
    }

    private async Task<AzureCloudStorageManager> GetAzureCloudStorageManagerAsync(string workspaceAcronym)
    {
        //string accountName = projectStorageConfigurationService.GetProjectStorageAccountName(workspaceAcronym);
        //string accountKey = await projectStorageConfigurationService.GetProjectStorageAccountKey(workspaceAcronym);
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
