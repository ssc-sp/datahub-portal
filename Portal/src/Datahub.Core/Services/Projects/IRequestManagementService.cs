using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Datahub.Core.Model.Projects;
using Datahub.Shared.Entities;

namespace Datahub.Core.Services.Projects;

public record ProjectResourceFormParams(FieldDefinitions FieldDefinitions, MetadataProfile Profile);

public interface IRequestManagementService
{
    public const string DATABRICKS = ProjectResourceConstants.SERVICE_TYPE_DATABRICKS;
    public const string POWERBI = ProjectResourceConstants.SERVICE_TYPE_POWERBI;
    public const string STORAGE = ProjectResourceConstants.SERVICE_TYPE_STORAGE;
    public const string SQLSERVER = ProjectResourceConstants.SERVICE_TYPE_SQL_SERVER;
    public const string POSTGRESQL = ProjectResourceConstants.SERVICE_TYPE_POSTGRES;
    public const string VIRTUAL_MACHINE  = ProjectResourceConstants.SERVICE_TYPE_VIRTUAL_MACHINE;

    Task<ProjectResourceFormParams> CreateResourceInputFormParams(string resourceType);
    Task<Dictionary<string, string>> GetDefaultValues(string resourceType);
    Task<string> GetResourceInputDefinitionJson(string resourceType);
    Task<List<Project_Resources2>> GetResourcesByRequest(Datahub_ProjectServiceRequests request);
    Task HandleRequestService(Datahub_Project project, string serviceType);
    Task<bool> HandleTerraformRequestServiceAsync(Datahub_Project project, string terraformTemplate);
    Task RequestService(Datahub_ProjectServiceRequests request, Dictionary<string, string> inputParams = null);
    Task RequestServiceWithDefaults(Datahub_ProjectServiceRequests request);
    Task SaveResourceInputDefinitionJson(string resourceType, string jsonContent);
    Task<bool> UpdateResourceInputParameters(Guid resourceId, Dictionary<string, string> inputParams);
    Task<WorkspaceDefinition> GetWorkspaceDefinition(string projectAcronym);
}