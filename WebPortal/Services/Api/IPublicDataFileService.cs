using System;
using System.Threading.Tasks;
using Microsoft.Graph;
using Datahub.Core.Data;
using System.Collections.Generic;
using Datahub.Core.EFCore;

namespace Datahub.Portal.Services
{
    public interface IPublicDataFileService
    {
        Task<Uri> DownloadPublicUrlSharedFile(Guid fileId);
        Task<Uri> DoDownloadFile(SharedDataFile publicFile);
        Task CreateDataSharingRequest(FileMetaData fileMetaData, string projectCode, User requestingUser, bool openDataRequest = false);
        Task<SharedDataFile> LoadPublicUrlSharedFileInfo(Guid fileId);
        Task<OpenDataSharedFile> LoadOpenDataSharedFileInfo(Guid fileId);
        Task<bool> MarkMetadataComplete(Guid fileId);
        Task<bool> SaveTempSharingExpiryDate(Guid fileId, DateTime? expiryDate);
        Task<bool> SubmitPublicUrlShareForApproval(Guid fileId);
        Task<int> GetDataSharingRequestsAwaitingApprovalCount(string projectCode);
        Task<int> GetUsersOwnDataSharingRequestsCount(string projectCode, string requestingUserId);
        Task<List<SharedDataFile>> GetProjectSharingRequestsAwaitingApproval(string projectCode);
        Task ApprovePublicUrlShare(Guid fileId, DateTime? publicationDate = null);
        Task DenyPublicUrlShare(Guid fileId);
        Task UpdateOpenDataApprovalFormId(Guid fileId, int approvalFormId);
        Task UpdateOpenDataSignedApprovalFormUrl(Guid fileId, string url);
        Task<List<SharedDataFile>> GetAllSharedDataForProject(string projectCode);
        Task<bool> IsUserProjectDataApprover(string projectCode, string userId);
        Task CancelPublicDataShare(Guid fileId);
        string GetOpenDataApprovalFormPdfUrl(int approvalFormId);
    }
}