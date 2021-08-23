using System;
using System.Threading.Tasks;
using Microsoft.Graph;
using NRCan.Datahub.Shared.Data;
using System.Collections.Generic;
using NRCan.Datahub.Shared.EFCore;

namespace NRCan.Datahub.Portal.Services
{
    public interface IPublicDataFileService
    {
        Task<Uri> DownloadSharedFile(Guid fileId);
        Task<Uri> DoDownloadFile(PublicDataFile publicFile);
        Task CreateFileShareRequest(FileMetaData fileMetaData, string projectCode, User requestingUser);
        Task<PublicDataFile> LoadPublicDataFileInfo(Guid fileId);
        Task<bool> SubmitPublicUrlShareForApproval(Guid fileId);
        Task<int> GetPublicUrlSharingRequestCount(string projectCode);
        Task<List<PublicDataFile>> GetProjectSharingRequests(string projectCode);
        Task ApprovePublicUrlShare(Guid fileId, DateTime? publicationDate = null);
        Task DenyPublicUrlShare(Guid fileId);
    }
}