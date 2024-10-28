using System.Text.Json;
using Azure;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Resources.Mocking;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Cost;
using Datahub.Application.Services.ResourceGroups;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Subscriptions;
using Datahub.Infrastructure.Services.ResourceGroups;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.Extensions;
using Reqnroll;
using Reqnroll.BoDi;

namespace Datahub.SpecflowTests.Hooks
{
    [Binding]
    public class WorkspaceResourceGroupsHook
    {
        [BeforeScenario("WorkspaceResourceGroups")]
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
            var armClient = Substitute.For<ArmClient>(); 
            var logger = Substitute.For<ILogger<WorkspaceResourceGroupsManagementService>>();
            var config = Substitute.For<IConfiguration>();
            MockArmMethods(armClient);

            var workspaceRgManagementService =
                new WorkspaceResourceGroupsManagementService(armClient, logger, dbContextFactory, config);

            config["DataHub_ENVNAME"].Returns("test");

            objectContainer.RegisterInstanceAs<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);
            objectContainer.RegisterInstanceAs<IWorkspaceResourceGroupsManagementService>(workspaceRgManagementService);
            objectContainer.RegisterInstanceAs(datahubPortalConfiguration);

            SeedDb(dbContextFactory);
        }

        [AfterScenario("WorkspaceResourceGroups")]
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
                JsonContent = "{}",
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

        private async Task MockArmMethods(ArmClient armClient)
        {
            var rg1 = Substitute.For<ResourceGroupResource>();
            var rg1Id = new ResourceIdentifier($"/subscriptions/{Testing.WorkspaceSubscriptionGuid}/resourceGroups/{Testing.ResourceGroupName1}");
            var rg1Data = ResourceManagerModelFactory.ResourceGroupData(name : Testing.ResourceGroupName1);
            var rg2 = Substitute.For<ResourceGroupResource>();
            var rg2Id = new ResourceIdentifier($"/subscriptions/{Testing.WorkspaceSubscriptionGuid}/resourceGroups/{Testing.ResourceGroupName2}");
            var rg2Data = ResourceManagerModelFactory.ResourceGroupData(name : Testing.ResourceGroupName2);
            rg1.HasData.Returns(true);
            rg1.Data.Returns(rg1Data);
            rg1.Id.Returns(rg1Id);
            rg2.HasData.Returns(true);
            rg2.Data.Returns(rg2Data);
            rg2.Id.Returns(rg2Id);

            var sub = Substitute.For<SubscriptionResource>();
            var rgCollection = Substitute.For<ResourceGroupCollection>();
            var asyncPageable = Substitute.For<AsyncPageable<ResourceGroupResource>>();
            var page = Page<ResourceGroupResource>.FromValues(new[] { rg1, rg2 }, null, Substitute.For<Response>());
            var asyncEnumeratorPage = (new List<Page<ResourceGroupResource>> { page }).ToAsyncEnumerable();
            asyncPageable.AsPages().Returns(asyncEnumeratorPage);
            rgCollection.GetAllAsync().Returns(asyncPageable);
            sub.GetResourceGroups().Returns(rgCollection);
            armClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{Testing.WorkspaceSubscriptionGuid}")).Returns(sub);
            armClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{Testing.InvalidSubscriptionId}"))
                .Throws(new Exception());
        }
    }
}