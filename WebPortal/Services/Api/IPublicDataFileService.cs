using System;
using System.Threading.Tasks;
using Microsoft.Graph;
using Datahub.Core.Data;
using System.Collections.Generic;
using Datahub.Core.EFCore;
using System.Net;

namespace Datahub.Portal.Services
{
    public interface IPublicDataFileService
    {
        Task<Uri> DownloadPublicUrlSharedFile(Guid fileId, IPAddress ipAddress = null);
        Task<Uri> DoDownloadFile(SharedDataFile publicFile, bool anonymous = false, IPAddress ipAddress = null);
        Task CreateDataSharingRequest(FileMetaData fileMetaData, string projectCode, User requestingUser, bool openDataRequest = false);
        Task<string> CreateExternalOpenDataSharing(int approvalFormId, string fileId, string fileName, string fileUrl, string contactEmail, string projectCode);
        Task<SharedDataFile> LoadPublicUrlSharedFileInfo(Guid fileId);
        Task<OpenDataSharedFile> LoadOpenDataSharedFileInfo(Guid fileId);
        Task<bool> MarkMetadataComplete(Guid fileId);
        Task<bool> SaveTempSharingExpiryDate(Guid fileId, DateTime? expiryDate);
        Task<bool> SubmitPublicUrlShareForApproval(Guid fileId);
        Task<int> GetDataSharingRequestsAwaitingApprovalCount(string projectCode);
        Task<int> GetUsersOwnDataSharingRequestsCount(string projectCode, string requestingUserId);
        Task<List<SharedDataFile>> GetProjectSharingRequestsAwaitingApproval(string projectCode);
        Task ApprovePublicUrlShare(Guid fileId, DateTime? publicationDate = null);
        Task ApproveOpenDataShare(Guid fileId, DateTime publicationDate);
        Task DenyPublicUrlShare(Guid fileId);
        Task UpdateOpenDataApprovalFormId(Guid fileId, int approvalFormId);
        Task UpdateOpenDataSignedApprovalFormUrl(Guid fileId, string url);
        Task<List<SharedDataFile>> GetAllSharedDataForProject(string projectCode);
        Task<bool> IsUserProjectDataApprover(string projectCode, string userId);
        Task<Datahub_Project> GetProjectWithUsers(string projectCode);
        Task CancelPublicDataShare(Guid fileId);
        string GetOpenDataApprovalFormPdfUrl(int approvalFormId);
        string GetPublicSharedFileUrl(string fileId);
        Task<List<OpenDataSharedFile>> GetPendingApprovalOpenDataFiles();
        Task SetPendingApprovalOpenDataAsRead(OpenDataSharedFile file);
        Task SetPendingApprovalOpenDataAsApproved(OpenDataSharedFile file);
        Task NotifySignedPDFUploaded();
        Task<OpenDataSharedFile> UpdateOpenDataPublication(Guid fileId, bool urlSharing);
    }
}