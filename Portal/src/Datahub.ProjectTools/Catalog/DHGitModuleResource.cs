using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.UserManagement;
using Datahub.ProjectTools.Catalog.ResourceCards;
using Datahub.ProjectTools.Services;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;

namespace Datahub.ProjectTools.Catalog;

public class DHGitModuleResource : IProjectResource
{
    private GitHubModule currentModule;
    private GitHubModuleDescriptor descriptor;
    private Datahub_Project project;
    private bool serviceRequested;
    private readonly IDbContextFactory<DatahubProjectDBContext> dbFactoryProject;
    private readonly CultureService cultureService;

    public DHGitModuleResource(IDbContextFactory<DatahubProjectDBContext> dbFactoryProject, CultureService cultureService)
    {
        this.dbFactoryProject = dbFactoryProject;
        this.cultureService = cultureService;
    }

    public (Type type, IDictionary<string, object> parameters)[] GetActiveResources()
    {
        if (currentModule.Name == TerraformTemplate.AzureStorageBlob)
        {
            var resourceType = RequestManagementService.GetTerraformServiceType(TerraformTemplate.AzureStorageBlob);
            return project.Resources
                .Where(r => r.ResourceType == resourceType)
                .Select(_ => (typeof(StorageResourceCard), GetActiveParameters(TerraformTemplate.AzureStorageBlob)))
                .ToArray();
        }

        if (currentModule.Name == TerraformTemplate.AzureDatabricks)
        {
            var resourceType = RequestManagementService.GetTerraformServiceType(TerraformTemplate.AzureDatabricks);
            return project.Resources
                .Where(r => r.ResourceType == resourceType)
                .Select(_ => (typeof(DatabricksResourceCard), GetActiveParameters(TerraformTemplate.AzureDatabricks)))
                .ToArray();
        }
        return Array.Empty<(Type type, IDictionary<string, object> parameters)>();
    }

    public string? GetCostEstimatorLink()
    {
        // TODO: Get cost estimator link from module
        return null;
    }

    public (Type type, IDictionary<string, object> parameters)? GetInactiveResource()
    {
        return (typeof(InactiveTerraformResource), GetInactiveParameters());
    }

    private IDictionary<string, object> GetActiveParameters(string template)
    {
        switch (template)
        {
            case TerraformTemplate.AzureStorageBlob:
            case TerraformTemplate.AzureDatabricks:
                return new Dictionary<string, object>
                {
                    { nameof(StorageResourceCard.Descriptor), descriptor },
                    { nameof(StorageResourceCard.GitHubModule), currentModule },
                    { nameof(StorageResourceCard.IsIconSvg), false },
                    { nameof(StorageResourceCard.Project), project }
                };
        }

        return new Dictionary<string, object>();
    }

    protected Dictionary<string, object> GetInactiveParameters()
        => new()
        {
            { nameof(InactiveTerraformResource.Descriptor), descriptor },
            { nameof(InactiveTerraformResource.GitHubModule), currentModule},
            { nameof(InactiveTerraformResource.IsIconSvg), false },
            { nameof(InactiveTerraformResource.ResourceRequested),serviceRequested  },            
            { nameof(InactiveTerraformResource.Project), project },
        };
    public string[] GetTags() => descriptor.Tags;

    public async Task<bool> InitializeAsync(Datahub_Project project, string userId, User graphUser, bool isProjectAdmin)
    {
        await using var projectDbContext = await dbFactoryProject.CreateDbContextAsync();
        this.project = project;
        var serviceRequests = project.ServiceRequests;
        serviceRequested = serviceRequests.Any(r => r.ServiceType == RequestManagementService.GetTerraformServiceType(currentModule.Name) && r.Is_Completed == null);
        return true;
    }

    public void ConfigureGitModule(GitHubModule item)
    {
        currentModule = item;
        descriptor = item.Descriptors.First(l => l.Language == "en");
        var frDescriptor = item.Descriptors.First(l => l.Language == "fr") ?? descriptor;
        if (cultureService.IsFrench)
            descriptor = frDescriptor;
    }
}