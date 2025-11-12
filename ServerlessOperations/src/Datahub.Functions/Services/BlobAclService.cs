using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions.Services;

/// <summary>
/// Implementation of blob ACL service for managing access control on Data Lake Storage Gen2 blobs
/// </summary>
public class BlobAclService : IBlobAclService
{
    private readonly ILogger<BlobAclService> _logger;
    private readonly DatahubProjectDBContext _dbContext;
    private readonly AzureConfig _azureConfig;

    public BlobAclService(
        ILogger<BlobAclService> logger,
        DatahubProjectDBContext dbContext,
        AzureConfig azureConfig)
    {
        _logger = logger;
        _dbContext = dbContext;
        _azureConfig = azureConfig;
    }

    /// <inheritdoc/>
    public async Task GrantWorkspaceMembersReadAccessAsync(Uri blobUri, string workspaceAcronym)
    {
        try
        {
            // Get workspace and its members from database
            var workspace = await _dbContext.Projects
                .Include(p => p.UserRoles)
                .ThenInclude(u => u.PortalUser)
                .FirstOrDefaultAsync(p => p.Project_Acronym_CD == workspaceAcronym);

            if (workspace == null)
            {
                _logger.LogWarning("Workspace not found: {WorkspaceAcronym}", workspaceAcronym);
                return;
            }

            // Get active workspace members (exclude removed users)
            var activeMembers = workspace.UserRoles
                .Where(u => u.RoleId != (int)Project_Role.RoleNames.Removed)
                .Select(u => u.PortalUser?.GraphGuid)
                .Where(guid => !string.IsNullOrEmpty(guid))
                .Cast<string>() // Cast to non-nullable string after filtering
                .ToList();

            if (!activeMembers.Any())
            {
                _logger.LogInformation("No active members found for workspace: {WorkspaceAcronym}", workspaceAcronym);
                return;
            }

            _logger.LogInformation("Found {Count} active members in workspace {WorkspaceAcronym}", 
                activeMembers.Count, workspaceAcronym);

            // Get Data Lake file client for the blob
            var fileClient = GetDataLakeFileClient(blobUri);

            // Get current ACL
            var accessControl = await fileClient.GetAccessControlAsync();
            var currentAcl = accessControl.Value.AccessControlList.ToList();

            _logger.LogDebug("Current ACL has {Count} entries", currentAcl.Count);

            // Update ACL to include read permissions for all workspace members
            var updatedAcl = UpdateAclWithWorkspaceMembers(currentAcl, activeMembers);

            // Set the updated ACL
            await fileClient.SetAccessControlListAsync(updatedAcl);

            _logger.LogInformation("Successfully granted read access to {Count} workspace members for blob: {BlobUri}", 
                activeMembers.Count, blobUri);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to grant workspace members read access for blob: {BlobUri}", blobUri);
            throw;
        }
    }

    /// <summary>
    /// Updates the ACL list to include read permissions for workspace members
    /// </summary>
    private List<PathAccessControlItem> UpdateAclWithWorkspaceMembers(
        List<PathAccessControlItem> currentAcl,
        List<string> memberObjectIds)
    {
        var aclDictionary = currentAcl.ToDictionary(
            item => $"{item.AccessControlType}:{item.EntityId}",
            item => item
        );

        foreach (var objectId in memberObjectIds)
        {
            var key = $"user:{objectId}";
            
            if (!aclDictionary.ContainsKey(key))
            {
                // Add read permission for this user
                var newAclItem = PathAccessControlItem.Parse($"user:{objectId}:r--");
                aclDictionary[key] = newAclItem;
                _logger.LogDebug("Adding read permission for user: {ObjectId}", objectId);
            }
            else
            {
                // User already has an ACL entry, ensure they have at least read permission
                var existingItem = aclDictionary[key];
                if (!existingItem.Permissions.HasFlag(RolePermissions.Read))
                {
                    var updatedPermissions = existingItem.Permissions | RolePermissions.Read;
                    var updatedAclItem = PathAccessControlItem.Parse($"user:{objectId}:{FormatPermissions(updatedPermissions)}");
                    aclDictionary[key] = updatedAclItem;
                    _logger.LogDebug("Updated permissions for user: {ObjectId}", objectId);
                }
            }
        }

        return aclDictionary.Values.ToList();
    }

    /// <summary>
    /// Formats RolePermissions into ACL string format (e.g., "rwx", "r--", etc.)
    /// </summary>
    private string FormatPermissions(RolePermissions permissions)
    {
        var read = permissions.HasFlag(RolePermissions.Read) ? "r" : "-";
        var write = permissions.HasFlag(RolePermissions.Write) ? "w" : "-";
        var execute = permissions.HasFlag(RolePermissions.Execute) ? "x" : "-";
        return $"{read}{write}{execute}";
    }

    /// <summary>
    /// Creates a DataLakeFileClient from a blob URI
    /// </summary>
    private DataLakeFileClient GetDataLakeFileClient(Uri blobUri)
    {
        // Extract storage account, container, and blob path from URI
        var host = blobUri.Host;
        var accountName = host.Split('.')[0];
        
        // Get the path segments: /container/path/to/file
        var pathSegments = blobUri.AbsolutePath.TrimStart('/').Split('/', 2);
        var containerName = pathSegments[0];
        var blobPath = pathSegments.Length > 1 ? pathSegments[1] : string.Empty;

        // Create Data Lake service client
        // Note: You may need to use different authentication depending on your setup
        var serviceUri = new Uri($"https://{accountName}.dfs.core.windows.net");
        var serviceClient = new DataLakeServiceClient(serviceUri, _azureConfig.GetAzureCredential());
        
        var fileSystemClient = serviceClient.GetFileSystemClient(containerName);
        return fileSystemClient.GetFileClient(blobPath);
    }
}
