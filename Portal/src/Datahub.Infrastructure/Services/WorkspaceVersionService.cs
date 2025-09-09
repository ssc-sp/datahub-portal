using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace Datahub.Infrastructure.Services
{
    public class WorkspaceVersionService(
        IDbContextFactory<DatahubProjectDBContext> datahubProjectDbFactory,
        ILogger<WorkspaceVersionService> logger) : IWorkspaceVersionService
    {
        public async Task<string> GetLatestVersionAsync()
        {
            try
            {
                await using var db = await datahubProjectDbFactory.CreateDbContextAsync();
                var versionTags = await db.VersionTags
                                    .Where(t => t.IsActive)
                                    .Select(t => t.Tag)
                                    .ToListAsync();

                var latest = versionTags
                     .Select(v => Version.Parse(v.TrimStart('v')))
                     .OrderByDescending(v => v)
                     .First();
                var latestStr = $"v{latest.ToString()}";

                return latestStr;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting latest version");
                throw;
            }
        }

        public async Task<List<VersionTag>> GetAllVersionsAsync()
        {
            try
            {
                await using var db = await datahubProjectDbFactory.CreateDbContextAsync();
                var versionTags = await db.VersionTags
                    .ToListAsync();

                var orderedVersionTags = versionTags
                    .OrderByDescending(v => Version.Parse(v.Tag.TrimStart('v')))
                    .ToList();

                return orderedVersionTags;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting all versions");
                throw;
            }
        }

        public async Task<VersionTag?> GetVersionByIdAsync(int versionTagId)
        {
            try
            {
                await using var db = await datahubProjectDbFactory.CreateDbContextAsync();
                return await db.VersionTags
                    .FirstOrDefaultAsync(v => v.VersionTagId == versionTagId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting version by ID: {VersionTagId}", versionTagId);
                throw;
            }
        }

        public async Task<bool> AddNewVersion(VersionTag versionTag)
        {
            try
            {
                await using var db = await datahubProjectDbFactory.CreateDbContextAsync();
                await db.VersionTags.AddAsync(versionTag);
                var isSaved = await db.SaveChangesAsync();
                return isSaved > 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding new version with tag: {Tag}", versionTag?.Tag);
                throw;
            }
        }

        public async Task<bool> UpdateVersionTag(VersionTag versionTag)
        {
            try
            {
                await using var db = await datahubProjectDbFactory.CreateDbContextAsync();
                db.VersionTags.Update(versionTag);
                var isSaved = await db.SaveChangesAsync();
                return isSaved > 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating version tag with ID: {VersionTagId}", versionTag?.VersionTagId);
                throw;
            }
        }

        public async Task<bool> DeleteVersion(VersionTag versionTag)
        {
            try
            {
                await using var db = await datahubProjectDbFactory.CreateDbContextAsync();
                db.VersionTags.Remove(versionTag);
                var isDeleted = await db.SaveChangesAsync();
                return isDeleted > 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting version tag with ID: {VersionTagId}", versionTag?.VersionTagId);
                throw;
            }
        }

        public async Task<bool> SetResourcesToCreateRequested(int projectId)
        {
            try
            {
                await using var db = await datahubProjectDbFactory.CreateDbContextAsync();
                var projectResources = await db.Project_Resources2
                    .Where(r => r.ProjectId == projectId)
                    .ToListAsync();

                projectResources.ForEach(resource => resource.Status = TerraformStatus.CreateRequested);

                return await db.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error setting resources to create requested for project ID: {ProjectId}", projectId);
                throw;
            }
        }

        public async Task<bool> SetWorkspaceToUpdateRequested(int projectId)
        {
            try
            {
                await using var db = await datahubProjectDbFactory.CreateDbContextAsync();
                var project = await db.Projects.FirstOrDefaultAsync(p => p.Project_ID == projectId);

                if (project == null)
                {
                    logger.LogError("Project with ID {ProjectId} not found.", projectId);
                    return false;
                }

                project.IsVersionUpdateRequested = true;
                return await db.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error setting workspace to update requested for project ID: {ProjectId}", projectId);
                throw;
            }
        }

        public async Task<List<OutdatedWorkspaceInfo>> GetWorkspacesNotOnLatestVersionAsync()
        {
            try
            {
                await using var db = await datahubProjectDbFactory.CreateDbContextAsync();
                
                // Get the latest version
                var latestVersion = await GetLatestVersionAsync();
                
                // Get all projects that are not deleted and have a version different from the latest
                var outdatedWorkspaces = await db.Projects
                    .Where(p => p.Deleted_DT == null && 
                               p.Version != null && 
                               p.Version != latestVersion &&
                               p.Version != "latest")
                    .Select(p => new OutdatedWorkspaceInfo
                    {
                        WorkspaceName = p.Project_Name,
                        WorkspaceAcronym = p.Project_Acronym_CD,
                        CurrentVersion = p.Version
                    })
                    .OrderBy(w => w.WorkspaceAcronym)
                    .ToListAsync();

                return outdatedWorkspaces;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting workspaces not on latest version");
                throw;
            }
        }

        public async Task<WorkspaceVersionStatistics> GetWorkspaceVersionStatisticsAsync()
        {
            try
            {
                await using var db = await datahubProjectDbFactory.CreateDbContextAsync();
                
                // Get the latest version
                var latestVersion = await GetLatestVersionAsync();
                
                // Get all active workspaces (not deleted)
                var allWorkspaces = await db.Projects
                    .Where(p => p.Deleted_DT == null)
                    .ToListAsync();

                var totalWorkspaces = allWorkspaces.Count;
                
                // Count workspaces on latest version (including those with "latest" version)
                var latestVersionWorkspaces = allWorkspaces.Count(p => 
                    p.Version == latestVersion || p.Version == "latest");
                
                var outdatedWorkspaces = totalWorkspaces - latestVersionWorkspaces;
                
                // Count workspaces with pending update requests
                var workspacesWithUpdateRequests = allWorkspaces.Count(p => p.IsVersionUpdateRequested);
                
                var percentageOnLatest = totalWorkspaces > 0 
                    ? Math.Round((decimal)latestVersionWorkspaces / totalWorkspaces * 100, 1)
                    : 0;

                return new WorkspaceVersionStatistics
                {
                    TotalWorkspaces = totalWorkspaces,
                    LatestVersionWorkspaces = latestVersionWorkspaces,
                    OutdatedWorkspaces = outdatedWorkspaces,
                    PercentageOnLatest = percentageOnLatest,
                    WorkspacesWithUpdateRequests = workspacesWithUpdateRequests,
                    LatestVersion = latestVersion
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting workspace version statistics");
                throw;
            }
        }
    }
}
