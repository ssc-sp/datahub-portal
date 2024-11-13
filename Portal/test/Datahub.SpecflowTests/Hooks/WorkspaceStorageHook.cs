using System.Text.Json;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Query.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Monitor;
using Azure.ResourceManager.Monitor.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Resources.Mocking;
using Azure.ResourceManager.Resources.Models;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Mocking;
using Azure.ResourceManager.Storage.Models;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Cost;
using Datahub.Application.Services.ResourceGroups;
using Datahub.Application.Services.Storage;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Subscriptions;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Extensions;
using Reqnroll;
using Reqnroll.BoDi;

namespace Datahub.SpecflowTests.Hooks
{
    [Binding]
    public class WorkspaceStorageHook
    {
        [BeforeScenario("WorkspaceStorage")]
        public async Task BeforeScenarioWorkspaceCosts(IObjectContainer objectContainer,
            ScenarioContext scenarioContext)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets<Hooks>()
                .AddJsonFile("appsettings.test.json", optional: true)
                .Build();

            var datahubPortalConfiguration = new DatahubPortalConfiguration();
            configuration.Bind(datahubPortalConfiguration);

            var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var dbContextFactory = new SpecFlowDbContextFactory(options);
            var armClient = Substitute.For<ArmClient>();
            var logger = Substitute.For<ILogger<WorkspaceStorageManagementService>>();

            MockServiceCalls(armClient, logger, dbContextFactory, datahubPortalConfiguration, objectContainer);
            await MockArmMethods(armClient);
            SeedDb(dbContextFactory);
        }

        [AfterScenario("WorkspaceStorage")]
        public void AfterScenarioWorkspaceCosts(IObjectContainer objectContainer)
        {
            var dbContextFactory = objectContainer.Resolve<IDbContextFactory<DatahubProjectDBContext>>();
            using var context = dbContextFactory.CreateDbContext();
            context.Database.EnsureDeleted();
        }

        /* Currently unused
        [BeforeScenario("WorkspaceStorageMockArm")]
        public void BeforeScenarioWorkspaceCostsMockArm(IObjectContainer objectContainer,
            ScenarioContext scenarioContext)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets<Hooks>()
                .AddJsonFile("appsettings.test.json", optional: true)
                .Build();

            var datahubPortalConfiguration = new DatahubPortalConfiguration();
            configuration.Bind(datahubPortalConfiguration);

            var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var dbContextFactory = new SpecFlowDbContextFactory(options);
            var armClient = Substitute.For<ArmClient>();
            var logger = Substitute.For<ILogger<WorkspaceStorageManagementService>>();


            MockServiceCalls(armClient, logger, dbContextFactory, datahubPortalConfiguration, objectContainer);
            SeedDb(dbContextFactory);
        }

        [AfterScenario("WorkspaceStorageMockArm")]
        public void AfterScenarioWorkspaceCostsMockArm(IObjectContainer objectContainer)
        {
            var dbContextFactory = objectContainer.Resolve<IDbContextFactory<DatahubProjectDBContext>>();
            using var context = dbContextFactory.CreateDbContext();
            context.Database.EnsureDeleted();
        }
        */

