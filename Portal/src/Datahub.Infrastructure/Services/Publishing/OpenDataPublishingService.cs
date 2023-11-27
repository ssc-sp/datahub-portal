using Datahub.Application.Services.Publishing;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services;
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
                ProcessType = OpenDataPublishProcessType.TBSOpenGov,
                RequestDate = DateTime.Today,
                RequestingUserId = currentUser.Id,
                RequestingUser = currentUser,
                Files = files
            };

            return submission;
        }

        #endregion
    }
}
