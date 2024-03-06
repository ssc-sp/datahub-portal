using Datahub.CKAN.Service;
using Datahub.Core.Model.Datahub;
using Datahub.Metadata.DTO;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Portal.Services;

public class OpenDataService : IOpenDataService
{
    readonly ICKANServiceFactory _serviceFactory;
    readonly IHttpClientFactory _httpClientFactory;
    readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;

    public OpenDataService(IHttpClientFactory httpClientFactory, IDbContextFactory<DatahubProjectDBContext> dbContextFactory, ICKANServiceFactory serviceFactory)
    {
        _serviceFactory = serviceFactory;
        _dbContextFactory = dbContextFactory;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<CKANApiResult> PublishFileAsUrl(FieldValueContainer fileMetadata, bool allFields, string url)
    {
        var service = _serviceFactory.CreateService();
        return await service.CreatePackage(fileMetadata, allFields, url);
    }

    public Task PublishFile(FieldValueContainer fileMetadata, long sharedRecordId, string fileId, string fileName, string fileUrl)
    {
        var task = DownloadFileContent(fileUrl, async stream =>
        {
            SetFileShareStatus(sharedRecordId, OpenDataUploadStatus.UploadingFile);

            var ckanService = _serviceFactory.CreateService();

            // publish to open data record
            var result = await ckanService.CreatePackage(fileMetadata, false);
            if (result.Succeeded)
            {
                SetFileShareStatus(sharedRecordId, OpenDataUploadStatus.RecordCreated);

                // publish to open data resource
                result = await ckanService.AddResourcePackage(fileId, fileName, stream);
                if (result.Succeeded)
                {
                    // record successfull uploaded
                    SetFileShareCompleted(sharedRecordId);
                }
                else
                {
                    // create resource failed
                    SetFileShareStatus(sharedRecordId, OpenDataUploadStatus.Failed, result.ErrorMessage);
                }
            }
            else
            {
                // create package failed
                SetFileShareStatus(sharedRecordId, OpenDataUploadStatus.Failed, result.ErrorMessage);
            }
        });
        return task;
    }

    public async Task<CKANApiResult> UnpublishFile(string fileId)
    {
        var service = _serviceFactory.CreateService();
        var apiResult = await service.DeletePackage(fileId);
        if (apiResult.Succeeded)
        {
            SetFileUnpublishedCompleted(fileId);
        }
        return apiResult;
    }

    public bool IsStaging() => _serviceFactory.IsStaging();

    private async Task DownloadFileContent(string fileUrl, Func<Stream, Task> processFile)
    {
        var httpClient = _httpClientFactory.CreateClient();
        using (var response = await httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead))
        {
            response.EnsureSuccessStatusCode();
            await using (var fileData = await response.Content.ReadAsStreamAsync())
            {
                await processFile.Invoke(fileData);
            }
        }
    }

    private void SetFileShareStatus(long sharedFileId, OpenDataUploadStatus status, string errorMessage = null)
    {
        using var ctx = _dbContextFactory.CreateDbContext();
        var sharedFile = ctx.OpenDataSharedFiles.FirstOrDefault(e => e.SharedDataFileID == sharedFileId);
        if (sharedFile is not null)
        {
            sharedFile.UploadStatusCD = status;
            sharedFile.UploadErrorTXT = errorMessage;
            // todo: use auditing service here
            ctx.SaveChanges();
        }
    }

    private void SetFileShareCompleted(long sharedFileId)
    {
        using var ctx = _dbContextFactory.CreateDbContext();
        var sharedFile = ctx.OpenDataSharedFiles.FirstOrDefault(e => e.SharedDataFileID == sharedFileId);
        if (sharedFile is not null)
        {
            sharedFile.UploadStatusCD = OpenDataUploadStatus.UploadCompleted;
            sharedFile.UploadErrorTXT = string.Empty;
            sharedFile.FileStorageCD = FileStorageType.OpenData;
            // todo: use auditing service here
            ctx.SaveChanges();
        }
    }

    private void SetFileUnpublishedCompleted(string fileId)
    {
        var fileGuid = Guid.Parse(fileId);
        using var ctx = _dbContextFactory.CreateDbContext();
        var sharedFile = ctx.OpenDataSharedFiles.FirstOrDefault(e => e.FileID == fileGuid);
        if (sharedFile is not null)
        {
            sharedFile.UnpublishDateDT = DateTime.UtcNow;
            ctx.SaveChanges();
        }
    }
}