        private void SeedDb(IDbContextFactory<DatahubProjectDBContext> contextFactory)
        {
            using var context = contextFactory.CreateDbContext();

            var sub = new DatahubAzureSubscription
            {
                TenantId = Testing.WorkspaceTenantGuid,
                SubscriptionId = Testing.WorkspaceSubscriptionGuid,
                SubscriptionName = Testing.SubscriptionName,
            };

            var projects = new List<Datahub_Project>
            {
                new()
                {
                    Project_Name = Testing.WorkspaceName,
                    Project_Acronym_CD = Testing.WorkspaceAcronym,
                    DatahubAzureSubscription = sub
                },
                new()
                {
                    Project_Name = Testing.WorkspaceName2,
                    Project_Acronym_CD = Testing.WorkspaceAcronym2,
                    DatahubAzureSubscription = sub
                }
            };

            context.AzureSubscriptions.Add(sub);
            context.Projects.AddRange(projects);
            context.SaveChanges();

            var project1 = projects.First(p => p.Project_Acronym_CD == Testing.WorkspaceAcronym);
            var project2 = projects.First(p => p.Project_Acronym_CD == Testing.WorkspaceAcronym2);
            var resourceTypes = new List<string>
            {
                TerraformTemplate.GetTerraformServiceType(TerraformTemplate.NewProjectTemplate),
                TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureStorageBlob),
                TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureDatabricks),
            };
            var projectResources = new List<Project_Resources2>();
            foreach (var resource in resourceTypes)
            {
                projectResources.Add(new Project_Resources2
                {
                    ProjectId = project1.Project_ID,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    JsonContent = $"{{\"resource_group_name\": \"{Testing.ResourceGroupName1}\"}}",
                    ResourceType = resource
                });
            }

            projectResources.Add(new()
            {
                ProjectId = project2.Project_ID,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                JsonContent = "{{}}",
                ResourceType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.NewProjectTemplate)
            });
            context.Project_Resources2.AddRange(projectResources);
            context.SaveChanges();


            var random = new Random();


            var costs = new List<Datahub_Project_Costs>();
            foreach (var date in Testing.Dates)
            {
                foreach (var service in Testing.ServiceNames)
                {
                    costs.Add(new()
                    {
                        Project_ID = project1.Project_ID,
                        ServiceName = service,
                        Date = date.Date,
                        CadCost = random.Next(1, 1000),
                    });
                }
            }

            context.Project_Costs.AddRange(costs);
            context.SaveChanges();

            var costs1 = context.Project_Costs.Where(c => c.Project_ID == project1.Project_ID).ToList();
            var dailyCosts = costs1.Select(c => new DailyServiceCost
            {
                Amount = (decimal)c.CadCost, Date = c.Date, Source = c.ServiceName, ResourceGroupName = string.Empty
            }).ToList();
            var currentBySource = dailyCosts.GroupBySource();
            var currentByDate = dailyCosts.GroupByDate();
            var current = dailyCosts.TotalAmount();
            var yesterdayCosts = dailyCosts.FilterDateRange(DateTime.UtcNow.Date.AddDays(-1));
            var yesterdayBySource = yesterdayCosts.GroupBySource();
            var yesterday = yesterdayCosts.TotalAmount();

            var projectCredit = new Project_Credits
            {
                ProjectId = project1.Project_ID,
                Current = (double)current,
                CurrentPerDay = JsonSerializer.Serialize(currentByDate),
                CurrentPerService = JsonSerializer.Serialize(currentBySource),
                YesterdayCredits = (double)yesterday,
                YesterdayPerService = JsonSerializer.Serialize(yesterdayBySource),
                LastUpdate = DateTime.UtcNow.AddHours(-6),
            };
            context.Project_Credits.Add(projectCredit);
            context.SaveChanges();

            var projectAverage = new Project_Storage
            {
                AverageCapacity = 10,
                CloudProvider = "azure",
                Date = Testing.Dates.First(),
                ProjectId = project1.Project_ID,
            };
            context.Project_Storage_Avgs.Add(projectAverage);
            context.SaveChanges();
        }

        public void MockServiceCalls(ArmClient armClient, ILogger<WorkspaceStorageManagementService> logger,
            IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
            DatahubPortalConfiguration datahubPortalConfiguration, IObjectContainer objectContainer)
        {
            var workspaceRgManagementService = Substitute.For<IWorkspaceResourceGroupsManagementService>();
            var workspaceStorageManagementService = new WorkspaceStorageManagementService(armClient, logger,
                dbContextFactory, workspaceRgManagementService);


            workspaceRgManagementService
                .GetWorkspaceResourceGroupsAsync(Testing.WorkspaceAcronym)
                .Returns(new List<string> { Testing.ResourceGroupName1 });
            workspaceRgManagementService
                .GetWorkspaceResourceGroupsAsync(Testing.WorkspaceAcronym2)
                .Returns(new List<string> { Testing.ResourceGroupName2 });
            workspaceRgManagementService
                .GetAllSubscriptionResourceGroupsAsync(datahubPortalConfiguration.AzureAd.SubscriptionId)
                .Returns(new List<string> { Testing.ResourceGroupName1, Testing.ResourceGroupName2 });
            workspaceRgManagementService
                .GetAllResourceGroupsAsync()
                .Returns(new List<string> { Testing.ResourceGroupName1, Testing.ResourceGroupName2 });
            workspaceRgManagementService
                .GetWorkspaceResourceGroupsIdentifiersAsync(Testing.WorkspaceAcronym)
                .Returns(new List<ResourceIdentifier>
                {
                    new ResourceIdentifier(
                        $"/subscriptions/{Testing.WorkspaceSubscriptionGuid}/resourceGroups/{Testing.ResourceGroupName1}")
                });
            workspaceRgManagementService
                .GetWorkspaceResourceGroupsIdentifiersAsync(Testing.WorkspaceAcronym2)
                .Returns(new List<ResourceIdentifier>
                {
                    new ResourceIdentifier(
                        $"/subscriptions/{Testing.WorkspaceSubscriptionGuid}/resourceGroups/{Testing.ResourceGroupName2}")
                });

            objectContainer.RegisterInstanceAs<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);
            objectContainer.RegisterInstanceAs(workspaceRgManagementService);
            objectContainer.RegisterInstanceAs<IWorkspaceStorageManagementService>(workspaceStorageManagementService);
            objectContainer.RegisterInstanceAs(datahubPortalConfiguration);
        }

        public async Task MockArmMethods(ArmClient armClient)
        {
            var timeSeriesData = ArmMonitorModelFactory.MonitorMetricValue(average: 10.0);
            var timeSeries = ArmMonitorModelFactory.MonitorTimeSeriesElement(data: new[] { timeSeriesData });
            var monitorMetric = ArmMonitorModelFactory.MonitorMetric(timeseries: new[] { timeSeries });
            var monitorMetricPage =
                Page<MonitorMetric>.FromValues(new[] { monitorMetric }, null, Substitute.For<Response>());
            var pageableMetrics = Pageable<MonitorMetric>.FromPages(new[] { monitorMetricPage });
            var storage1Id =
                new ResourceIdentifier(
                    $"/subscriptions/{Testing.WorkspaceSubscriptionGuid}/resourceGroups/{Testing.ResourceGroupName1}/providers/Microsoft.Storage/storageAccounts/{Testing.StorageAccountId}");

            var rg1 = Substitute.For<ResourceGroupResource>();
            var rg1Id = new ResourceIdentifier(
                $"/subscriptions/{Testing.WorkspaceSubscriptionGuid}/resourceGroups/{Testing.ResourceGroupName1}");
            var storage1 = Substitute.For<StorageAccountResource>();
            var storage1Data = ArmStorageModelFactory.StorageAccountData(id: storage1Id);
            var storage1Collection = Substitute.For<StorageAccountCollection>();

            storage1.Data.Returns(storage1Data);
            storage1.Id.Returns(storage1Id);
            rg1.Id.Returns(rg1Id);
            var storage1Page =
                Page<StorageAccountResource>.FromValues(new[] { storage1 }, null, Substitute.For<Response>());
            var storage1Pageable = Pageable<StorageAccountResource>.FromPages(new[] { storage1Page });
            storage1Collection.GetAll().Returns(storage1Pageable);
            armClient.GetResourceGroupResource(rg1Id).Returns((ResourceGroupResource)rg1);
            rg1.GetStorageAccounts().Returns(storage1Collection);
            var rg2 = Substitute.For<ResourceGroupResource>();
            var rg2Id = new ResourceIdentifier(
                $"/subscriptions/{Testing.WorkspaceSubscriptionGuid}/resourceGroups/{Testing.ResourceGroupName2}");
            var rg2Data = ResourceManagerModelFactory.ResourceGroupData(id: rg2Id);
            var storage2 = Substitute.For<StorageAccountResource>();
            var storage2Id =
                new ResourceIdentifier(
                    $"/subscriptions/{Testing.WorkspaceSubscriptionGuid}/resourceGroups/{Testing.ResourceGroupName2}/providers/Microsoft.Storage/storageAccounts/{Testing.StorageAccountId}");
            var storage2Data = ArmStorageModelFactory.StorageAccountData(id: storage2Id);
            var storage2Collection = Substitute.For<StorageAccountCollection>();
            storage2.Data.Returns(storage2Data);
            storage2.Id.Returns(storage2Id);
            rg2.Id.Returns(rg2Id);
            rg2.HasData.Returns(true);
            rg2.Data.Returns(rg2Data);
            var storage2Page =
                Page<StorageAccountResource>.FromValues(new[] { storage2 }, null, Substitute.For<Response>());
            var storage2Pageable = Pageable<StorageAccountResource>.FromPages(new[] { storage2Page });
            storage2Collection.GetAll().Returns(storage2Pageable);

            var today = DateTime.UtcNow;
            var dateFormat = "yyyy-MM-ddTHH:00:00.000Z";
            var fromDate = today.AddDays(-1).ToString(dateFormat);
            var toDate = today.ToString(dateFormat);
            var option = new ArmResourceGetMonitorMetricsOptions();
            option.Metricnames = "UsedCapacity";
            option.Aggregation = "average";
            option.Timespan = $"{fromDate}/{toDate}";
            option.Metricnamespace = "Microsoft.Storage/storageAccounts";
            option.ValidateDimensions = false;

            rg1.GetAsync()
                .Returns(Task.FromResult(Response.FromValue(rg1, Substitute.For<Response>())));
            rg2.GetAsync()
                .Returns(Task.FromResult(Response.FromValue(rg2, Substitute.For<Response>())));
            rg1.Configure().GetStorageAccounts().Returns(storage1Collection);
            rg2.Configure().GetStorageAccounts().Returns(storage2Collection);
            armClient.GetMonitorMetrics(storage1Id, option)
                .Returns(pageableMetrics);
            armClient.GetMonitorMetrics(storage2Id, option)
                .Returns(pageableMetrics);
            armClient.GetResourceGroupResource(rg1Id).Returns(rg1);
            armClient.GetResourceGroupResource(rg2Id).Returns(rg2);
        }
    }
}