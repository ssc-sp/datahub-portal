using Datahub.Application.Services;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services.UserManagement;
using Datahub.ProjectTools.Catalog.ResourceCards;
using Datahub.ProjectTools.Services;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;

namespace Datahub.ProjectTools.Catalog;

public class ServiceCatalogGitModuleResource : IProjectResource
{
    private GitHubModule _currentModule;
    private GitHubModuleDescriptor _descriptor;
    private Datahub_Project _project;
    private bool _serviceRequested;
    private bool _serviceCreated;
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbFactoryProject;
    private readonly CultureService _cultureService;

    public ServiceCatalogGitModuleResource(IDbContextFactory<DatahubProjectDBContext> dbFactoryProject, CultureService cultureService)
    {
        _dbFactoryProject = dbFactoryProject;
        _cultureService = cultureService;
    }

    public (Type type, IDictionary<string, object> parameters)[] GetActiveResources()
    {
        switch (_currentModule.Name)
        {
            case TerraformTemplate.AzureStorageBlob:
            {
                var resourceType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureStorageBlob);
                return _project.Resources
                    .Where(r => r.ResourceType == resourceType)
                    .Select(_ => (typeof(StorageResourceCard), GetActiveParameters(TerraformTemplate.AzureStorageBlob)))
                    .ToArray();
            }
            case TerraformTemplate.AzureDatabricks:
            {
                var resourceType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureDatabricks);
                return _project.Resources
                    .Where(r => r.ResourceType == resourceType)
                    .Select(_ => (typeof(DatabricksResourceCard), GetActiveParameters(TerraformTemplate.AzureDatabricks)))
                    .ToArray();
            }
            default:
                return Array.Empty<(Type type, IDictionary<string, object> parameters)>();
        }
    }

    public string? GetCostEstimatorLink()
    {
        // TODO: Get cost estimator link from module
        return null;
    }

    public (Type type, IDictionary<string, object> parameters)? GetInactiveResource()
    {
        return (typeof(ServiceCatalogTerraformResource), GetInactiveParameters());
    }

    private IDictionary<string, object> GetActiveParameters(string template)
    {
        switch (template)
        {
            case TerraformTemplate.AzureStorageBlob:
            case TerraformTemplate.AzureDatabricks:
                return new Dictionary<string, object>
                {
                    { nameof(StorageResourceCard.Descriptor), _descriptor },
                    { nameof(StorageResourceCard.GitHubModule), _currentModule },
                    { nameof(StorageResourceCard.IsIconSvg), false },
                    { nameof(StorageResourceCard.Project), _project }
                };
        }

        return new Dictionary<string, object>();
    }

    protected Dictionary<string, object> GetInactiveParameters()
        => new()
        {
            { nameof(ServiceCatalogTerraformResource.Descriptor), _descriptor },
            { nameof(ServiceCatalogTerraformResource.GitHubModule), _currentModule},
            { nameof(ServiceCatalogTerraformResource.IsIconSvg), false },
            { nameof(ServiceCatalogTerraformResource.ResourceRequested),_serviceRequested },            
            { nameof(ServiceCatalogTerraformResource.ResourceCreated),_serviceCreated },            
            { nameof(ServiceCatalogTerraformResource.Project), _project },
            { nameof(ServiceCatalogTerraformResource.IsResourceWhitelisted), GetWhitelistStatus()}
        };
    public string[] GetTags() => _descriptor.Tags;

    public async Task<bool> InitializeAsync(Datahub_Project project, string? userId, User graphUser, bool isProjectAdmin)
    {
        await using var projectDbContext = await _dbFactoryProject.CreateDbContextAsync();
        _project = project;
        // var serviceRequests = project.ProjectRequestAudits;
        // _serviceRequested = serviceRequests.Any(r => r.RequestType == TerraformTemplate.GetTerraformServiceType(_currentModule.Name) && r.Is_Completed == null);
        
        // TODO: Check if service is created off a request GUID down the road
        // _serviceCreated = serviceRequests.Any(r => r.RequestType == TerraformTemplate.GetTerraformServiceType(_currentModule.Name) && r.Is_Completed != null);
        return true;
    }

    public void ConfigureGitModule(GitHubModule item)
    {
        _currentModule = item;
        _descriptor = item.Descriptors.First(l => l.Language == "en");
        var frDescriptor = item.Descriptors.First(l => l.Language == "fr") ?? _descriptor;
        if (_cultureService.IsFrench)
            _descriptor = frDescriptor;
    }
    
    private bool GetWhitelistStatus()
    {
        var whitelist = _project.Whitelist ?? new Project_Whitelist();
        return _currentModule.Name switch
        {
            TerraformTemplate.AzureDatabricks => whitelist.AllowDatabricks,
            TerraformTemplate.AzureStorageBlob => whitelist.AllowStorage,
            TerraformTemplate.AzureVirtualMachine => whitelist.AllowVMs,
            //Add future modules here
            _ => true
        };
    }
    
}