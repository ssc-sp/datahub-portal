using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using NRCan.Datahub.Shared.Data;
using NRCan.Datahub.Shared.EFCore;
using NRCan.Datahub.Shared.Services;

namespace NRCan.Datahub.Portal.Services
{
    public class PublicFileSharingConfiguration
    {
        public string OpenDataApprovalPdfBaseUrl { get; set; }
        public string OpenDataApprovalPdfFormIdParam { get; set; }
    }

    public class PublicDataFileService : IPublicDataFileService
    {
        private DatahubProjectDBContext _projectDbContext;
        private IApiService _apiService;
        private ILogger<IPublicDataFileService> _logger;
        private IMetadataBrokerService _metadataService;

        public static readonly string PUBLIC_FILE_SHARING_CONFIG_ROOT_KEY = "PublicFileSharing";
        private PublicFileSharingConfiguration _config;

        public PublicDataFileService(
            IApiService apiService,
            DatahubProjectDBContext projectDbContext,
            ILogger<IPublicDataFileService> logger,
            IMetadataBrokerService metadataService,
            IConfiguration config
        )
        {
            _apiService = apiService;
            _projectDbContext = projectDbContext;
            _logger = logger;
            _metadataService = metadataService;
            _config = new();
            config.Bind(PUBLIC_FILE_SHARING_CONFIG_ROOT_KEY, _config);
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
            return f => f.ProjectCode_CD != null 
                && f.ProjectCode_CD.ToLower() == projectCode.ToLower() 
                && f.SubmittedDate_DT.HasValue 
                && !f.ApprovedDate_DT.HasValue
                && !f.IsOpenDataRequest_FLAG;
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

        public async Task DenyPublicUrlShare(Guid fileId) => await DoDeletePublicUrlShare(fileId);

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

        private async Task DoDeletePublicUrlShare(Guid fileId)
        {
            var shareInfo = await LoadPublicUrlSharedFileInfo(fileId);
            _projectDbContext.SharedDataFiles.Remove(shareInfo);
            await _projectDbContext.SaveChangesAsync();
        }

        private async Task DoDeleteOpenDataShare(Guid fileId)
        {
            var shareInfo = await LoadOpenDataSharedFileInfo(fileId);
            if (shareInfo.ApprovalForm_ID.HasValue)
            {
                await _metadataService.DeleteApprovalForm(shareInfo.ApprovalForm_ID.Value);
            }
            _projectDbContext.OpenDataSharedFiles.Remove(shareInfo);
            await _projectDbContext.SaveChangesAsync();
        }

        public async Task CancelPublicDataShare(Guid fileId)
        {
            var shareInfo = await LoadPublicUrlSharedFileInfo(fileId);
            if (shareInfo.IsOpenDataRequest_FLAG)
            {
                await DoDeleteOpenDataShare(fileId);
            }
            else
            {
                await DoDeletePublicUrlShare(fileId);
            }
        }

        public string GetOpenDataApprovalFormPdfUrl(int approvalFormId)
        {
            //https://app.powerbi.com/groups/6ca76417-b205-43c2-be1b-6aeeedcb84ae/rdlreports/04b8625f-f532-43fe-89d5-5911bbabd79b?rp:p_approval_form_id=9&rdl:format=pdf
            return $"{_config.OpenDataApprovalPdfBaseUrl}?{_config.OpenDataApprovalPdfFormIdParam}={approvalFormId}&rdl:format=pdf";
        }
    }
}