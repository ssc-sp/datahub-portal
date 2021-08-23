using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using NRCan.Datahub.Shared.Data;
using NRCan.Datahub.Shared.EFCore;
using NRCan.Datahub.Shared.Services;

namespace NRCan.Datahub.Portal.Services
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

        public async Task CreateFileShareRequest(FileMetaData fileMetaData, string projectCode, User requestingUser)
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

            var existingFile = await _projectDbContext.PublicDataFiles.FirstOrDefaultAsync(e => e.File_ID == fileId);
            if (existingFile != null)
            {
                _logger.LogError($"File {fileId} already has a sharing record");
                return;
            }

            var shareRequest = new PublicDataFile()
            {
                File_ID = fileId,
                Filename_TXT = fileMetaData.filename,
                FolderPath_TXT = fileMetaData.folderpath,
                ProjectCode_CD = projectCode?.ToLowerInvariant(),
                RequestingUser_ID = requestingUser.Id,
                RequestedDate_DT = DateTime.UtcNow
            };

            var dbResult = await _projectDbContext.PublicDataFiles.AddAsync(shareRequest);
            var numSaved = await _projectDbContext.SaveChangesAsync();

            _logger.LogInformation($"Wrote file share record for {fileId} - {numSaved} records written to database");
        }

        public async Task<Uri> DownloadSharedFile(Guid fileId)
        {
            var publicFile = await LoadPublicDataFileInfo(fileId);
            
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

        public async Task<Uri> DoDownloadFile(PublicDataFile publicFile)
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

        private System.Linq.Expressions.Expression<Func<PublicDataFile, bool>> GenerateSharingRequestCondition(string projectCode)
        {
            return f => f.ProjectCode_CD != null && f.ProjectCode_CD.ToLower() == projectCode.ToLower() 
                && f.SubmittedDate_DT.HasValue 
                && !f.ApprovedDate_DT.HasValue;
        } 

        public async Task<List<PublicDataFile>> GetProjectSharingRequests(string projectCode)
        {
            return await _projectDbContext.PublicDataFiles
                .Where(GenerateSharingRequestCondition(projectCode))
                .ToListAsync();
        }

        public async Task<int> GetPublicUrlSharingRequestCount(string projectCode)
        {
            return await _projectDbContext.PublicDataFiles
                .CountAsync(GenerateSharingRequestCondition(projectCode));
        }

        public async Task<PublicDataFile> LoadPublicDataFileInfo(Guid fileId)
        {
            return await _projectDbContext.PublicDataFiles.FirstOrDefaultAsync(e => e.File_ID == fileId);
        }

        public async Task<bool> SubmitPublicUrlShareForApproval(Guid fileId)
        {
            var submission = await LoadPublicDataFileInfo(fileId);
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
            var shareInfo = await LoadPublicDataFileInfo(fileId);
            
            shareInfo.ApprovedDate_DT = DateTime.UtcNow;
            shareInfo.PublicationDate_DT = publicationDate ?? shareInfo.ApprovedDate_DT;

            await _projectDbContext.SaveChangesAsync();
        }

        public async Task DenyPublicUrlShare(Guid fileId)
        {
            var shareInfo = await LoadPublicDataFileInfo(fileId);
            _projectDbContext.PublicDataFiles.Remove(shareInfo);
            await _projectDbContext.SaveChangesAsync();
        }
    }
}