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

public class WorkspaceVersionTests
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
        services.AddScoped<IWorkspaceVersionService, WorkspaceVersionService>();
        
        //dependency for ProjectCreationService
        services.AddSingleton(Configuration);
        
        services.AddScoped<IUserInformationService, OfflineUserInformationService>();
        return services.BuildServiceProvider();
    }
    
    [Fact]
    public async Task GivenListOfVersionTagsThenGetLatestVersion()
    {
        var versions = new List<string> { "1.4", "8.3.6", "2.0.1", "10.3.4", "4.13.6", "3.0.1" };

        var latest = versions
            .Select(v => Version.Parse(v))
            .OrderByDescending(v => v)
            .First();

        Assert.True(latest.ToString() == "10.3.4", $"Latest version should be 10.3.4 but was {latest}");

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