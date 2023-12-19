using Datahub.Application.Services.Publishing;
using Datahub.Core.Data;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services;
using Datahub.Core.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Infrastructure.Services.Publishing
{
    public class OpenDataPublishingService(IUserInformationService userService, IDbContextFactory<DatahubProjectDBContext> dbContextFactory) : IOpenDataPublishingService
    {
        private readonly IUserInformationService _userService = userService;
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory = dbContextFactory;

        public async Task<List<OpenDataSubmission>> GetAvailableOpenDataSubmissionsForWorkspaceAsync(int workspaceId)
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();

            var submissions = await ctx.OpenDataSubmissions
                .Where(s => s.ProjectId == workspaceId && s.OpenForAttachingFiles)
                .ToListAsync();

            return await Task.FromResult(submissions);
        }

        public async Task<OpenDataSubmission> GetOpenDataSubmissionAsync(long submissionId)
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();

            var submission = await ctx.OpenDataSubmissions
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
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();

            var submissions = await ctx.OpenDataSubmissions
                .Include(s => s.RequestingUser)
                .Where(s => s.ProjectId == workspaceId)
                .ToListAsync();

            return await Task.FromResult(submissions);
        }

        public async Task<TbsOpenGovSubmission> UpdateTbsOpenGovSubmission(TbsOpenGovSubmission submission)
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();

            var existingSubmission = await ctx.TbsOpenGovSubmissions.FirstOrDefaultAsync(s => s.Id == submission.Id);

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
            
            submission.Status = TbsOpenGovPublishingUtils.GetCurrentStatus(submission).ToString();

            UpdateGenericSubmissionData(existingSubmission, submission);

            await ctx.SaveChangesAsync();

            return submission;
        }

        private static void UpdateGenericSubmissionData(OpenDataSubmission existing, OpenDataSubmission updated)
        {
            existing.DatasetTitle = updated.DatasetTitle;
            existing.Status = updated.Status;
            existing.OpenForAttachingFiles = updated.OpenForAttachingFiles;
        }

        public async Task<OpenDataSubmission> CreateOpenDataSubmission(OpenDataSubmissionBasicInfo openDataSubmissionBasicInfo)
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();

            OpenDataSubmission? submission = openDataSubmissionBasicInfo.ProcessType switch
            {
                OpenDataPublishProcessType.TbsOpenGovPublishing => await DoCreateTbsOpenGovSubmission(openDataSubmissionBasicInfo, ctx),
                _ => null
            };

            if (submission == null)
            {
                throw new OpenDataPublishingException("Cannot create an open data submission without specifying a process type");
            }

            var user = await _userService.GetCurrentPortalUserAsync();
            submission.RequestingUser = user;
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

        public async Task<int> AddFilesToSubmission(OpenDataSubmission openDataSubmission, IEnumerable<FileMetaData> files, int? containerId)
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();

            var submission = await ctx.OpenDataSubmissions.FirstOrDefaultAsync(s => s.Id == openDataSubmission.Id);

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
                Submission = submission
            });

            submission.Files ??= new List<OpenDataPublishFile>();
            var addedFiles = 0;
            
            foreach (var file in additionalFiles)
            {
                submission.Files.Add(file);
                addedFiles++;
            }

            await ctx.SaveChangesAsync();

            return await Task.FromResult(addedFiles);
        }
    }
}
