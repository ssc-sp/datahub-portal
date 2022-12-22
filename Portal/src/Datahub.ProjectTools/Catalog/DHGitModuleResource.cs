using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.Projects;
using Datahub.Core.Services.ProjectTools;
using Datahub.Core.Services.UserManagement;
using Datahub.ProjectTools.Services;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        if (currentModule.Name == "azure-storage-blob")
        {
            return project.Resources
                .Where(r => r.ResourceType == "azure-storage-blob")
                .Select(_ => (typeof(StorageResourceCard), GetActiveParameters()))
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

    private IDictionary<string, object> GetActiveParameters()
    {
        return new Dictionary<string, object>()
        {
              { nameof(StorageResourceCard.Descriptor), descriptor },
              { nameof(StorageResourceCard.Icon), currentModule.Icon ?? "fa-solid fa-cloud-question" },
            { nameof(StorageResourceCard.IsIconSvg), false },
            { nameof(StorageResourceCard.TemplateName),currentModule.Name  },
            { nameof(StorageResourceCard.Project), project }
        };
    }

    protected Dictionary<string, object> GetInactiveParameters()
        => new Dictionary<string, object>
        {
            { nameof(InactiveTerraformResource.Descriptor), descriptor },
            { nameof(InactiveTerraformResource.Icon), currentModule.Icon },
            { nameof(InactiveTerraformResource.IsIconSvg), false },
            { nameof(InactiveTerraformResource.ResourceRequested),serviceRequested  },            
            { nameof(InactiveTerraformResource.TemplateName),currentModule.Name  },
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
        //cultureService.IsEnglish ?
        descriptor = item.Descriptors.First(l => l.Language == "en");
        var frDescriptor = item.Descriptors.First(l => l.Language == "fr") ?? descriptor;
        if (cultureService.IsFrench)
            descriptor = frDescriptor;
    }
}