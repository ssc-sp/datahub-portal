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
    Task<string> GetResourceInputDefinitionJson(string resourceType);
    Task<List<Project_Resources2>> GetResourcesByRequest(Datahub_ProjectRequestAudit request);
    
    [Obsolete("Use HandleTerraformRequestServiceAsync instead")]
    Task HandleRequestService(Datahub_Project project, string serviceType);
    
    /// <summary>
    /// This method is used to handle the terraform request service, it takes in the project and the terraform template to run
    /// </summary>
    /// <param name="project"></param>
    /// <param name="terraformTemplate"></param>
    /// <returns>
    /// Returns true if the terraform request service was handled successfully, false otherwise
    /// </returns>
    Task<bool> HandleTerraformRequestServiceAsync(Datahub_Project project, string terraformTemplate);
    
    Task HandleUserUpdatesToExternalPermissions(Datahub_Project project);
    Task SaveRequestToAuditing(Datahub_ProjectRequestAudit request);
    Task SaveResourceInputDefinitionJson(string resourceType, string jsonContent);
    Task<bool> UpdateResourceInputParameters(Guid resourceId, Dictionary<string, string> inputParams);
}