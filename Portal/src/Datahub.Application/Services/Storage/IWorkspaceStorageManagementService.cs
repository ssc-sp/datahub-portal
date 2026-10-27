using Azure.Core;
using Datahub.Core.Model.Context;

namespace Datahub.Application.Services.Storage
{
    public interface IWorkspaceStorageManagementService
    {
        public const string AzureDefaultContainerName = "datahub";
        public const string AzureExternalUploadContainerName = "external-uploads";
        public const string AzureExternalUsersContainerName = "users";
        public const string AzureSharedContainerName = "shared";
        public const string AzureVirusScanEvidenceContainerName = "virus-scan-evidence";

        /// <summary>
        /// Queries the monitoring metrics of a storage account to get the used capacity
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <param name="storageAccountId">Optional storage account ids to use. If not provided, will be interpolated</param>
        /// <returns></returns>
        public Task<double> GetStorageCapacity(string workspaceAcronym, List<string>? storageAccountId = null);
        
        /// <summary>
        /// Update the storage capacity of a workspace in database
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <param name="storageAccountId">Optional storage account ids to use. If not provided, will be interpolated</param>
        /// <returns></returns>
        public Task<double?> UpdateStorageCapacity(string workspaceAcronym, List<string>? storageAccountId = null);

        /// <summary>
        /// Checks if a storage update is needed for a workspace
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym to check for</param>
        /// <param name="ctx">Project db context to use</param>
        /// <returns>True if it is needed, false otherwise</returns>
        public bool CheckUpdateNeeded(string workspaceAcronym, DatahubProjectDBContext ctx);

        /// <summary>
        /// Uploads a virus-scan evidence blob to the workspace storage account and returns a read SAS URL.
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym used to resolve the storage account.</param>
        /// <param name="containerName">The target container name.</param>
        /// <param name="blobName">The target blob path relative to the container.</param>
        /// <param name="fileStream">The file contents to upload.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The read SAS URL for the uploaded blob.</returns>
        public Task<string> UploadVirusScanEvidenceAsync(string workspaceAcronym, string containerName, string blobName, Stream fileStream, CancellationToken cancellationToken = default);

        /// <summary>
        /// Copies a blob from its current container into the external users container and preserves metadata.
        /// </summary>
        /// <param name="scannedFileUri">The absolute URI of the source blob.</param>
        /// <param name="credential">The Azure credential used to access the storage account.</param>
        /// <returns>The target blob path in the form "users/{blobName}" and null if the source doesn't exist</returns>
        public Task<string?> MoveBlobToUsersContainerAsync(string scannedFileUri, TokenCredential credential);
    }
}
