using Datahub.Application.Exceptions;
using Datahub.Application.Services;
using Datahub.Application.Services.Publishing;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services;
using Datahub.Core.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Datahub.Infrastructure.Services.Publishing
{
    public class OpenDataPublishingService(
            IUserInformationService userService,
            IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
            IMemoryCache memoryCache,
            IProjectUserManagementService projectUserManagementService) : IOpenDataPublishingService
    {
        public event Func<OpenDataPublishFile, Task>? FileUploadStatusUpdated;
        
        private const string WorkspaceBlocklistCacheKeyPrefix = "workspace_blocklist_";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

        public async Task<List<OpenDataSubmission>> GetAvailableOpenDataSubmissionsForWorkspaceAsync(int workspaceId)
        {
            await using var ctx = await dbContextFactory.CreateDbContextAsync();

            var submissions = await ctx.OpenDataSubmissions
                .AsNoTracking()
                .Where(s => s.ProjectId == workspaceId && s.OpenForAttachingFiles)
                .ToListAsync();

            return await Task.FromResult(submissions);
        }

        public async Task<OpenDataSubmission> GetOpenDataSubmissionAsync(long submissionId)
        {
            await using var ctx = await dbContextFactory.CreateDbContextAsync();

            var submission = await ctx.OpenDataSubmissions
                .AsNoTracking()
                .Include(s => s.Files)
                .Include(s => s.Project)
                .Include(s => s.RequestingUser)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
            {
                throw new OpenDataPublishingException($"Submission {submissionId} not found", new FileNotFoundException());
            }

            return await Task.FromResult(submission);
        }

        public async Task<List<OpenDataSubmission>> GetOpenDataSubmissionsAsync(int workspaceId)
        {
            await using var ctx = await dbContextFactory.CreateDbContextAsync();

            var submissions = await ctx.OpenDataSubmissions
                .AsNoTracking()
                .Include(s => s.RequestingUser)
                .Where(s => s.ProjectId == workspaceId)
                .ToListAsync();

            return await Task.FromResult(submissions);
        }

        public async Task<TbsOpenGovSubmission> UpdateTbsOpenGovSubmission(TbsOpenGovSubmission submission)
        {
            await using var ctx = await dbContextFactory.CreateDbContextAsync();

            var existingSubmission = await ctx.TbsOpenGovSubmissions
                .Include(s => s.Files)
                .FirstOrDefaultAsync(s => s.Id == submission.Id);

            if (existingSubmission == null)
            {
                // not found
                throw new OpenDataPublishingException($"Submission {submission.Id} not found", new FileNotFoundException());
            }

            existingSubmission.MetadataComplete = submission.MetadataComplete;
            existingSubmission.OpenGovCriteriaFormId = submission.OpenGovCriteriaFormId;
            existingSubmission.OpenGovCriteriaMetDate = submission.OpenGovCriteriaMetDate;
            existingSubmission.LocalDQCheckStarted = submission.LocalDQCheckStarted;
            existingSubmission.LocalDQCheckPassed = submission.LocalDQCheckPassed;
            existingSubmission.InitialOpenGovSubmissionDate = submission.InitialOpenGovSubmissionDate;
            existingSubmission.OpenGovDQCheckPassed = submission.OpenGovDQCheckPassed;
            existingSubmission.ImsoApprovalRequestDate = submission.ImsoApprovalRequestDate;
            existingSubmission.ImsoApprovedDate = submission.ImsoApprovedDate;
            existingSubmission.OpenGovPublicationDate = submission.OpenGovPublicationDate;

            foreach (var file in submission.Files)
            {
                var existingFile = existingSubmission.Files.FirstOrDefault(f => f.Id == file.Id);
                if (existingFile != null)
                {
                    DoUpdateOpenDataPublishFile(existingFile, file);
                }
                //TODO add/delete files
            }

            submission.Status = TbsOpenGovPublishingUtils.GetCurrentStatus(submission).ToString();

            UpdateGenericSubmissionData(existingSubmission, submission);

            await ctx.SaveChangesAsync();

            return submission;
        }

        private static void DoUpdateOpenDataPublishFile(OpenDataPublishFile existing, OpenDataPublishFile updated)
        {
            existing.FilePurpose = updated.FilePurpose;
            existing.UploadStatus = updated.UploadStatus;
            existing.UploadMessage = updated.UploadMessage;
        }

        private static void UpdateGenericSubmissionData(OpenDataSubmission existing, OpenDataSubmission updated)
        {
            existing.DatasetTitle = updated.DatasetTitle;
            existing.Status = updated.Status;
            existing.OpenForAttachingFiles = updated.OpenForAttachingFiles;
        }

        public async Task<OpenDataSubmission> CreateOpenDataSubmission(OpenDataSubmissionBasicInfo openDataSubmissionBasicInfo)
        {
            await using var ctx = await dbContextFactory.CreateDbContextAsync();

            OpenDataSubmission? submission = openDataSubmissionBasicInfo.ProcessType switch
            {
                OpenDataPublishProcessType.TbsOpenGovPublishing => await DoCreateTbsOpenGovSubmission(openDataSubmissionBasicInfo, ctx),
                _ => null
            };

            if (submission == null)
            {
                throw new OpenDataPublishingException("Cannot create an open data submission without specifying a process type");
            }

            var user = await userService.GetCurrentPortalUserAsync();
            submission.RequestingUserId = user.Id;
            submission.RequestDate = DateTime.Today;

            await ctx.SaveChangesAsync();

            return await Task.FromResult(submission);
        }

        private async Task<TbsOpenGovSubmission> DoCreateTbsOpenGovSubmission(OpenDataSubmissionBasicInfo openDataSubmissionBasicInfo, DatahubProjectDBContext ctx)
        {
            var submission = new TbsOpenGovSubmission()
            {
                UniqueId = Guid.NewGuid().ToString(),
                ProjectId = openDataSubmissionBasicInfo.ProjectId,
                DatasetTitle = openDataSubmissionBasicInfo.DatasetTitle,
                ProcessType = openDataSubmissionBasicInfo.ProcessType,
                OpenForAttachingFiles = true
            };

            await ctx.TbsOpenGovSubmissions.AddAsync(submission);

            submission.Status = TbsOpenGovPublishingUtils.GetCurrentStatus(submission).ToString();

            return await Task.FromResult(submission);
        }

        public async Task AddFilesToSubmission(OpenDataSubmission openDataSubmission, IEnumerable<FileMetaData> files, int? containerId, string containerName)
        {
            await using var ctx = await dbContextFactory.CreateDbContextAsync();

            var submission = await ctx.OpenDataSubmissions
                .Include(s => s.Files)
                .FirstOrDefaultAsync(s => s.Id == openDataSubmission.Id);

            if (submission == null)
            {
                throw new OpenDataPublishingException($"Submission not found: {openDataSubmission.Id}");
            }

            var additionalFiles = files.Select(f => new OpenDataPublishFile()
            {
                FileId = f.fileid,
                FileName = f.filename,
                FolderPath = f.folderpath,
                ProjectStorageId = containerId,
                ContainerName = containerName,
                Submission = submission
            });

            submission.Files ??= new List<OpenDataPublishFile>();
            
            foreach (var file in additionalFiles)
            {
                submission.Files.Add(file);
            }

            await ctx.SaveChangesAsync();
        }

        public async Task<OpenDataPublishFile> UpdateFileUploadStatus(OpenDataPublishFile file, OpenDataPublishFileUploadStatus status, string? uploadMessage = null)
        {
            await using var ctx = await dbContextFactory.CreateDbContextAsync();
            var existingFile = await ctx.OpenDataPublishFiles.FirstOrDefaultAsync(f => f.Id == file.Id);
            if (existingFile == null)
            {
                throw new OpenDataPublishingException($"Could not find file with ID {file.Id} (filename: {file.FileName}, submission ID #{file.SubmissionId})");
            }

            existingFile.UploadStatus = status;
            existingFile.UploadMessage = uploadMessage;

            await ctx.SaveChangesAsync();

            FileUploadStatusUpdated?.Invoke(existingFile);

            return existingFile;
        }

        // Publishing Blocklist methods
        public async Task<List<OpenGovPublishingBlocklist>> GetActiveBlocklistEntriesAsync()
        {
            await using var ctx = await dbContextFactory.CreateDbContextAsync();
            
            return await ctx.OpenGovPublishingBlocklist
                .Include(b => b.AddedByUser)
                .Where(b => b.Status == BlocklistStatus.Active)
                .OrderByDescending(b => b.DateAdded)
                .ToListAsync();
        }

        public async Task<OpenGovPublishingBlocklist> GetBlocklistEntryAsync(int id)
        {
            await using var ctx = await dbContextFactory.CreateDbContextAsync();
            
            var entry = await ctx.OpenGovPublishingBlocklist
                .Include(b => b.AddedByUser)
                .Include(b => b.RemovedByUser)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (entry == null)
                throw new InvalidOperationException($"Blocklist entry with ID {id} not found");

            return entry;
        }

        public async Task<bool> IsUserBlockedAsync(string emailDomain, string? departmentName = null)
        {
            if (string.IsNullOrWhiteSpace(emailDomain))
                return false;

            await using var ctx = await dbContextFactory.CreateDbContextAsync();

            // Check if the email domain is blocked (department is informational only, not used for matching)
            return await ctx.OpenGovPublishingBlocklist
                .Where(b => b.Status == BlocklistStatus.Active && b.EmailHostname == emailDomain)
                .AnyAsync();
        }

        public async Task<bool> IsPublishingBlockedForWorkspaceAsync(string workspaceAcronym)
        {
            if (string.IsNullOrWhiteSpace(workspaceAcronym))
                return false;

            // Check cache first
            var cacheKey = $"{WorkspaceBlocklistCacheKeyPrefix}{workspaceAcronym.ToLowerInvariant()}";
            
            if (memoryCache.TryGetValue<bool>(cacheKey, out var cachedResult))
            {
                return cachedResult;
            }

            // Not in cache, check the database
            try
            {
                // Get the workspace lead
                var workspaceLead = await projectUserManagementService.GetProjectLeadAsync(workspaceAcronym);
                
                if (workspaceLead?.PortalUser?.Email != null)
                {
                    // Extract the email domain (everything after @)
                    var email = workspaceLead.PortalUser.Email;
                    var atIndex = email.IndexOf('@');
                    
                    if (atIndex > 0 && atIndex < email.Length - 1)
                    {
                        var emailDomain = email.Substring(atIndex);
                        
                        // Check if this domain is blocked
                        var isBlocked = await IsUserBlockedAsync(emailDomain);
                        
                        // Cache the result
                        memoryCache.Set(cacheKey, isBlocked, CacheDuration);
                        
                        return isBlocked;
                    }
                }

                // No valid email found, cache false
                memoryCache.Set(cacheKey, false, CacheDuration);
                return false;
            }
            catch
            {
                // In case of error, default to not blocking (don't cache errors)
                return false;
            }
        }

        public async Task<OpenGovPublishingBlocklist> AddBlocklistEntryAsync(string departmentName, string emailHostname, string notes)
        {
            await using var ctx = await dbContextFactory.CreateDbContextAsync();
            
            var currentUser = await userService.GetCurrentPortalUserAsync();
            if (currentUser == null)
                throw new InvalidOperationException("Current user not found");

            var now = DateTime.UtcNow;

            var entry = new OpenGovPublishingBlocklist
            {
                DepartmentName = string.IsNullOrWhiteSpace(departmentName) ? string.Empty : departmentName.Trim(),
                EmailHostname = string.IsNullOrWhiteSpace(emailHostname) ? string.Empty : emailHostname.Trim().ToLowerInvariant(),
                Status = BlocklistStatus.Active,
                DateAdded = now,
                AddedByUserId = currentUser.Id,
                Notes = notes ?? string.Empty
            };

            ctx.OpenGovPublishingBlocklist.Add(entry);
            await ctx.SaveChangesAsync();

            return entry;
        }

        public async Task<OpenGovPublishingBlocklist> UpdateBlocklistEntryAsync(int id, string departmentName, string emailHostname, string notes)
        {
            await using var ctx = await dbContextFactory.CreateDbContextAsync();
            
            var entry = await ctx.OpenGovPublishingBlocklist.FindAsync(id);
            if (entry == null)
                throw new InvalidOperationException($"Blocklist entry with ID {id} not found");

            entry.DepartmentName = string.IsNullOrWhiteSpace(departmentName) ? null : departmentName.Trim();
            entry.EmailHostname = string.IsNullOrWhiteSpace(emailHostname) ? null : emailHostname.Trim().ToLowerInvariant();
            entry.Notes = notes;

            await ctx.SaveChangesAsync();

            return entry;
        }

        public async Task DeleteBlocklistEntryAsync(int id)
        {
            await using var ctx = await dbContextFactory.CreateDbContextAsync();
            
            var entry = await ctx.OpenGovPublishingBlocklist.FindAsync(id);
            if (entry == null)
                throw new InvalidOperationException($"Blocklist entry with ID {id} not found");

            var currentUser = await userService.GetCurrentPortalUserAsync();
            if (currentUser == null)
                throw new InvalidOperationException("Current user not found");

            entry.Status = BlocklistStatus.Deleted;
            entry.DateRemoved = DateTime.UtcNow;
            entry.RemovedByUserId = currentUser.Id;

            await ctx.SaveChangesAsync();
        }
    }
}
