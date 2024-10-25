using System.Text.Json;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.CostManagement;
using Azure.ResourceManager.CostManagement.Models;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Cost;
using Datahub.Application.Services.ResourceGroups;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Subscriptions;
using Datahub.Infrastructure.Services.Cost;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Reqnroll;
using Reqnroll.BoDi;

namespace Datahub.SpecflowTests.Hooks
{
    [Binding]
    public class WorkspaceCostsHook
    {
        [BeforeScenario("WorkspaceCosts")]
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

            var logger = Substitute.For<ILogger<WorkspaceCostManagementService>>();

            var workspaceRgManagementService = Substitute.For<IWorkspaceResourceGroupsManagementService>();
            var workspaceCostsManagementService = new WorkspaceCostManagementService(armClient, logger,
                dbContextFactory, workspaceRgManagementService);

            var mockRgId1 = new ResourceIdentifier(
                $"/subscriptions/{Testing.WorkspaceSubscriptionGuid}/resourceGroups/{Testing.ResourceGroupName1}");
            var mockRgId2 = new ResourceIdentifier(
                $"/subscriptions/{Testing.WorkspaceSubscriptionGuid2}/resourceGroups/{Testing.ResourceGroupName2}");


            workspaceRgManagementService
                .GetWorkspaceResourceGroupsAsync(Testing.WorkspaceAcronym)
                .Returns(new List<string> { Testing.ResourceGroupName1 });
            workspaceRgManagementService
                .GetWorkspaceResourceGroupsAsync(Testing.WorkspaceAcronym2)
                .Returns(new List<string> { Testing.ResourceGroupName2 });
            workspaceRgManagementService
                .GetAllSubscriptionResourceGroupsAsync(Testing.WorkspaceSubscriptionGuid)
                .Returns(new List<string> { Testing.ResourceGroupName1 });
            workspaceRgManagementService
                .GetAllResourceGroupsAsync()
                .Returns(new List<string> { Testing.ResourceGroupName1 });
            workspaceRgManagementService
                .GetWorkspaceResourceGroupsIdentifiersAsync(Testing.WorkspaceAcronym)
                .Returns(new List<ResourceIdentifier>
                {
                    mockRgId1
                });
            workspaceRgManagementService
                .GetWorkspaceResourceGroupsIdentifiersAsync(Testing.WorkspaceAcronym2)
                .Returns(new List<ResourceIdentifier>
                {
                    mockRgId2
                });

            var mockGranularQuery =
                workspaceCostsManagementService.BuildGranularQueryDefinition(
                    new List<string> { Testing.ResourceGroupName1 },
                    Testing.Dates.First(), Testing.Dates.Last());
            var mockTotalQuery =
                workspaceCostsManagementService.BuildTotalsQueryDefinition(
                    new List<string> { Testing.ResourceGroupName1 },
                    Testing.Dates.First(), Testing.Dates.Last());
            armClient
                .UsageQueryAsync(mockRgId1, mockGranularQuery)
                .ReturnsForAnyArgs(c => c.ArgAt<QueryDefinition>(1) switch
                {
                    { Dataset: { } dataset } when dataset.Granularity == GranularityType.Daily => GranularQuery(),
                    _ => TotalQuery()
                });;

            objectContainer.RegisterInstanceAs<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);
            objectContainer.RegisterInstanceAs(workspaceRgManagementService);
            objectContainer.RegisterInstanceAs<IWorkspaceCostManagementService>(workspaceCostsManagementService);
            objectContainer.RegisterInstanceAs(datahubPortalConfiguration);

            SeedDb(dbContextFactory);
        }

        [AfterScenario("WorkspaceCosts")]
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

        private Response<QueryResult> GranularQuery()
        {
            var queryColumns = new List<QueryColumn>
            {
                BuildQueryColumn("ResourceGroupName"),
                BuildQueryColumn("Cost"),
                BuildQueryColumn("UsageDate"),
                BuildQueryColumn("ServiceName"),
            };
            var queryRows = new List<IList<BinaryData>>
            {
                BuildQueryRow([
                    Testing.ResourceGroupName1, "100", Testing.Dates.First().ToString("yyyyMMdd"), "Service1"
                ]),
                BuildQueryRow([
                    Testing.ResourceGroupName1, "200", Testing.Dates.First().ToString("yyyyMMdd"), "Service2"
                ]),
                BuildQueryRow([
                    Testing.ResourceGroupName1, "300", Testing.Dates.First().ToString("yyyyMMdd"), "Service3"
                ]),
                BuildQueryRow([
                    Testing.ResourceGroupName1, "400", Testing.Dates.First().ToString("yyyyMMdd"), "Service4"
                ]),
                BuildQueryRow([
                    Testing.ResourceGroupName1, "500", Testing.Dates.First().ToString("yyyyMMdd"), "Service5"
                ]),
            };
            var queryResult = ArmCostManagementModelFactory.QueryResult(columns: queryColumns, rows: queryRows);

            return Response.FromValue(queryResult, Substitute.For<Response>());
        }

        private QueryColumn BuildQueryColumn(string columnName)
        {
            return ArmCostManagementModelFactory.QueryColumn(columnName, "Dimension");
        }

        private IList<BinaryData> BuildQueryRow(List<string> values)
        {
            return values.Select(BinaryData.FromString).ToList();
        }

        private Response<QueryResult> TotalQuery()
        {
            var queryColumns = new List<QueryColumn>
            {
                BuildQueryColumn("ResourceGroupName"),
                BuildQueryColumn("Cost"),
            };
            var queryRows = new List<IList<BinaryData>>
            {
                BuildQueryRow([Testing.ResourceGroupName1, "100"]),
                BuildQueryRow([Testing.ResourceGroupName2, "200"]),
            };
            var queryResult = ArmCostManagementModelFactory.QueryResult(columns: queryColumns, rows: queryRows);

            return Response.FromValue(queryResult, Substitute.For<Response>());
        }
    }
}