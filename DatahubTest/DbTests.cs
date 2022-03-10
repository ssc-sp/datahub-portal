using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Datahub.Core.EFCore;
using Datahub.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Datahub.Core.UserTracking;
using Microsoft.Extensions.Configuration;
using System.Threading;

namespace Datahub.Tests
{
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

        [Fact]
        public async void GivenCosmosCxnStr_ValidateConnection()
        {
            LoadServices();

            using (var scope = _serviceProvider.CreateScope())
            {


                var myDataService = scope.ServiceProvider.GetService<UserLocationManagerService>();
                var userRecentActions = await myDataService.ReadRecentNavigations(_userId);

                if (userRecentActions != null)
                {
                    await myDataService.DeleteUserRecent(_userId);
                }

                userRecentActions = await myDataService.ReadRecentNavigations(_userId);
                Assert.True(userRecentActions == null);

                var userRecentActions1 = new UserRecentLink() { UserRecentActionId = Guid.NewGuid(), DataProject = "ABC", DatabricksURL = "https://databricks", accessedTime = DateTimeOffset.Now };
                var userRecentActions2 = new UserRecentLink() { UserRecentActionId = Guid.NewGuid(), DataProject = "ABC", DatabricksURL = "https://databricks", accessedTime = DateTimeOffset.Now };
                var userRecentActions3 = new UserRecentLink() { UserRecentActionId = Guid.NewGuid(), DataProject = "ABC", DatabricksURL = "https://databricks", accessedTime = DateTimeOffset.Now };
                var userRecentActions4 = new UserRecentLink() { UserRecentActionId = Guid.NewGuid(), DataProject = "ABC", DatabricksURL = "https://databricks", accessedTime = DateTimeOffset.Now };
                var userRecentActions5 = new UserRecentLink() { UserRecentActionId = Guid.NewGuid(), DataProject = "ABC", DatabricksURL = "https://databricks", accessedTime = DateTimeOffset.Now };
                var userRecent = new UserRecent() { UserId = _userId };
                userRecent.UserRecentActions.Add(userRecentActions1);
                userRecent.UserRecentActions.Add(userRecentActions2);
                userRecent.UserRecentActions.Add(userRecentActions3);
                userRecent.UserRecentActions.Add(userRecentActions4);
                userRecent.UserRecentActions.Add(userRecentActions5);


                await myDataService.RegisterNavigation(userRecent);
                userRecentActions = await myDataService.ReadRecentNavigations(_userId);

                Assert.True(userRecentActions.UserRecentActions.Count == 5);
            }
        }


       

        private void LoadServices()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDbContext<UserTrackingContext>(options =>
            {

                options.UseCosmos(
                   _cosmosCxnStr,
                    databaseName: "datahub-catalog-db"
               );
            });

            serviceCollection.AddSingleton(Configuration);
            serviceCollection.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
            });
            serviceCollection.AddScoped<UserLocationManagerService>();
            serviceCollection.AddSingleton<IKeyVaultService, KeyVaultService>();
            serviceCollection.AddScoped<UserLocationManagerService>();
            serviceCollection.AddSingleton<CommonAzureServices>();
            serviceCollection.AddScoped<DataLakeClientService>();
            serviceCollection.AddHttpClient();
            serviceCollection.AddScoped<IUserInformationService, UserInformationService>();
            serviceCollection.AddSingleton<IMSGraphService, MSGraphService>();

            serviceCollection.AddScoped<ApiService>();
            serviceCollection.AddScoped<IApiCallService, ApiCallService>();


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
}
