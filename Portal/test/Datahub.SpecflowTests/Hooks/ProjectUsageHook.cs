using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.ResourceManager;
using Azure.Storage.Blobs;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Cost;
using Datahub.Application.Services.ResourceGroups;
using Datahub.Application.Services.Storage;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Subscriptions;
using Datahub.Functions;
using Datahub.Functions.Services;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services.Cost;
using Datahub.Shared.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Reqnroll;
using Reqnroll.BoDi;

namespace Datahub.SpecflowTests.Hooks
{
    [Binding]
    public class ProjectUsageHook
    {
        [BeforeScenario("ProjectUsageNotifier")]
        public void BeforeScenarioProjectUsageNotifier(IObjectContainer objectContainer,
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
            
            var azureConfig = new AzureConfig(configuration);
            var resourceMessagingService = Substitute.For<IResourceMessagingService>();
            
            var sendEndpointProvider = Substitute.For<ISendEndpointProvider>();
            var emailService = Substitute.For<IEmailService>();

            objectContainer.RegisterInstanceAs<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);
            objectContainer.RegisterInstanceAs(azureConfig);
            objectContainer.RegisterInstanceAs(resourceMessagingService);
            objectContainer.RegisterInstanceAs(sendEndpointProvider);
            objectContainer.RegisterInstanceAs(emailService);
        }
        
        [BeforeScenario("ProjectUsage")]
        public void BeforeScenarioWorkspaceCosts(IObjectContainer objectContainer,
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

            var loggerFactory = new LoggerFactory();

            var workspaceCostsManagementService = Substitute.For<IWorkspaceCostManagementService>();
            var workspaceBudgetManagementService = Substitute.For<IWorkspaceBudgetManagementService>();
            var workspaceStorageManagementService = Substitute.For<IWorkspaceStorageManagementService>();
            var workspaceRgManagementService = Substitute.For<IWorkspaceResourceGroupsManagementService>();
            var sendEndpointProvider = Substitute.For<ISendEndpointProvider>();

            workspaceCostsManagementService
                .UpdateWorkspaceCostsAsync(Testing.WorkspaceAcronym, Arg.Any<List<DailyServiceCost>>())
                .Returns((false, 0));
            workspaceCostsManagementService
                .UpdateWorkspaceCostsAsync(Testing.WorkspaceAcronym2, Arg.Any<List<DailyServiceCost>>())
                .Returns((true, 10));
            workspaceCostsManagementService
                .VerifyAndRefreshWorkspaceCostsAsync(Testing.WorkspaceAcronym, Arg.Any<List<DailyServiceCost>>())
                .Returns(false);
            workspaceCostsManagementService
                .VerifyAndRefreshWorkspaceCostsAsync(Testing.WorkspaceAcronym2, Arg.Any<List<DailyServiceCost>>())
                .Returns(true);
            workspaceCostsManagementService
                .QuerySubscriptionCostsAsync(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(),
                    QueryGranularity.Daily, rgNames: Arg.Any<List<string>>())
                .Returns(new List<DailyServiceCost>
                {
                    new()
                    {
                        Amount = 10, Date = Testing.Dates.First(), Source = Testing.ServiceNames.First(),
                        ResourceGroupName = Testing.ResourceGroupName1
                    }
                });
            workspaceCostsManagementService
                .QuerySubscriptionCostsAsync(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(),
                    QueryGranularity.Total, rgNames: Arg.Any<List<string>>())
                .Returns(new List<DailyServiceCost>
                {
                    new()
                    {
                        Amount = 100,
                        ResourceGroupName = Testing.ResourceGroupName1
                    }
                });
            workspaceCostsManagementService
                .CheckUpdateNeeded(Testing.WorkspaceAcronym)
                .Returns(true);
            workspaceCostsManagementService
                .CheckUpdateNeeded(Testing.WorkspaceAcronym2)
                .Returns(false);
            workspaceBudgetManagementService
                .GetWorkspaceBudgetAmountAsync(Arg.Any<string>())
                .Returns(100);
            workspaceBudgetManagementService
                .SetWorkspaceBudgetAmountAsync(Arg.Any<string>(), Arg.Any<decimal>(), true)
                .Returns(Task.CompletedTask);
            workspaceStorageManagementService
                .UpdateStorageCapacity(Arg.Any<string>())
                .Returns(100.0);
            workspaceStorageManagementService
                .CheckUpdateNeeded(Testing.WorkspaceAcronym)
                .Returns(true);
            workspaceStorageManagementService
                .CheckUpdateNeeded(Testing.WorkspaceAcronym2)
                .Returns(false);
            workspaceRgManagementService
                .GetAllSubscriptionResourceGroupsAsync(Arg.Any<string>())
                .Returns(new List<string> { Testing.ResourceGroupName1 });

            var projectUsageScheduler = new ProjectUsageScheduler(loggerFactory, dbContextFactory, sendEndpointProvider,
                workspaceCostsManagementService, workspaceStorageManagementService, workspaceRgManagementService,
                configuration);
            projectUsageScheduler.Mock = true;
            var projectUsageUpdater = new ProjectUsageUpdater(loggerFactory, workspaceCostsManagementService,
                workspaceBudgetManagementService, workspaceStorageManagementService, sendEndpointProvider,
                configuration);
            projectUsageUpdater.Mock = true;
            projectUsageUpdater.MockCosts = new List<DailyServiceCost>
            {
                new()
                {
                    Amount = 10, Date = Testing.Dates.First(), Source = Testing.ServiceNames.First(),
                    ResourceGroupName = Testing.ResourceGroupName1
                }
            };

            objectContainer.RegisterInstanceAs<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);
            objectContainer.RegisterInstanceAs(datahubPortalConfiguration);
            objectContainer.RegisterInstanceAs(projectUsageScheduler);
            objectContainer.RegisterInstanceAs(projectUsageUpdater);

            SeedDb(dbContextFactory);
        }

        [AfterScenario("ProjectUsage")]
        public void AfterScenarioWorkspaceCosts(IObjectContainer objectContainer)
        {
            var dbContextFactory = objectContainer.Resolve<IDbContextFactory<DatahubProjectDBContext>>();
            using var context = dbContextFactory.CreateDbContext();
            context.Database.EnsureDeleted();
        }

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
        }
    }
}