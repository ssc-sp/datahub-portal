using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Monitor;
using Azure.ResourceManager.Monitor.Models;
using Azure.ResourceManager.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Datahub.Application.Services.ResourceGroups;
using Datahub.Application.Services.Storage;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services.Storage
{
    public class WorkspaceStorageManagementService(
        ArmClient armClient,
        ILogger<WorkspaceStorageManagementService> logger,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        IWorkspaceResourceGroupsManagementService rgMgmtService)
        : IWorkspaceStorageManagementService
    {
        #region Implementations

        /// <inheritdoc />
        public async Task<double> GetStorageCapacity(string workspaceAcronym, List<string>? storageAccountIds = null)
        {
            logger.LogInformation("Getting storage capacity for workspace {WorkspaceAcronym}", workspaceAcronym);
            if (storageAccountIds is null)
            {
                storageAccountIds = await GetStorageAccountIds(workspaceAcronym);
            }
            logger.LogInformation("Storage account ids: {Join}", string.Join(", ", storageAccountIds));

            var today = DateTime.UtcNow;
            var dateFormat = "yyyy-MM-ddTHH:00:00.000Z";
            var fromDate = today.AddDays(-1).ToString(dateFormat);
            var toDate = today.ToString(dateFormat);

            var totalCapacity = 0.0;

            storageAccountIds.ForEach(id =>
            {
                var storageId = new ResourceIdentifier(id);
                var option = new ArmResourceGetMonitorMetricsOptions();
                option.Metricnames = "UsedCapacity";
                option.Aggregation = "average";
                option.Timespan = $"{fromDate}/{toDate}";
                option.Metricnamespace = "Microsoft.Storage/storageAccounts";
                option.ValidateDimensions = false;
                logger.LogInformation("Getting metrics for storage account with id {Id}", id);
                var metrics = armClient.GetMonitorMetrics(storageId, option).ToList();
                var timeseries = metrics[0].Timeseries.ToList()[0];
                var timeseriesData = timeseries.Data.ToList().Last(x => x.Average != null);
                var average = timeseriesData.Average;
                if (average is null)
                {
                    logger.LogError("Could not parse data from storage account with id {Id}", id);
                    throw new Exception($"Could not parse data from storage account with id {id}");
                }

                logger.LogInformation("Storage account with id {Id} has capacity {Average}", id, average);

                totalCapacity += average.Value;
            });
            logger.LogInformation("Total capacity for workspace {WorkspaceAcronym} is {TotalCapacity}",
                workspaceAcronym, totalCapacity);

            return totalCapacity;
        }

        /// <inheritdoc />
        public async Task<double> UpdateStorageCapacity(string workspaceAcronym, List<string>? storageAccountIds = null)
        {
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var date = DateTime.UtcNow.Date;
            var project = await ctx.Projects.FirstOrDefaultAsync(p => p.Project_Acronym_CD == workspaceAcronym);
            if (project is null)
            {
                logger.LogError("Project with acronym {WorkspaceAcronym} not found", workspaceAcronym);
                throw new Exception($"Project with acronym {workspaceAcronym} not found");
            }

            var projectAverage =
                await ctx.Project_Storage_Avgs.FirstOrDefaultAsync(p =>
                    p.ProjectId == project.Project_ID && p.Date == date.Date);
            if (projectAverage is null)
            {
                logger.LogInformation("Creating new project storage average for today");
                projectAverage = new Project_Storage { ProjectId = project.Project_ID };
                ctx.Project_Storage_Avgs.Add(projectAverage);
                await ctx.SaveChangesAsync();
            }

            var capacity = await GetStorageCapacity(workspaceAcronym, storageAccountIds);
            projectAverage.AverageCapacity = capacity;
            projectAverage.Date = date;
            projectAverage.CloudProvider = "azure";
            logger.LogInformation("Updating storage capacity for workspace {WorkspaceAcronym} to {Capacity}",
                workspaceAcronym, capacity);
            ctx.Project_Storage_Avgs.Update(projectAverage);
            await ctx.SaveChangesAsync();
            return capacity;
        }

        /// <inheritdoc />
        public bool CheckUpdateNeeded(string workspaceAcronym, DatahubProjectDBContext ctx)
        {
            var date = DateTime.UtcNow.Date;
            var project = ctx.Projects
                .AsNoTracking()
                .First(p => p.Project_Acronym_CD == workspaceAcronym);
            var projectAverage = ctx.Project_Storage_Avgs
                .AsNoTracking()
                .FirstOrDefault(p => p.ProjectId == project.Project_ID && p.Date == date.Date);
            if (projectAverage is null) return true;
            return false;
        }

        /// <inheritdoc />
        public async Task<string> MoveBlobToUsersContainerAsync(string scannedFileUri, string? connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Storage connection string is missing.");
            }

            if (!Uri.TryCreate(scannedFileUri, UriKind.Absolute, out var sourceUri))
            {
                throw new InvalidOperationException($"Invalid scanned file URI: {scannedFileUri}");
            }

            var path = sourceUri.AbsolutePath.TrimStart('/');
            var split = path.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);

            if (split.Length < 2)
            {
                throw new InvalidOperationException($"Invalid scanned file URI path: {scannedFileUri}");
            }

            var sourceContainerName = split[0];
            var blobName = split[1];
            var targetBlobPath = $"{IWorkspaceStorageManagementService.AzureExternalUsersContainerName}/{blobName}";

            if (sourceContainerName.Equals(IWorkspaceStorageManagementService.AzureExternalUsersContainerName, StringComparison.OrdinalIgnoreCase))
            {
                return targetBlobPath;
            }

            var serviceClient = new BlobServiceClient(connectionString);
            var sourceContainer = serviceClient.GetBlobContainerClient(sourceContainerName);
            var destinationContainer = serviceClient.GetBlobContainerClient(IWorkspaceStorageManagementService.AzureExternalUsersContainerName);

            var sourceBlob = sourceContainer.GetBlobClient(blobName);
            var destinationBlob = destinationContainer.GetBlobClient(blobName);

            await destinationContainer.CreateIfNotExistsAsync();
            await destinationBlob.DeleteIfExistsAsync();

            var sourceProperties = await sourceBlob.GetPropertiesAsync();
            var metadata = sourceProperties.Value.Metadata is { Count: > 0 }
                ? new Dictionary<string, string>(sourceProperties.Value.Metadata, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var copyOperation = await destinationBlob.StartCopyFromUriAsync(sourceBlob.Uri);
            await copyOperation.WaitForCompletionAsync();

            var destinationProperties = await destinationBlob.GetPropertiesAsync();
            if (destinationProperties.Value.CopyStatus != CopyStatus.Success)
            {
                throw new InvalidOperationException($"Copy to {IWorkspaceStorageManagementService.AzureExternalUsersContainerName} container failed with status {destinationProperties.Value.CopyStatus}.");
            }

            await destinationBlob.SetMetadataAsync(metadata);
            await sourceBlob.DeleteIfExistsAsync();
            return targetBlobPath;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets the storage account resource ids for a workspace
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym to search for</param>
        /// <returns>A list of resource identifiers, representing the storage accounts belonging to this workspace</returns>
        internal async Task<List<string>> GetStorageAccountIds(string workspaceAcronym)
        {
            using var ctx = await dbContextFactory.CreateDbContextAsync();

            var rgIds = await rgMgmtService.GetWorkspaceResourceGroupsIdentifiersAsync(workspaceAcronym);
            if (rgIds is null || rgIds.Count == 0)
            {
                throw new Exception($"No resource groups found for workspace {workspaceAcronym}");
            }
            var storageIds = new List<string>();

            foreach (var rgId in rgIds)
            {
                try
                {
                    var resourceGroup = armClient.GetResourceGroupResource(rgId);
                    if (resourceGroup == null)
                    {
                        logger.LogError("Resource group with id {RgId} not found", rgId);
                        throw new Exception($"Resource group with id {rgId} not found");
                    }

                    var response = await resourceGroup.GetAsync();
                    if (response.Value == null)
                    {
                        logger.LogError("Resource group with id {RgId} not found", rgId);
                        throw new Exception($"Resource group with id {rgId} not found");
                    }

                    var storageAccountsCollection = response.Value.GetStorageAccounts();
                    var storageAccountsPageable = storageAccountsCollection.GetAll();
                    var storageAccounts = storageAccountsPageable.ToList();

                    storageAccounts.ForEach(sa => { storageIds.Add(sa.Id.ToString()); });
                }
                catch (Exception e)
                {
                    logger.LogWarning("Skipping resource group with id {RgId}: {Message}", rgId, e.Message);
                }
            }

            return storageIds;
        }

        #endregion
    }
}
