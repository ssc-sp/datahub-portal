using Datahub.Application.Services;
using Datahub.Application.Services.Publishing;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.Storage;
using Datahub.CKAN.Package;
using Datahub.CKAN.Service;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.Metadata;
using Datahub.Infrastructure.Services.Storage;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg;

namespace Datahub.Infrastructure.Services.Publishing;

public class TbsOpenDataService(IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        ICKANServiceFactory ckanServiceFactory,
        IMetadataBrokerService metadataBrokerService,
        IProjectStorageConfigurationService projectStorageConfigService,
        CloudStorageManagerFactory cloudStorageManagerFactory,
        IHttpClientFactory httpClientFactory,
        IOpenDataPublishingService publishingService,
        IKeyVaultUserService keyvaultUserService) : ITbsOpenDataService
{
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory = dbContextFactory;
    private readonly ICKANServiceFactory _ckanServiceFactory = ckanServiceFactory;
    private readonly IMetadataBrokerService _metadataBrokerService = metadataBrokerService;
    private readonly IProjectStorageConfigurationService _projectStorageConfigService = projectStorageConfigService;
    private readonly CloudStorageManagerFactory _cloudStorageManagerFactory = cloudStorageManagerFactory;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IOpenDataPublishingService _publishingService = publishingService;
    private readonly IKeyVaultUserService _keyvaultUserService = keyvaultUserService;

    public async Task<CKANApiResult> CreateOrFetchPackage(OpenDataSubmission submission)
    {
        var metadata = await _metadataBrokerService.GetObjectMetadataValues(submission.UniqueId, null);
        if (metadata == null)
        {
            throw new OpenDataPublishingException($"Metadata not found for submission with ID {submission.Id} (unique id: {submission.UniqueId})");
        }

        var apiKey = await GetApiKeyForWorkspace(submission.Project.Project_Acronym_CD);
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new OpenDataPublishingException($"TBS OpenGov API Key not found for workspacce {submission.Project.Project_Acronym_CD}");
        }

        var ckanService = _ckanServiceFactory.CreateService(apiKey);

        var result = await ckanService.CreateOrFetchPackage(metadata, false);

        return await Task.FromResult(result);
    }

    private async Task<ICloudStorageManager> GetCloudStorageManagerAsync(OpenDataPublishFile publishFile)
    {
        if (publishFile.ProjectStorageId.HasValue)
        {
            //TODO fetch from db, etc
            throw new NotImplementedException();
        }
        else
        {
            var projectAcronym = publishFile.Submission.Project.Project_Acronym_CD;
            var accountName = _projectStorageConfigService.GetProjectStorageAccountName(projectAcronym);
            var accountKey = await _projectStorageConfigService.GetProjectStorageAccountKey(projectAcronym);
            var storageManager = new AzureCloudStorageManager(accountName, accountKey);
            return await Task.FromResult(storageManager);
        }
    }

    // TODO put into a util class somewhere
    private static string JoinPath(string folder, string fileName)
    {
        var splitPath = (folder ?? "").Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
        splitPath.Add(fileName);
        return string.Join("/", splitPath);
    }

    private async Task DownloadFileContent(Uri fileUrl, Func<Stream, Task> processFile)
    {
        var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        await using var fileData = await response.Content.ReadAsStreamAsync();
        await processFile.Invoke(fileData);
    }


    public async Task<OpenDataPublishFile> UploadFile(OpenDataPublishFile publishFile)
    {
        try
        {
            var updatedFile = await _publishingService.UpdateFileUploadStatus(publishFile, OpenDataPublishFileUploadStatus.InProgress);

            var packageApiResult = await CreateOrFetchPackage(publishFile.Submission);

            if (packageApiResult.CkanObject is not CkanPackageBasic package)
            {
                throw new OpenDataPublishingException($"Package not found for submission with ID {publishFile.Submission.Id} (unique id: {publishFile.Submission.UniqueId})");
            }

            var metadata = await _metadataBrokerService.GetObjectMetadataValues(publishFile.FileId, null);
            if (metadata == null)
            {
                throw new OpenDataPublishingException($"Metadata not found for file {publishFile.FileName}");
            }

            var apiKey = await GetApiKeyForWorkspace(publishFile.Submission.Project.Project_Acronym_CD);
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new OpenDataPublishingException($"TBS OpenGov API Key not found for workspacce {publishFile.Submission.Project.Project_Acronym_CD}");
            }

            var cloudStorageManager = await GetCloudStorageManagerAsync(publishFile);
            var fullFilePath = JoinPath(publishFile.FolderPath, publishFile.FileName);
            var downloadUrl = await cloudStorageManager.DownloadFileAsync(publishFile.ContainerName, fullFilePath);

            // fire and forget
            var task = DownloadFileContent(downloadUrl, async stream =>
            {
                var ckanService = _ckanServiceFactory.CreateService(apiKey);
                try
                {
                    var uploadResult = await ckanService.AddResourcePackage(package.Id, publishFile.FileName, publishFile.FilePurpose, metadata, stream);

                    if (uploadResult.Succeeded)
                    {
                        await _publishingService.UpdateFileUploadStatus(publishFile, OpenDataPublishFileUploadStatus.Completed);
                    }
                    else
                    {
                        await _publishingService.UpdateFileUploadStatus(publishFile, OpenDataPublishFileUploadStatus.Failed, uploadResult.ErrorMessage);
                    }
                }
                catch (Exception e)
                {
                    await _publishingService.UpdateFileUploadStatus(publishFile, OpenDataPublishFileUploadStatus.Failed, e.Message);
                }
            });

            return await Task.FromResult(updatedFile);
        }
        catch (OpenDataPublishingException e)
        {
            var failedFile = await _publishingService.UpdateFileUploadStatus(publishFile, OpenDataPublishFileUploadStatus.Failed, e.Message);
            return await Task.FromResult(failedFile);
        }
    }

    public async Task<bool> IsApiKeyConfiguredForWorkspace(string workspaceAcronym)
    {
        var apiKey = await GetApiKeyForWorkspace(workspaceAcronym);
        return await Task.FromResult(!string.IsNullOrEmpty(apiKey));
    }

    public async Task<string?> GetApiKeyForWorkspace(string workspaceAcronym)
    {
        return await _keyvaultUserService.GetSecret(workspaceAcronym, ITbsOpenDataService.WORKSPACE_CKAN_API_KEY);
    }

    public async Task SetApiKeyForWorkspace(string workspaceAcronym, string apiKey)
    {
        await _keyvaultUserService.StoreSecret(workspaceAcronym, ITbsOpenDataService.WORKSPACE_CKAN_API_KEY, apiKey);
    }
}
