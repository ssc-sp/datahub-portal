﻿using Datahub.CKAN.Service;
using Datahub.Core.Model.Datahub;

namespace Datahub.Application.Services.Publishing;

public interface ITbsOpenDataService
{
    public const string WORKSPACE_CKAN_API_KEY = "TbsCkanApiKey";
    Task<CKANApiResult> CreateOrFetchPackage(OpenDataSubmission submission);
    Task<OpenDataPublishFile> UploadFile(OpenDataPublishFile publishFile);
    Task<bool> IsApiKeyConfiguredForWorkspace(string workspaceAcronym);
    Task<string?> GetApiKeyForWorkspace(string workspaceAcronym);
    Task SetApiKeyForWorkspace(string workspaceAcronym, string apiKey);
    Task<bool> IsWorkspaceReadyForSubmission(string workspaceAcronym);
}
