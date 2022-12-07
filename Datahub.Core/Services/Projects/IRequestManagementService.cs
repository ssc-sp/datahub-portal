using Datahub.Core.EFCore;
using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Datahub.Core.Services.Projects
{
    public record ProjectResourceFormParams(FieldDefinitions FieldDefinitions, MetadataProfile Profile);

    public interface IRequestManagementService
    {
        public const string DATABRICKS = ProjectResourceConstants.SERVICE_TYPE_DATABRICKS;
        public const string POWERBI = ProjectResourceConstants.SERVICE_TYPE_POWERBI;
        public const string STORAGE = ProjectResourceConstants.SERVICE_TYPE_STORAGE;
        public const string SQLSERVER = ProjectResourceConstants.SERVICE_TYPE_SQL_SERVER;
        public const string POSTGRESQL = ProjectResourceConstants.SERVICE_TYPE_POSTGRES;

        Task<ProjectResourceFormParams> CreateResourceInputFormParams(string resourceType);
        Task<Dictionary<string, string>> GetDefaultValues(string resourceType);
        Task<string> GetResourceInputDefinitionJson(string resourceType);
        Task<List<Project_Resources2>> GetResourcesByRequest(Datahub_ProjectServiceRequests request);
        Task HandleRequestService(Datahub_Project project, string serviceType);
        Task RequestService(Datahub_ProjectServiceRequests request, Dictionary<string, string> inputParams = null);
        Task RequestServiceWithDefaults(Datahub_ProjectServiceRequests request);
        Task SaveResourceInputDefinitionJson(string resourceType, string jsonContent);
        Task<bool> UpdateResourceInputParameters(Guid resourceId, Dictionary<string, string> inputParams);
    }
}