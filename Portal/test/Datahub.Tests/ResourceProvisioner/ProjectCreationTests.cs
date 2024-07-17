using System.Collections.Generic;
using System.Linq;
using Xunit;
using System;
using System.Collections;
using System.Threading.Tasks;
using Datahub.Application.Services;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data.ResourceProvisioner;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services;
using Datahub.Infrastructure.Offline;
using Datahub.Infrastructure.Services;
using Foundatio.Queues;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Datahub.Core.Model.Context;

namespace Datahub.Tests.ResourceProvisioner;

public class ProjectCreationTests
{
    private IConfiguration _config;
    private const string ResourceProvisionerUrl = "https://localhost:7275";
    private static IEnumerable<T> LoadCollectionGeneric<TS,T>(ServiceProvider provider, Func<TS, IEnumerable> loadSource) where TS:DbContext
    {            
        //Expression<Func<S, IEnumerable>> expression = d => d.Projects;
        //Func<S, IEnumerable> loadSource = d => d.Projects;
        //IDbContextFactory
        var fac = provider.GetRequiredService<IDbContextFactory<TS>>();
        using var ctx = fac.CreateDbContext();
        return ((loadSource(ctx) as IEnumerable<T>) ?? throw new InvalidOperationException()).ToList();
    }
    private ServiceProvider SetupServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPooledDbContextFactory<DatahubProjectDBContext>(options => options.UseInMemoryDatabase("datahubProjects"));
        services.AddScoped<IProjectCreationService, ProjectCreationService>();
        
        //dependency for ProjectCreationService
        services.AddSingleton(Configuration);
        
        services.AddScoped<IUserInformationService, OfflineUserInformationService>();
        return services.BuildServiceProvider();
    }
    
    [Fact (Skip = "Needs to be validated")]
    public async Task GivenListOfProjects_CreateAcronyms()
    {
        // ReSharper disable StringLiteralTypo
        var projects = new[]
        {
            "Datahub", "Datahub Core", "Datahub Core Services", "Datahub Core Services API", 
            "Datahub Core Services App", "Datahub Core Services App API", "Datahub Core Services Application",
        };
        var acronyms = new List<string>();
        var projectCreationService =
            SetupServices().GetRequiredService<IProjectCreationService>();
        //var acronyms = projects.Select(p => p.Split(' ').Select(w => w[0]).Aggregate("", (a, b) => a + b));
        foreach (var project in projects)
        {
            var acronym = await projectCreationService.GenerateProjectAcronymAsync(project, acronyms);
            acronyms.Add(acronym);
        }
        // ReSharper disable StringLiteralTypo
        Assert.Equal(new[] { "DAT", "DACO", "DCS", "DCSA", "DCSA1", "DCSAA", "DCSA2" }, acronyms);
    }


    [Fact (Skip = "Needs to be validated")]
    public async Task GivenDatahubProjectWithoutAcronym_CreateResourcesAndAddProject()
    {
        const string projectName = "Datahub Unit Testing";
        const string organization = "Unit Testing";
        var serviceProvider = SetupServices();
        var projectCreationService = serviceProvider.GetRequiredService<IProjectCreationService>();
        var config = serviceProvider.GetRequiredService<IConfiguration>();
        var isAdded = await projectCreationService.CreateProjectAsync(projectName, organization);
        Assert.True(isAdded);
        var projects = LoadCollectionGeneric<DatahubProjectDBContext, Datahub_Project>(SetupServices(), d => d.Projects);
        Assert.Single(projects);
        //test api success
        using IQueue<CreateResourceData> queue = new AzureStorageQueue<CreateResourceData>(new AzureStorageQueueOptions<CreateResourceData>()
        {
            ConnectionString = config["ProjectCreationQueue:ConnectionString"],
            Name = config["ProjectCreationQueue:Name"],
        });
        //test queue success
        var project = await queue.DequeueAsync();
        Assert.Equal(projectName, project.Value.Workspace.Name);
        Assert.Equal(organization, project.Value.Workspace.TerraformOrganization.Name);
    }

    private IConfiguration Configuration
    {
        get
        {
            if (_config != null) return _config;
            var builder = new ConfigurationBuilder().AddJsonFile($"testsettings.json", optional: false);
            _config = builder.Build();

            return _config;
        }
    }
}