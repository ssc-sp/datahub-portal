using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Datahub.Shared.Data;
using Datahub.Shared.EFCore;
using Datahub.Shared.Services;

namespace Datahub.Portal.Services
{
    public class PublicDataFileService : IPublicDataFileService
    {
        private DatahubProjectDBContext _projectDbContext;
        private IApiService _apiService;
        private ILogger<IPublicDataFileService> _logger;

        public PublicDataFileService(
            IApiService apiService,
            DatahubProjectDBContext projectDbContext,
            ILogger<IPublicDataFileService> logger
        )
        {
            _apiService = apiService;
            _projectDbContext = projectDbContext;
            _logger = logger;
        }

        public async Task CreateDataSharingRequest(FileMetaData fileMetaData, string projectCode, User requestingUser, bool openDataRequest = false)
        {
            if (fileMetaData == null || requestingUser == null) 
            {
                _logger.LogError($"Null user {requestingUser} or file {fileMetaData}");
                return;
            }

            Guid fileId;
            if (!Guid.TryParse(fileMetaData.fileid, out fileId))
            {
                _logger.LogError($"Invalid file id: {fileMetaData.fileid}");
                return;
            }

            var existingFile = await _projectDbContext.SharedDataFiles.FirstOrDefaultAsync(e => e.File_ID == fileId);
            if (existingFile != null)
            {
                _logger.LogError($"File {fileId} already has a sharing record");
                return;
            }

            if (openDataRequest)
            {
                var shareRequest = new OpenDataSharedFile
                {
                    IsOpenDataRequest_FLAG = true,
                    File_ID = fileId,
                    Filename_TXT = fileMetaData.filename,
                    FolderPath_TXT = fileMetaData.folderpath,
                    ProjectCode_CD = projectCode?.ToLowerInvariant(),
                    RequestingUser_ID = requestingUser.Id,
                    RequestedDate_DT = DateTime.UtcNow
                };

                var dbResult = await _projectDbContext.OpenDataSharedFiles.AddAsync(shareRequest);
            }
            else 
            {
                var shareRequest = new SharedDataFile()
                {
                    File_ID = fileId,
                    Filename_TXT = fileMetaData.filename,
                    FolderPath_TXT = fileMetaData.folderpath,
                    ProjectCode_CD = projectCode?.ToLowerInvariant(),
                    RequestingUser_ID = requestingUser.Id,
                    RequestedDate_DT = DateTime.UtcNow
                };

                var dbResult = await _projectDbContext.SharedDataFiles.AddAsync(shareRequest);
            }

            var numSaved = await _projectDbContext.SaveChangesAsync();

            _logger.LogInformation($"Wrote file share record for {fileId} - {numSaved} records written to database");
        }

        public async Task<Uri> DownloadPublicUrlSharedFile(Guid fileId)
        {
            var publicFile = await LoadPublicUrlSharedFileInfo(fileId);
            
            if (publicFile == null) 
            {
                _logger.LogError($"File not found: {fileId}");
                //TODO an exception instead of returning null
                return await Task.FromResult<Uri>(null);
            }

            if (!publicFile.ApprovedDate_DT.HasValue || publicFile.ApprovedDate_DT > DateTime.UtcNow)
            {
                _logger.LogError($"File not approved for public: {fileId}");
                return await Task.FromResult<Uri>(null);
            }

            if (!publicFile.PublicationDate_DT.HasValue || publicFile.PublicationDate_DT > DateTime.UtcNow)
            {
                _logger.LogError($"File {fileId} is not yet published (publication: {publicFile.PublicationDate_DT?.ToShortDateString()})");
                return await Task.FromResult<Uri>(null);
            }

            return await DoDownloadFile(publicFile);

        }

        public async Task<Uri> DoDownloadFile(SharedDataFile publicFile)
        {
            var fileMetadata = new FileMetaData()
            {
                filename = publicFile.Filename_TXT,
                name = publicFile.Filename_TXT,
                folderpath = publicFile.FolderPath_TXT
            };

            if (publicFile.IsProjectBased)
            {
                return await _apiService.GetUserDelegationSasBlob(fileMetadata, publicFile.ProjectCode_CD.ToLowerInvariant());
            }
            else
            {
                return await _apiService.DownloadFile(fileMetadata);
            }
        }

