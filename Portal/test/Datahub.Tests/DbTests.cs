using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Datahub.Core.Services;
using System;
using Xunit;
using Microsoft.Extensions.Configuration;
using System.Threading;
using Datahub.Core.Model.UserTracking;
using Datahub.Core.Services.Api;
using Datahub.Core.Services.UserManagement;
using Datahub.Core.Services.Security;
using Datahub.Core.Services.Storage;

namespace Datahub.Tests;

public class DbTests
{
    private ServiceProvider _serviceProvider;
    private IConfiguration _config;
    private string _cosmosCxnStr = @"AccountEndpoint=https://dh-portal-cosmosdb-dev.documents.azure.com:443/;AccountKey=QAwclaaNFK2G5foH4g9NqGa2xBJfLS46n53LW3LKOqYMiGxBI4J9H3sOSSAwx9ZI7YHqPQzc5w3QZD29vSZBDg==;";
    private string _userId = "myuserid";


    [Fact]
    public async void GraphServiceTest()
    {
        LoadServices();

        using (var scope = _serviceProvider.CreateScope())
        {
                
            var myGraphService = scope.ServiceProvider.GetRequiredService<IMSGraphService>();
            //var graphService = scope.ServiceProvider.
            var user = await myGraphService.GetUserIdFromEmailAsync("alexander.khavich@nrcan-rncan.gc.ca", CancellationToken.None);

            Assert.True(user != null);
        }
    }

    [Fact]
    public async void GraphServiceTest_GetUser()
    {
        LoadServices();

        using (var scope = _serviceProvider.CreateScope())
        {

            var myGraphService = scope.ServiceProvider.GetRequiredService<IMSGraphService>();
            //var graphService = scope.ServiceProvider.
            var user = await myGraphService.GetUserAsync("0403528c-5abc-423f-9201-9c945f628595", CancellationToken.None);
                
            Assert.True(user != null);
        }
    }


    [Fact]
    public async void GraphServiceTest_GetUsers()
    {
        LoadServices();

        using (var scope = _serviceProvider.CreateScope())
        {

            var myGraphService = scope.ServiceProvider.GetRequiredService<IMSGraphService>();
                
            var user = await myGraphService.GetUsersListAsync("erik", CancellationToken.None);

            Assert.True(user != null);
        }
    }
     
    private void LoadServices()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(Configuration);
        serviceCollection.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
        });
        serviceCollection.AddScoped<UserLocationManagerService>();
        serviceCollection.AddSingleton<IKeyVaultService, KeyVaultCoreService>();
        serviceCollection.AddScoped<UserLocationManagerService>();
        serviceCollection.AddSingleton<CommonAzureServices>();
        serviceCollection.AddScoped<DataLakeClientService>();
        serviceCollection.AddHttpClient();
        serviceCollection.AddScoped<IUserInformationService, UserInformationService>();
        serviceCollection.AddSingleton<IMSGraphService, MSGraphService>();


        _serviceProvider = serviceCollection.BuildServiceProvider();
    }


    public IConfiguration Configuration
    {
        get
        {
            if (_config == null)
            {
                var builder = new ConfigurationBuilder().AddJsonFile($"testsettings.json", optional: false);
                _config = builder.Build();
            }

            return _config;
        }
    }

    //Task InitializeAsync()
    //{


    //    return Task.CompletedTask;
    //}
}