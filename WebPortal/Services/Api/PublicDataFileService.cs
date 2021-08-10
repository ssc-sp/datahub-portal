using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using NRCan.Datahub.Data.Projects;
using NRCan.Datahub.Shared.Data;
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

        public async Task CreateFileShareRequest(FileMetaData fileMetaData, string projcetCode, User requestingUser)
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
                ProjectCode_CD = projcetCode,
                RequestingUser_ID = requestingUser.Id,
                RequestingUserEmail_TXT = requestingUser.UserPrincipalName,
                RequestedDate_DT = DateTime.UtcNow
            };

            var dbResult = await _projectDbContext.PublicDataFiles.AddAsync(shareRequest);
            var numSaved = await _projectDbContext.SaveChangesAsync();

            _logger.LogInformation($"Wrote file share record for {fileId} - {numSaved} records written to database");
        }

        public async Task<Uri> DownloadSharedFile(Guid fileId)
        {
            var publicFile = await _projectDbContext.PublicDataFiles.FirstOrDefaultAsync(e => e.File_ID == fileId);
            if (publicFile == null) 
            {
                _logger.LogError($"File not found: {fileId}");
                //TODO an exception instead of returning null
                return await Task.FromResult<Uri>(null);
            }
            else if (!publicFile.ApprovedDate_DT.HasValue)
            {
                _logger.LogError($"File not approved for public: {fileId}");
                return await Task.FromResult<Uri>(null);
            }

            var fileMetadata = new FileMetaData()
            {
                filename = publicFile.Filename_TXT,
                name = publicFile.Filename_TXT,
                folderpath = publicFile.FolderPath_TXT
            };

            if (publicFile.IsProjectBased)
            {
                return await _apiService.GetUserDelegationSasBlob(fileMetadata, publicFile.ProjectCode_CD);
            }
            else
            {
                return await _apiService.DownloadFile(fileMetadata);
            }
        }
    }
}