using Datahub.Application.Services.Publishing;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services;
using Datahub.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Infrastructure.Services.Publishing
{
    public class OpenDataPublishingService : IOpenDataPublishingService
    {
        private readonly IUserInformationService _userService;

        public OpenDataPublishingService(IUserInformationService userService)
        {
            _userService = userService;
        }

        public async Task<List<OpenDataSubmission>> GetAvailableOpenDataSubmissionsForWorkspaceAsync(int workspaceId)
        {
            // TODO actial implementation
            var result = new List<OpenDataSubmission>
            {
                await CreateTestSubmission(1)
            };

            return await Task.FromResult(result);
        }

        public async Task<OpenDataSubmission> GetOpenDataSubmissionAsync(long submissionId)
        {
            //TODO actual implementation
            return await CreateTestSubmission(submissionId);
        }

        public async Task<List<OpenDataSubmission>> GetOpenDataSubmissionsAsync(int workspaceId)
        {
            //TODO actual implementation

            var result = new List<OpenDataSubmission>
            {
                await CreateTestSubmission(1)
            };

            return await Task.FromResult(result);
        }

        public async Task<TbsOpenGovSubmission> UpdateTbsOpenGovSubmission(TbsOpenGovSubmission submission)
        {
            //TODO load from db
            var existingSubmission = await CreateTestSubmission(submission.Id) as TbsOpenGovSubmission;

            if (existingSubmission == null)
            {
                // not found
                throw new OpenDataPublishingException();
            }

            existingSubmission.OpenGovCriteriaFormId = submission.OpenGovCriteriaFormId;
            existingSubmission.OpenGovCriteriaMetDate = submission.OpenGovCriteriaMetDate;
            existingSubmission.LocalDQCheckStarted = submission.LocalDQCheckStarted;
            existingSubmission.LocalDQCheckPassed = submission.LocalDQCheckPassed;
            existingSubmission.InitialOpenGovSubmissionDate = submission.InitialOpenGovSubmissionDate;
            existingSubmission.OpenGovDQCheckPassed = submission.OpenGovDQCheckPassed;
            existingSubmission.ImsoApproved = submission.ImsoApproved;
            existingSubmission.OpenGovPublicationDate = submission.OpenGovPublicationDate;

            submission.Status = TbsOpenGovPublishingUtils.GetCurrentStatus(submission).ToString();

            UpdateGenericSubmissionData(existingSubmission, submission);

            return submission;
        }

        private void UpdateGenericSubmissionData(OpenDataSubmission existing, OpenDataSubmission updated)
        {
            existing.DatasetTitle = updated.DatasetTitle;
            existing.Status = updated.Status;
            existing.OpenForAttachingFiles = updated.OpenForAttachingFiles;
        }


        #region Dummy Test Data

        private async Task<TbsOpenGovSubmission> CreateTestSubmission(long submissionId)
        {
            var currentUser = await _userService.GetCurrentPortalUserAsync();

            var files = new List<OpenDataPublishFile>()
            {
                new(){FileName="Dataset.csv",FileId=Guid.NewGuid().ToString(), FilePurpose=TbsOpenGovSubmission.DATASET_FILE_TYPE },
                new() {FileName="Metadata.xlsx",FileId=Guid.NewGuid().ToString()},
                new(){FileName="DataDictionary.docx",FileId=Guid.NewGuid().ToString()},
                new(){FileName="Supportingdoc1.docx" ,FileId=Guid.NewGuid().ToString(), FilePurpose=TbsOpenGovSubmission.SUPPORTING_DOC_FILE_TYPE},
                new(){FileName="Supportingdoc2.docx" ,FileId=Guid.NewGuid().ToString(), FilePurpose=TbsOpenGovSubmission.SUPPORTING_DOC_FILE_TYPE},

                new() {FileName = "IMSOApproval.pdf",FileId=Guid.NewGuid().ToString()}
            };

            var submission = new TbsOpenGovSubmission()
            {
                Id = submissionId,
                ProcessType = OpenDataPublishProcessType.TbsOpenGovPublishing,
                DatasetTitle = "Test Data",
                Status = TbsOpenGovSubmission.ProcessSteps.AwaitingFiles.ToString(),
                RequestDate = DateTime.Today,
                OpenGovCriteriaMetDate = DateTime.Today,
                RequestingUserId = currentUser.Id,
                RequestingUser = currentUser,
                Files = files
            };

            return submission;
        }

        #endregion
    }
}
