using Azure;
using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Datahub.Application.Services;
using Datahub.Application.Services.Storage;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services.Storage;

public class WorkspaceAclService : IWorkspaceAclService
{
    private readonly ILogger<WorkspaceAclService> _logger;
    private readonly IProjectStorageConfigurationService _storageConfig;
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
    private const string ContainerName = "datahub";

    public WorkspaceAclService(
        ILogger<WorkspaceAclService> logger,
        IProjectStorageConfigurationService storageConfig,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
    {
        _logger = logger;
        _storageConfig = storageConfig;
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    /// Normalizes the path for Azure Data Lake operations. Root "/" becomes empty string.
    /// </summary>
    private static string NormalizePath(string path)
    {
        return path == "/" ? "" : path;
    }

    public async Task<Datahub_Project?> GetWorkspaceAsync(string workspaceAcronym)
    {
        await using var ctx = await _dbContextFactory.CreateDbContextAsync();
        return await ctx.Projects
            .Include(p => p.UserRoles)
            .ThenInclude(ur => ur.PortalUser)
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == workspaceAcronym);
    }

    public async Task<List<string>> GetWorkspaceMemberIdsAsync(string workspaceAcronym)
    {
        var workspace = await GetWorkspaceAsync(workspaceAcronym);
        if (workspace == null)
        {
            _logger.LogWarning("Workspace {Workspace} not found", workspaceAcronym);
            return new List<string>();
        }

        var memberIds = workspace.UserRoles
            .Where(ur => ur.PortalUser?.GraphGuid != null)
            .Select(ur => ur.PortalUser!.GraphGuid!)
            .ToList();

        _logger.LogInformation("Found {Count} members for workspace {Workspace}", memberIds.Count, workspaceAcronym);
        return memberIds;
    }

    public async Task<int> ApplyWorkspaceMemberAclsAsync(string workspaceAcronym, string filePath, string permissions = "r-x", bool recursive = true)
    {
        var memberIds = await GetWorkspaceMemberIdsAsync(workspaceAcronym);
        if (memberIds.Count == 0)
        {
            _logger.LogWarning("No members found for workspace {Workspace}", workspaceAcronym);
            return 0;
        }

        try
        {
            var serviceClient = await GetDataLakeServiceClientAsync(workspaceAcronym);
            var fileSystemClient = serviceClient.GetFileSystemClient(ContainerName);

            // Get the path client (could be file or directory)
            var pathClient = fileSystemClient.GetDirectoryClient(NormalizePath(filePath));
            
            // Check if it exists as a directory
            bool isDirectory = false;
            try
            {
                await pathClient.GetPropertiesAsync();
                isDirectory = true;
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == "PathNotFound")
            {
                // Try as a file
                var fileClient = fileSystemClient.GetFileClient(NormalizePath(filePath));
                await fileClient.GetPropertiesAsync();
            }

            int updateCount = 0;

            if (isDirectory && recursive)
            {
                updateCount = await ApplyAclsRecursivelyAsync(pathClient, memberIds, permissions);
            }
            else
            {
                updateCount = await ApplyAclsToSinglePathAsync(filePath, fileSystemClient, memberIds, permissions);
            }

            _logger.LogInformation(
                "Applied ACLs ({Permissions}) to {Count} items in workspace {Workspace} path {Path}",
                permissions, updateCount, workspaceAcronym, filePath);

            return updateCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying ACLs to workspace {Workspace} path {Path}", workspaceAcronym, filePath);
            throw;
        }
    }

    private async Task<int> ApplyAclsToSinglePathAsync(
        string filePath,
        DataLakeFileSystemClient fileSystemClient,
        List<string> userObjectIds,
        string permissions)
    {
        try
        {
            // Try as directory first
            var pathClient = fileSystemClient.GetDirectoryClient(filePath);
            var accessControl = await pathClient.GetAccessControlAsync();
            var updatedAcl = UpdateAccessControlList(accessControl.Value.AccessControlList, userObjectIds, permissions);
            await pathClient.SetAccessControlListAsync(updatedAcl);
            return 1;
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "PathNotFound" || ex.ErrorCode == "InvalidResourceType")
        {
            // Try as file
            var fileClient = fileSystemClient.GetFileClient(filePath);
            var accessControl = await fileClient.GetAccessControlAsync();
            var updatedAcl = UpdateAccessControlList(accessControl.Value.AccessControlList, userObjectIds, permissions);
            await fileClient.SetAccessControlListAsync(updatedAcl);
            return 1;
        }
    }

    private async Task<int> ApplyAclsRecursivelyAsync(
        DataLakeDirectoryClient directoryClient,
        List<string> userObjectIds,
        string permissions)
    {
        int updateCount = 0;

        try
        {
            // Update current directory
            var accessControl = await directoryClient.GetAccessControlAsync();
            var updatedAcl = UpdateAccessControlList(accessControl.Value.AccessControlList, userObjectIds, permissions);
            await directoryClient.SetAccessControlListAsync(updatedAcl);
            updateCount++;

            // List all items in directory
            await foreach (var pathItem in directoryClient.GetPathsAsync(recursive: false))
            {
                if (pathItem.IsDirectory == true)
                {
                    var subDirClient = directoryClient.GetSubDirectoryClient(pathItem.Name);
                    updateCount += await ApplyAclsRecursivelyAsync(subDirClient, userObjectIds, permissions);
                }
                else
                {
                    var fileClient = directoryClient.GetFileClient(pathItem.Name);
                    var fileAccessControl = await fileClient.GetAccessControlAsync();
                    var fileUpdatedAcl = UpdateAccessControlList(
                        fileAccessControl.Value.AccessControlList, userObjectIds, permissions);
                    await fileClient.SetAccessControlListAsync(fileUpdatedAcl);
                    updateCount++;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying ACLs recursively to directory {Directory}", directoryClient.Path);
            throw;
        }

        return updateCount;
    }

    private List<PathAccessControlItem> UpdateAccessControlList(
        IEnumerable<PathAccessControlItem> existingAcl,
        List<string> userObjectIds,
        string permissions)
    {
        var aclList = existingAcl.ToList();

        foreach (var userId in userObjectIds)
        {
            // Parse the permissions string for this user using PathAccessControlExtensions
            var userAclString = $"user:{userId}:{permissions}";
            var parsedAcl = PathAccessControlExtensions.ParseAccessControlList(userAclString);
            var rolePermissions = parsedAcl.First().Permissions;

            // Check if user already has an ACL entry
            var existingEntry = aclList.FirstOrDefault(a =>
                a.AccessControlType == AccessControlType.User &&
                a.EntityId == userId);

            if (existingEntry != null)
            {
                // Update existing permissions
                existingEntry.Permissions = rolePermissions;
            }
            else
            {
                // Add new ACL entry
                aclList.Add(new PathAccessControlItem(
                    accessControlType: AccessControlType.User,
                    permissions: rolePermissions,
                    entityId: userId));
            }
        }

        return aclList;
    }

    private async Task<DataLakeServiceClient> GetDataLakeServiceClientAsync(string workspaceAcronym)
    {
        var storageAccountName = _storageConfig.GetProjectStorageAccountName(workspaceAcronym);
        var storageAccountKey = await _storageConfig.GetProjectStorageAccountKey(workspaceAcronym);

        // Use connection string approach (same as AzureCloudStorageManager)
        var connectionString = $"DefaultEndpointsProtocol=https;AccountName={storageAccountName};AccountKey={storageAccountKey};EndpointSuffix=core.windows.net";
        
        return new DataLakeServiceClient(connectionString);
    }

    public async Task<int> RemoveAllUserAclsFromPathAsync(
        string workspaceAcronym,
        string filePath,
        bool recursive = false)
    {
        try
        {
            _logger.LogInformation("APPLYING DUAL QUARANTINE (ACL + Metadata) to files in workspace {Workspace} path {Path}", 
                workspaceAcronym, filePath);

            var serviceClient = await GetDataLakeServiceClientAsync(workspaceAcronym);
            var fileSystemClient = serviceClient.GetFileSystemClient(ContainerName);

            // First, let's count how many files exist for debugging
            var totalFiles = await CountFilesAsync(fileSystemClient, filePath);
            _logger.LogInformation("Found {Count} total files in workspace {Workspace} path {Path}", 
                totalFiles, workspaceAcronym, filePath);

            int quarantinedCount = 0;

            // Apply ACL changes to deny all access
            _logger.LogDebug("Processing files recursively from path {Path}", filePath);
            quarantinedCount = await RemoveAllUserAclsFromFilesAsync(fileSystemClient, filePath, recursive);

            _logger.LogInformation(
                "QUARANTINED {Count} files in workspace {Workspace} path {Path} - Applied ACL restrictions AND metadata locks",
                quarantinedCount, workspaceAcronym, filePath);

            return quarantinedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error quarantining files in workspace {Workspace} path {Path}", 
                workspaceAcronym, filePath);
            throw;
        }
    }

    private async Task<int> RemoveAllUserAclsFromFilesAsync(
        DataLakeFileSystemClient fileSystemClient, 
        string basePath, 
        bool recursive)
    {
        int updateCount = 0;
        
        try
        {
            // Normalize path - for root, use empty string for Azure Data Lake
            var searchPath = basePath == "/" ? "" : basePath.TrimStart('/');
            
            _logger.LogDebug("Searching for files in path: '{SearchPath}' (recursive: {Recursive})", searchPath, recursive);

            // Use GetPathsAsync to iterate through all items
            await foreach (var pathItem in fileSystemClient.GetPathsAsync(searchPath, recursive))
            {
                _logger.LogDebug("Processing item: {Name}, IsDirectory: {IsDirectory}", pathItem.Name, pathItem.IsDirectory);

                if (pathItem.IsDirectory == true)
                {
                    // Update directory ACL
                    var dirClient = fileSystemClient.GetDirectoryClient(pathItem.Name);
                    var dirAccessControl = await dirClient.GetAccessControlAsync();
                    var dirUpdatedAcl = RemoveAllUsersFromAccessControlList(dirAccessControl.Value.AccessControlList);
                    await dirClient.SetAccessControlListAsync(dirUpdatedAcl);
                    updateCount++;
                    _logger.LogDebug("Updated ACL for directory: {Name}", pathItem.Name);
                }
                else
                {
                    // Update file ACL AND set quarantine metadata
                    var fileClient = fileSystemClient.GetFileClient(pathItem.Name);
                    var fileAccessControl = await fileClient.GetAccessControlAsync();
                    var fileUpdatedAcl = RemoveAllUsersFromAccessControlList(fileAccessControl.Value.AccessControlList);
                    await fileClient.SetAccessControlListAsync(fileUpdatedAcl);
                    
                    // Set metadata to mark file as quarantined - this WILL work with SharedKey auth
                    await SetQuarantineMetadata(fileClient);
                    
                    updateCount++;
                    _logger.LogDebug("Updated ACL and set quarantine metadata for file: {Name}", pathItem.Name);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing all user ACLs from files in path {Path}", basePath);
            throw;
        }

        return updateCount;
    }

    private async Task SetQuarantineMetadata(DataLakeFileClient fileClient)
    {
        try
        {
            var metadata = new Dictionary<string, string>
            {
                ["quarantine_status"] = "locked",
                ["quarantine_timestamp"] = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                ["quarantine_reason"] = "awaiting_virus_scan",
                ["quarantine_method"] = "dual_acl_metadata",
                ["quarantine_version"] = "2.0"
            };
            
            await fileClient.SetMetadataAsync(metadata);
            _logger.LogDebug("Set quarantine metadata on file");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set quarantine metadata");
            throw;
        }
    }

    private async Task<int> RemoveAllUserAclsFromSinglePathAsync(
        string filePath,
        DataLakeFileSystemClient fileSystemClient)
    {
        try
        {
            // Try as directory first
            var pathClient = fileSystemClient.GetDirectoryClient(filePath);
            var accessControl = await pathClient.GetAccessControlAsync();
            var updatedAcl = RemoveAllUsersFromAccessControlList(accessControl.Value.AccessControlList);
            await pathClient.SetAccessControlListAsync(updatedAcl);
            return 1;
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "PathNotFound" || ex.ErrorCode == "InvalidResourceType")
        {
            // Try as file
            var fileClient = fileSystemClient.GetFileClient(filePath);
            var accessControl = await fileClient.GetAccessControlAsync();
            var updatedAcl = RemoveAllUsersFromAccessControlList(accessControl.Value.AccessControlList);
            await fileClient.SetAccessControlListAsync(updatedAcl);
            return 1;
        }
    }

    private async Task<int> RemoveAllUserAclsRecursivelyAsync(DataLakeDirectoryClient directoryClient)
    {
        int updateCount = 0;

        try
        {
            // Update current directory
            var accessControl = await directoryClient.GetAccessControlAsync();
            var updatedAcl = RemoveAllUsersFromAccessControlList(accessControl.Value.AccessControlList);
            await directoryClient.SetAccessControlListAsync(updatedAcl);
            updateCount++;

            // List all items in directory
            await foreach (var pathItem in directoryClient.GetPathsAsync(recursive: false))
            {
                if (pathItem.IsDirectory == true)
                {
                    var subDirClient = directoryClient.GetSubDirectoryClient(pathItem.Name);
                    updateCount += await RemoveAllUserAclsRecursivelyAsync(subDirClient);
                }
                else
                {
                    var fileClient = directoryClient.GetFileClient(pathItem.Name);
                    var fileAccessControl = await fileClient.GetAccessControlAsync();
                    var fileUpdatedAcl = RemoveAllUsersFromAccessControlList(fileAccessControl.Value.AccessControlList);
                    await fileClient.SetAccessControlListAsync(fileUpdatedAcl);
                    updateCount++;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing all user ACLs recursively from directory {Directory}", 
                directoryClient.Path);
            throw;
        }

        return updateCount;
    }

    private List<PathAccessControlItem> RemoveAllUsersFromAccessControlList(
        IEnumerable<PathAccessControlItem> existingAcl)
    {
        var aclList = existingAcl.ToList();

        // Log current ACL for debugging
        _logger.LogDebug("Current ACL entries: {Entries}", string.Join(", ", 
            aclList.Select(a => $"{a.AccessControlType}:{a.EntityId ?? "owner"}:{a.Permissions}")));

        // DUAL APPROACH STRATEGY:
        // 1. ACL RESTRICTIONS: Work with Azure AD authentication (service principals, managed identities)
        //    - Set all permissions to "---" for comprehensive access denial
        //    - Effective when app uses token-based auth instead of SharedKey
        // 
        // 2. METADATA LOCKS: Work with ALL authentication methods (including SharedKey)
        //    - Set quarantine metadata flags on files
        //    - Application-level checks in download logic prevent access
        //    - Reliable fallback when ACLs are bypassed
        //
        // This dual approach ensures file quarantine works regardless of auth method!

        // Set ALL permissions to "---" (no access) - including owner, users, groups, and other
        // This provides ACL-level protection for Azure AD authenticated access
        for (int i = 0; i < aclList.Count; i++)
        {
            var aclItem = aclList[i];
            PathAccessControlItem noAccessItem;
            
            switch (aclItem.AccessControlType)
            {
                case AccessControlType.User:
                    if (string.IsNullOrEmpty(aclItem.EntityId))
                    {
                        // This is the owning user (EntityId is null for owner)
                        var ownerNoAccessItems = PathAccessControlExtensions.ParseAccessControlList("user::---");
                        noAccessItem = ownerNoAccessItems.First();
                        _logger.LogDebug("Set OWNER permissions to: --- (no access)");
                    }
                    else
                    {
                        // This is a specific user
                        var userNoAccessItems = PathAccessControlExtensions.ParseAccessControlList($"user:{aclItem.EntityId}:---");
                        noAccessItem = userNoAccessItems.First();
                        _logger.LogDebug("Set user {UserId} permissions to: --- (no access)", aclItem.EntityId);
                    }
                    break;
                    
                case AccessControlType.Group:
                    if (string.IsNullOrEmpty(aclItem.EntityId))
                    {
                        // This is the owning group
                        var groupNoAccessItems = PathAccessControlExtensions.ParseAccessControlList("group::---");
                        noAccessItem = groupNoAccessItems.First();
                        _logger.LogDebug("Set OWNING GROUP permissions to: --- (no access)");
                    }
                    else
                    {
                        // This is a specific group
                        var groupNoAccessItems = PathAccessControlExtensions.ParseAccessControlList($"group:{aclItem.EntityId}:---");
                        noAccessItem = groupNoAccessItems.First();
                        _logger.LogDebug("Set group {GroupId} permissions to: --- (no access)", aclItem.EntityId);
                    }
                    break;
                    
                case AccessControlType.Other:
                    var otherNoAccessItems = PathAccessControlExtensions.ParseAccessControlList("other::---");
                    noAccessItem = otherNoAccessItems.First();
                    _logger.LogDebug("Set 'other' permissions to: --- (no access)");
                    break;
                    
                default:
                    // Keep the original item if we don't know how to handle it
                    noAccessItem = aclItem;
                    _logger.LogDebug("Unknown ACL type {Type}, keeping original permissions", aclItem.AccessControlType);
                    break;
            }
            
            aclList[i] = noAccessItem;
        }

        _logger.LogDebug("Updated ACL entries: {Entries}", string.Join(", ", 
            aclList.Select(a => $"{a.AccessControlType}:{a.EntityId ?? "owner"}:{a.Permissions}")));

        return aclList;
    }

    public async Task<int> SimulateScanSuccessAsync(string workspaceAcronym, string filePath)
    {
        try
        {
            _logger.LogInformation("RESTORING ACCESS (ACL + Metadata) to files in workspace {Workspace} path {Path}", 
                workspaceAcronym, filePath);

            var serviceClient = await GetDataLakeServiceClientAsync(workspaceAcronym);
            var fileSystemClient = serviceClient.GetFileSystemClient(ContainerName);

            // First, let's count how many files exist for debugging
            var totalFiles = await CountFilesAsync(fileSystemClient, filePath);
            _logger.LogInformation("Found {Count} total files in workspace {Workspace} path {Path}", 
                totalFiles, workspaceAcronym, filePath);

            int restoredCount = 0;

            // Apply ACL changes to restore read/execute access and remove quarantine metadata
            _logger.LogDebug("Processing files recursively from path {Path}", filePath);
            restoredCount = await RestoreAccessToFilesAsync(fileSystemClient, filePath, recursive: true);

            _logger.LogInformation(
                "RESTORED ACCESS to {Count} files in workspace {Workspace} path {Path} - Applied ACL permissions AND removed metadata locks",
                restoredCount, workspaceAcronym, filePath);

            return restoredCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring access to files in workspace {Workspace} path {Path}", 
                workspaceAcronym, filePath);
            throw;
        }
    }

    private async Task<int> RestoreAccessToFilesAsync(
        DataLakeFileSystemClient fileSystemClient, 
        string basePath, 
        bool recursive)
    {
        int updateCount = 0;
        
        try
        {
            // Normalize path - for root, use empty string for Azure Data Lake
            var searchPath = basePath == "/" ? "" : basePath.TrimStart('/');
            
            _logger.LogDebug("Searching for files in path: '{SearchPath}' (recursive: {Recursive})", searchPath, recursive);

            // Use GetPathsAsync to iterate through all items
            await foreach (var pathItem in fileSystemClient.GetPathsAsync(searchPath, recursive))
            {
                _logger.LogDebug("Processing item: {Name}, IsDirectory: {IsDirectory}", pathItem.Name, pathItem.IsDirectory);

                if (pathItem.IsDirectory == true)
                {
                    // Update directory ACL to restore access
                    var dirClient = fileSystemClient.GetDirectoryClient(pathItem.Name);
                    var dirAccessControl = await dirClient.GetAccessControlAsync();
                    var dirUpdatedAcl = RestoreAccessToAccessControlList(dirAccessControl.Value.AccessControlList);
                    await dirClient.SetAccessControlListAsync(dirUpdatedAcl);
                    updateCount++;
                    _logger.LogDebug("Restored ACL for directory: {Name}", pathItem.Name);
                }
                else
                {
                    // For files: Restore access by setting fresh permissions
                    var fileClient = fileSystemClient.GetFileClient(pathItem.Name);
                    
                    _logger.LogDebug("Restoring access for file: {Name}", pathItem.Name);
                    
                    // Set scan success metadata first (before modifying ACLs)
                    await SetScanSuccessMetadata(fileClient);
                    
                    // Then restore ACL permissions with a fresh ACL
                    var restoredAcl = PathAccessControlExtensions.ParseAccessControlList("user::rwx,group::r-x,other::r-x");
                    await fileClient.SetAccessControlListAsync(restoredAcl);
                    
                    updateCount++;
                    _logger.LogDebug("Restored ACL and set scan success metadata for file: {Name}", pathItem.Name);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring access to files in path {Path}", basePath);
            throw;
        }

        return updateCount;
    }

    private List<PathAccessControlItem> RestoreAccessToAccessControlList(
        IEnumerable<PathAccessControlItem> existingAcl)
    {
        var aclList = existingAcl.ToList();

        // Log current ACL for debugging
        _logger.LogDebug("Current ACL entries: {Entries}", string.Join(", ", 
            aclList.Select(a => $"{a.AccessControlType}:{a.EntityId ?? "owner"}:{a.Permissions}")));

        // Restore read/execute permissions for all entries
        for (int i = 0; i < aclList.Count; i++)
        {
            var aclItem = aclList[i];
            PathAccessControlItem restoredItem;
            
            switch (aclItem.AccessControlType)
            {
                case AccessControlType.User:
                    if (string.IsNullOrEmpty(aclItem.EntityId))
                    {
                        // This is the owning user - restore full access
                        var ownerItems = PathAccessControlExtensions.ParseAccessControlList("user::rwx");
                        restoredItem = ownerItems.First();
                        _logger.LogDebug("Restored OWNER permissions to: rwx");
                    }
                    else
                    {
                        // This is a specific user - restore read/execute
                        var userItems = PathAccessControlExtensions.ParseAccessControlList($"user:{aclItem.EntityId}:r-x");
                        restoredItem = userItems.First();
                        _logger.LogDebug("Restored user {UserId} permissions to: r-x", aclItem.EntityId);
                    }
                    break;
                    
                case AccessControlType.Group:
                    if (string.IsNullOrEmpty(aclItem.EntityId))
                    {
                        // This is the owning group - restore read/execute
                        var groupItems = PathAccessControlExtensions.ParseAccessControlList("group::r-x");
                        restoredItem = groupItems.First();
                        _logger.LogDebug("Restored OWNING GROUP permissions to: r-x");
                    }
                    else
                    {
                        // This is a specific group - restore read/execute
                        var groupItems = PathAccessControlExtensions.ParseAccessControlList($"group:{aclItem.EntityId}:r-x");
                        restoredItem = groupItems.First();
                        _logger.LogDebug("Restored group {GroupId} permissions to: r-x", aclItem.EntityId);
                    }
                    break;
                    
                case AccessControlType.Other:
                    // Restore read/execute for 'other'
                    var otherItems = PathAccessControlExtensions.ParseAccessControlList("other::r-x");
                    restoredItem = otherItems.First();
                    _logger.LogDebug("Restored 'other' permissions to: r-x");
                    break;
                    
                default:
                    // Keep the original item if we don't know how to handle it
                    restoredItem = aclItem;
                    _logger.LogDebug("Unknown ACL type {Type}, keeping original permissions", aclItem.AccessControlType);
                    break;
            }
            
            aclList[i] = restoredItem;
        }

        _logger.LogDebug("Updated ACL entries: {Entries}", string.Join(", ", 
            aclList.Select(a => $"{a.AccessControlType}:{a.EntityId ?? "owner"}:{a.Permissions}")));

        return aclList;
    }

    private async Task SetScanSuccessMetadata(DataLakeFileClient fileClient)
    {
        try
        {
            // Create fresh metadata dictionary with only scan success fields (same approach as lock function)
            // Use underscore format instead of colon format to match quarantine metadata
            var metadata = new Dictionary<string, string>
            {
                ["scan_status"] = "Clean",
                ["scan_date"] = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                ["scan_engine"] = "ClamAV-Simulated"
            };
            
            await fileClient.SetMetadataAsync(metadata);
            _logger.LogDebug("Set scan success metadata on file");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set scan success metadata");
            throw;
        }
    }

    private async Task<int> CountFilesAsync(DataLakeFileSystemClient fileSystemClient, string path)
    {
        try
        {
            // Normalize path - for root, use empty string for Azure Data Lake
            var searchPath = path == "/" ? "" : path.TrimStart('/');
            
            int fileCount = 0;
            
            await foreach (var pathItem in fileSystemClient.GetPathsAsync(searchPath, recursive: true))
            {
                if (pathItem.IsDirectory != true) // Count files only, not directories
                {
                    fileCount++;
                    _logger.LogDebug("Found file: {Name}", pathItem.Name);
                }
            }
            
            return fileCount;
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "PathNotFound" || ex.ErrorCode == "BlobNotFound")
        {
            _logger.LogDebug("Path {Path} not found when counting files", path);
            return 0;
        }
    }

    private async Task<int> CountFilesRecursivelyAsync(DataLakeDirectoryClient directoryClient)
    {
        int fileCount = 0;

        try
        {
            _logger.LogDebug("Counting files in directory: {Directory}", directoryClient.Path);
            
            await foreach (var pathItem in directoryClient.GetPathsAsync(recursive: false))
            {
                _logger.LogDebug("Found path item: {Name}, IsDirectory: {IsDirectory}", 
                    pathItem.Name, pathItem.IsDirectory);
                    
                if (pathItem.IsDirectory == true)
                {
                    var subDirClient = directoryClient.GetSubDirectoryClient(pathItem.Name);
                    fileCount += await CountFilesRecursivelyAsync(subDirClient);
                }
                else
                {
                    fileCount++;
                    _logger.LogDebug("Found file: {File}", pathItem.Name);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting files in directory {Directory}", directoryClient.Path);
            throw;
        }

        _logger.LogDebug("Total files counted in directory {Directory}: {Count}", 
            directoryClient.Path, fileCount);
        return fileCount;
    }
}
