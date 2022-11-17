using System.Collections.Generic;
using System.Linq;
using Xunit;
using System;
using System.Collections;
using System.Threading.Tasks;
using Datahub.Core.EFCore;
using Datahub.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

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
        services.AddPooledDbContextFactory<DatahubProjectDBContext>(options => options.UseInMemoryDatabase("datahubProjects"));
        services.AddScoped<IProjectCreationService, ProjectCreationService>();
        
        //dependency for ProjectCreationService
        services.AddSingleton(Configuration);
        services.AddScoped<ITokenAcquisition, MockTokenAcquisitionService>();
        services.AddHttpClient<IProjectCreationService, ProjectCreationService>(client =>
        {
            client.BaseAddress = new Uri(ResourceProvisionerUrl);
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
        });      
        services.AddScoped<IUserInformationService, OfflineUserInformationService>();
        return services.BuildServiceProvider();
    }
    
    [Fact]
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


    [Fact]
    public async Task GivenDatahubProjectWithoutAcronym_CreateResourcesAndAddProject()
    {
        var projectCreationService =
            SetupServices().GetRequiredService<IProjectCreationService>();
        var resourceProvisionerTriggered = await projectCreationService.CreateProjectAsync(projectName: "Datahub Unit Testing", organization: "Unit Testing");
        var projects = LoadCollectionGeneric<DatahubProjectDBContext, Datahub_Project>(SetupServices(), d => d.Projects);
        Assert.Single(projects);
        Assert.True(resourceProvisionerTriggered);
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