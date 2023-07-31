using Datahub.Core.Configuration;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services;
using Datahub.ProjectTools.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Datahub.ProjectTools.Services;

public static class ProjectResourcesListingServiceExtensions
{
    public static void AddProjectResources(this IServiceCollection services)
    {
        ProjectResourcesListingService.RegisterResources(services);
        services.AddScoped<ProjectResourcesListingService>();
    }
}

public class ProjectResourcesListingService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IUserInformationService _userInformationService;
    private readonly GitHubToolsService _githubToolsService;
    private readonly IOptions<DataProjectsConfiguration> _configuration;
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbFactoryProject;
    private readonly ILogger<ProjectResourcesListingService> _logger;

    public ProjectResourcesListingService(IServiceProvider serviceProvider,
        IUserInformationService userInformationService,
        GitHubToolsService githubToolsService,
        IOptions<DataProjectsConfiguration> configuration,
        ILogger<ProjectResourcesListingService> logger,
        IDbContextFactory<DatahubProjectDBContext> dbFactoryProject)
    {
        _serviceProvider = serviceProvider;
        _userInformationService = userInformationService;
        _githubToolsService = githubToolsService;
        _configuration = configuration;
        _dbFactoryProject = dbFactoryProject;
        _logger = logger;
    }

    private static readonly Type[] ResourceProviders = {
        typeof(DHPublicSharing)
    }; 

    public static void RegisterResources(IServiceCollection services)
    {
        foreach (var resource in ResourceProviders)
            services.AddTransient(resource);
        services.AddTransient<ServiceCatalogGitModuleResource>();
    }

    public async Task<List<IProjectResource>> GetResourcesForProject(string projectAcronym)
    {
        await using var ctx = await _dbFactoryProject.CreateDbContextAsync();
        //load requests for project
        
        var project = await ctx.Projects
            .AsSingleQuery()
            .Include(p => p.ServiceRequests)
            .Include(p => p.Resources)
            .Include(p => p.Whitelist)
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);

        if (project == null)
        {
            _logger.LogError("Project {ProjectAcronym} not found", projectAcronym);
            return new List<IProjectResource>();
        }

        var services = new ServiceCollection();

        using var serviceScope = _serviceProvider.CreateScope();
        var authUser = await _userInformationService.GetAuthenticatedUser();
        var output = new List<IProjectResource>();
        var isUserAdmin = await _userInformationService.IsUserProjectAdmin(project.Project_Acronym_CD);
        var isUserDHAdmin = await _userInformationService.IsUserDatahubAdmin();

        var allResourceProviders = GetAllResourceProviders(_configuration.Value);

        foreach (var item in allResourceProviders)
        {
            _logger.LogDebug("Configuring {ItemName} in project {ProjectProjectId} ({ProjectProjectName})", item.Name, project.Project_ID, project.Project_Name);
            try
            {
                if (serviceScope.ServiceProvider.GetRequiredService(item) is IProjectResource dhResource)
                {
                    await dhResource.InitializeAsync(project, await _userInformationService.GetUserIdString(),
                        await _userInformationService.GetCurrentGraphUserAsync(), isUserAdmin || isUserDHAdmin);
                    output.Add(dhResource);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configuring {ItemName} in project {ProjectProjectId} ({ProjectProjectName})", item.Name, project.Project_ID, project.Project_Name);
            }
        }

        _logger.LogTrace("Git module repository enabled:{ValueEnableGitModuleRepository}", _configuration.Value.EnableGitModuleRepository);
        if (_configuration.Value.EnableGitModuleRepository)
        {
            try
            {
                var githubModules = await _githubToolsService.GetAllModules();
                foreach (var item in githubModules)
                {
                    _logger.LogDebug($"Configuring {item.Name} in project {project.Project_ID} ({project.Project_Name})");
                    var gitModule = serviceScope.ServiceProvider.GetRequiredService<ServiceCatalogGitModuleResource>();
                    gitModule.ConfigureGitModule(item);
                    await gitModule.InitializeAsync(project, await _userInformationService.GetUserIdString(),
                        await _userInformationService.GetCurrentGraphUserAsync(), isUserAdmin || isUserDHAdmin);
                    output.Add(gitModule);
                }
            }
            catch
            {
                #if !DEBUG
                throw;
                #endif
            }
        }

        return output;
    }

    private static List<Type> GetAllResourceProviders(DataProjectsConfiguration config)
    {
        var allResourceProviders = new List<Type>(ResourceProviders);

        if (config.PowerBI)
        {
            allResourceProviders.Add(typeof(DHPowerBIResource));
        }

        if (config.PublicSharing)
        {
            allResourceProviders.Add(typeof(DHPublicSharing));
        }

        if (config.DataEntry)
        {
            allResourceProviders.Add(typeof(DHDataEntry));
        }

        // add legacy Databricks and Storage in case Git Modules are disabled
        if (!config.EnableGitModuleRepository)
        {
            allResourceProviders.Add(typeof(DHStorageResource));
            allResourceProviders.Add(typeof(DHDatabricksResource));
        }

        return allResourceProviders;
    }
}