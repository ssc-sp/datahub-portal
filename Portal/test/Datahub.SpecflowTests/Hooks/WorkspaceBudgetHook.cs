using System.Text.Json;
using Azure;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Billing.Models;
using Azure.ResourceManager.Consumption;
using Azure.ResourceManager.Consumption.Models;
using Azure.ResourceManager.CostManagement.Models;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Monitor.Models;
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
    public class WorkspaceBudgetHook
    {
        [BeforeScenario("WorkspaceBudget")]
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
            var logger = Substitute.For<ILogger<WorkspaceBudgetManagementService>>();

            var workspaceRgManagementService = Substitute.For<IWorkspaceResourceGroupsManagementService>();
            var workspaceBudgetManagementService = new WorkspaceBudgetManagementService(armClient, logger,
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
                .Returns(new List<string> { Testing.ResourceGroupName1 });
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
            armClient
                .GetConsumptionBudgetResource(mockRgId1)
                .Returns(MockBudget(armClient));
            objectContainer.RegisterInstanceAs<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);
            objectContainer.RegisterInstanceAs(workspaceRgManagementService);
            objectContainer.RegisterInstanceAs<IWorkspaceBudgetManagementService>(workspaceBudgetManagementService);
            objectContainer.RegisterInstanceAs(datahubPortalConfiguration);

            SeedDb(dbContextFactory);
        }

        [AfterScenario("WorkspaceBudget")]
        public async Task AfterScenarioWorkspaceCosts(IObjectContainer objectContainer)
        {
            var dbContextFactory = objectContainer.Resolve<IDbContextFactory<DatahubProjectDBContext>>();
            using var context = await dbContextFactory.CreateDbContextAsync();
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

        private ConsumptionBudgetResource MockBudget(ArmClient armClient)
        {
            var currentSpend = Substitute.For<BudgetCurrentSpend>((decimal?)10, "CAD");
            var budgetData = Substitute.For<ConsumptionBudgetData>();
            budgetData.Amount = 10;
            budgetData.CurrentSpend.Returns(currentSpend);
            var budget =
                Substitute.For<ConsumptionBudgetResource>();
            budget.GetAsync()
                .Returns(Response.FromValue(budget, Substitute.For<Response>()));
            budget.UpdateAsync(WaitUntil.Completed, budgetData)
                .ReturnsForAnyArgs(Task.FromResult<ArmOperation<ConsumptionBudgetResource>>(null));
            return budget;
        }
    }
}