        private System.Linq.Expressions.Expression<Func<SharedDataFile, bool>> GenerateSharingRequestsAwaitingApprovalCondition(string projectCode)
        {
            return f => f.ProjectCode_CD != null && f.ProjectCode_CD.ToLower() == projectCode.ToLower() 
                && f.SubmittedDate_DT.HasValue 
                && !f.ApprovedDate_DT.HasValue;
        } 

        public async Task<List<SharedDataFile>> GetProjectSharingRequestsAwaitingApproval(string projectCode)
        {
            return await _projectDbContext.SharedDataFiles
                .Where(GenerateSharingRequestsAwaitingApprovalCondition(projectCode))
                .ToListAsync();
        }

        public async Task<int> GetDataSharingRequestsAwaitingApprovalCount(string projectCode)
        {
            return await _projectDbContext.SharedDataFiles
                .CountAsync(GenerateSharingRequestsAwaitingApprovalCondition(projectCode));
        }

        public async Task<SharedDataFile> LoadPublicUrlSharedFileInfo(Guid fileId)
        {
            return await _projectDbContext.SharedDataFiles.FirstOrDefaultAsync(e => e.File_ID == fileId);
        }
        public async Task<OpenDataSharedFile> LoadOpenDataSharedFileInfo(Guid fileId)
        {
            return await _projectDbContext.OpenDataSharedFiles.FirstOrDefaultAsync(e => e.File_ID == fileId);
        }

        public async Task<bool> SubmitPublicUrlShareForApproval(Guid fileId)
        {
            var submission = await LoadPublicUrlSharedFileInfo(fileId);
            if (submission != null)
            {
                submission.SubmittedDate_DT = DateTime.UtcNow;
                var result = await _projectDbContext.SaveChangesAsync();
                if (result < 1)
                {
                    _logger.LogError($"Error submitting file {fileId}");
                    return false;
                }
                
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task ApprovePublicUrlShare(Guid fileId, DateTime? publicationDate = null)
        {
            var shareInfo = await LoadPublicUrlSharedFileInfo(fileId);
            
            shareInfo.ApprovedDate_DT = DateTime.UtcNow;
            shareInfo.PublicationDate_DT = publicationDate ?? shareInfo.ApprovedDate_DT;

            await _projectDbContext.SaveChangesAsync();
        }

        public async Task DenyPublicUrlShare(Guid fileId)
        {
            var shareInfo = await LoadPublicUrlSharedFileInfo(fileId);
            _projectDbContext.SharedDataFiles.Remove(shareInfo);
            // _projectDbContext.PublicDataFiles.Remove(shareInfo);
            await _projectDbContext.SaveChangesAsync();
        }

        public async Task UpdateOpenDataApprovalFormId(Guid fileId, int approvalFormId)
        {
            var shareInfo = await LoadOpenDataSharedFileInfo(fileId);

            shareInfo.ApprovalForm_ID = approvalFormId;

            await _projectDbContext.SaveChangesAsync();
        }

        public async Task UpdateOpenDataSignedApprovalFormUrl(Guid fileId, string url)
        {
            var shareInfo = await LoadOpenDataSharedFileInfo(fileId);

            shareInfo.SignedApprovalForm_URL = url;
            shareInfo.SubmittedDate_DT = DateTime.UtcNow;

            await _projectDbContext.SaveChangesAsync();
        }

        public async Task<List<SharedDataFile>> GetAllSharedDataForProject(string projectCode)
        {
            return await _projectDbContext.SharedDataFiles
                .Where(f => f.ProjectCode_CD != null && f.ProjectCode_CD.ToLower() == projectCode.ToLower())
                .ToListAsync();
        }

        public async Task<bool> IsUserProjectDataApprover(string projectCode, string userId)
        {
            var projectUserEntries = await _projectDbContext.Project_Users
                .Where(p => p.Project.Project_Acronym_CD == projectCode && p.User_ID == userId)
                .ToListAsync();
            return projectUserEntries.Any(p => p.IsDataApprover);
        }
    }
}