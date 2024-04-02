using Datahub.Application.Configuration;
using Datahub.Application.Exceptions;
using Datahub.Application.Services;
using Datahub.Application.Services.Publishing;
using Datahub.Application.Services.Security;
using Datahub.CKAN.Package;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.Metadata;
using Datahub.Core.Storage;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Metadata.DTO;
using Datahub.Metadata.Utils;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Services.Publishing;

public class TbsOpenDataService(IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        ICKANServiceFactory ckanServiceFactory,
        IMetadataBrokerService metadataBrokerService,
        IProjectStorageConfigurationService projectStorageConfigService,
        CloudStorageManagerFactory cloudStorageManagerFactory,
        IHttpClientFactory httpClientFactory,
        IOpenDataPublishingService publishingService,
        IKeyVaultUserService keyvaultUserService,
        DatahubPortalConfiguration config) : ITbsOpenDataService
{
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory = dbContextFactory;
    private readonly ICKANServiceFactory _ckanServiceFactory = ckanServiceFactory;
    private readonly IMetadataBrokerService _metadataBrokerService = metadataBrokerService;
    private readonly IProjectStorageConfigurationService _projectStorageConfigService = projectStorageConfigService;
    private readonly CloudStorageManagerFactory _cloudStorageManagerFactory = cloudStorageManagerFactory;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IOpenDataPublishingService _publishingService = publishingService;
    private readonly IKeyVaultUserService _keyvaultUserService = keyvaultUserService;
    private readonly DatahubPortalConfiguration _config = config;

    public async Task<CKANApiResult> CreateOrFetchPackage(OpenDataSubmission submission)
    {
        var submissionMetadata = await _metadataBrokerService.GetObjectMetadataValues(submission.UniqueId);
        if (submissionMetadata == null)
        {
            throw new OpenDataPublishingException($"Metadata not found for submission with ID {submission.Id} (unique id: {submission.UniqueId})");
        }

        await ApplyWorkspaceOwnerOrgToMetadata(submissionMetadata, submission.Project.Project_Acronym_CD);

        var ckanService = await CreateCkanServiceUsingWorkspaceApi(submission.Project.Project_Acronym_CD);

        var result = await ckanService.CreateOrFetchPackage(submissionMetadata, false);

        return await Task.FromResult(result);
    }

    private async Task<ICKANService> CreateCkanServiceUsingWorkspaceApi(string workspaceAcronym)
    {
        var apiKey = await GetApiKeyForWorkspace(workspaceAcronym);
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new OpenDataPublishingException($"TBS OpenGov API Key not found for workspacce {workspaceAcronym}");
        }

        return CKANService.CreateService(_httpClientFactory, _config.CkanConfiguration, apiKey);
    }

    private async Task ApplyWorkspaceOwnerOrgToMetadata(FieldValueContainer submissionMetadata, string workspaceAcronym)
    {
        var workspaceMetadata = await _metadataBrokerService.GetObjectMetadataValues(workspaceAcronym);
        if (workspaceMetadata == null)
        {
            throw new OpenDataPublishingException($"Metadata not found for workspace {workspaceAcronym}");
        }

        submissionMetadata[FieldNames.opengov_owner_org].Value_TXT = workspaceMetadata[FieldNames.opengov_owner_org].Value_TXT;
    }

    private async Task<ICloudStorageManager> GetCloudStorageManagerAsync(OpenDataPublishFile publishFile)
    {
        var projectAcronym = publishFile.Submission.Project.Project_Acronym_CD;

        if (publishFile.ProjectStorageId.HasValue)
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();
            var storage = await ctx.ProjectCloudStorages.AsNoTracking().FirstOrDefaultAsync(s => s.Id == publishFile.ProjectStorageId);
            if (storage == null)
            {
                throw new OpenDataPublishingException($"Project cloud storage not found with id {publishFile.ProjectStorageId} (submission {publishFile.SubmissionId})");
            }

            var storageManager = await _cloudStorageManagerFactory.CreateCloudStorageManager(projectAcronym, storage);
            if (storageManager == null)
            {
                throw new OpenDataPublishingException($"Could not open cloud storage with id {publishFile.ProjectStorageId} (submission {publishFile.SubmissionId})");
            }

            return await Task.FromResult(storageManager);
        }
        else
        {
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

            var ckanService = await CreateCkanServiceUsingWorkspaceApi(publishFile.Submission.Project.Project_Acronym_CD);
            var cloudStorageManager = await GetCloudStorageManagerAsync(publishFile);
            var fullFilePath = JoinPath(publishFile.FolderPath, publishFile.FileName);
            var downloadUrl = await cloudStorageManager.DownloadFileAsync(publishFile.ContainerName, fullFilePath);

            // fire and forget
            var task = DownloadFileContent(downloadUrl, async stream =>
            {
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
        return await _keyvaultUserService.GetSecretAsync(workspaceAcronym, ITbsOpenDataService.WORKSPACE_CKAN_API_KEY_SECRET_NAME);
    }

    public async Task SetApiKeyForWorkspace(string workspaceAcronym, string apiKey)
    {
        await _keyvaultUserService.StoreSecret(workspaceAcronym, ITbsOpenDataService.WORKSPACE_CKAN_API_KEY_SECRET_NAME, apiKey);
    }

    public async Task<bool> IsWorkspaceReadyForSubmission(string workspaceAcronym)
    {
        var isApiKeySetup = await IsApiKeyConfiguredForWorkspace(workspaceAcronym);
        var workspaceMetadata = await _metadataBrokerService.GetObjectMetadataValues(workspaceAcronym);
        var isOrgSetup = !string.IsNullOrEmpty(workspaceMetadata?[FieldNames.opengov_owner_org]?.Value_TXT);
        return await Task.FromResult(isApiKeySetup && isOrgSetup);
    }

    private async Task<CKANApiResult> DoUpdatePackage(TbsOpenGovSubmission submission, Dictionary<string, string> updateAttributes)
    {
        var ckanService = await CreateCkanServiceUsingWorkspaceApi(submission.Project.Project_Acronym_CD);
        return await ckanService.UpdatePackageAttributes(submission.UniqueId, updateAttributes);
    }

    public async Task<CKANApiResult> UpdatePackageImsoApproval(TbsOpenGovSubmission submission, bool imsoApproved)
    {
        var updateAttributes = new Dictionary<string, string>()
        {
            { CkanPackageBasic.IMSO_APPROVAL_JSON_PROPERTY_NAME, imsoApproved.ToString().ToLowerInvariant() }
        };

        var result = await DoUpdatePackage(submission, updateAttributes);

        if (result.Succeeded)
        {
            submission.ImsoApprovedDate = imsoApproved ? DateTime.Today : null;
            await _publishingService.UpdateTbsOpenGovSubmission(submission);
        }

        return await Task.FromResult(result);
    }

    public async Task<CKANApiResult> UpdatePackagePublication(TbsOpenGovSubmission submission, bool published)
    {
        var pubDate = DateTime.Today;

        var updateAttributes = new Dictionary<string, string>()
        {
            { CkanPackageBasic.READY_TO_PUBLISH_JSON_PROPERTY_NAME, published.ToString().ToLowerInvariant() },
            { CkanPackageBasic.DATE_PUBLISHED_JSON_PROPERTY_NAME, pubDate.ToString("yyyy-MM-dd") },
        };

        var result = await DoUpdatePackage(submission, updateAttributes);

        if (result.Succeeded)
        {
            submission.OpenGovPublicationDate = published ? pubDate : null;
            await _publishingService.UpdateTbsOpenGovSubmission(submission);
        }

        return await Task.FromResult(result);
    }

    public string DerivePublishUrl(TbsOpenGovSubmission submission)
    {
        return $"{_config.CkanConfiguration.DatasetUrl}/{submission.UniqueId}";
    }

    public async Task<string> TestConnectivity()
    {
        using var httpClient = _httpClientFactory.CreateClient();
        var apiUrl = $"{_config.CkanConfiguration.ApiUrl}/action/organization_list";
        try
        {
            using var response = await httpClient.GetAsync(apiUrl);
            return $"{(int)response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (HttpRequestException e)
        {
            return $"HttpRequestException: Status {e.StatusCode} - Exception: {e}";
        }
        catch (Exception e)
        {
            return $"Other exception: {e}";
        }
    }
}
