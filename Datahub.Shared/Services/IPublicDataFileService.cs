using System;
using System.Threading.Tasks;
using Microsoft.Graph;
using NRCan.Datahub.Shared.Data;

namespace NRCan.Datahub.Portal.Services
{
    public interface IPublicDataFileService
    {
        Task<Uri> DownloadSharedFile(Guid fileId);
        Task CreateFileShareRequest(FileMetaData fileMetaData, string projectCode, User requestingUser);
    }
}