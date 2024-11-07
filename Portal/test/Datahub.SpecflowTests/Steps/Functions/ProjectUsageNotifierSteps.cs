using Datahub.Application.Services;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Functions;
using Datahub.Functions.Services;
using Datahub.Functions.Validators;
using Datahub.Infrastructure.Services;
using Datahub.Shared;
using Datahub.Shared.Entities;
using Datahub.Shared.Enums;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps.Functions;

[Binding]
public class ProjectUsageNotifierSteps(
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
    AzureConfig azureConfig,
    IResourceMessagingService resourceMessagingService,
    ISendEndpointProvider sendEndpointProvider,
    IEmailService emailService,
    ScenarioContext scenarioContext)
{
    [Given(@"a workspace with usage exceeding its budget")]
    public async Task GivenAWorkspaceWithUsageExceedingItsBudget()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var workspace = new Datahub_Project()
        {
            Project_Acronym_CD = Testing.WorkspaceAcronym,
            Project_Name = "RD1",
            Project_Budget = 1,
        };

        var resourceTypes = new[]
        {
            "terraform:new-project-template",
            "terraform:azure-storage-blob",
            "terraform:azure-databricks",
            "terraform:azure-app-service",
            "terraform:azure-postgres",
        };

        foreach (var resourceType in resourceTypes)
        {
            var projectResource = new Project_Resources2()
            {
                ResourceType = resourceType,
                Project = workspace,
                Status = "Created",
            };

            await ctx.Project_Resources2.AddAsync(projectResource);
        }

        var projectCredits = new Project_Credits()
        {
            Project = workspace,
            Current = 10000,
        };

        await ctx.Projects.AddAsync(workspace);
        await ctx.Project_Credits.AddAsync(projectCredits);
        await ctx.SaveChangesAsync();
    }

    [When(@"the notifier checks if a delete is required")]
    public async Task WhenTheNotifierChecksIfADeleteIsRequired()
    {
        var logger = Substitute.For<ILoggerFactory>();
        var pongService = Substitute.For<QueuePongService>(sendEndpointProvider);
        var emailValidator = Substitute.For<EmailValidator>();

        var projectNotifier = new ProjectUsageNotifier(
            logger,
            azureConfig,
            dbContextFactory,
            pongService,
            emailValidator,
            sendEndpointProvider,
            emailService,
            resourceMessagingService);

        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var result =
            await projectNotifier.VerifyDeleteIsRequired(Testing.WorkspaceAcronym, CancellationToken.None, ctx);

        scenarioContext["result"] = result;
    }

    [Then(@"the result should be true")]
    public void ThenTheResultShouldBeTrue()
    {
        var result = scenarioContext.Get<bool>("result");
        result.Should().BeTrue();
    }

    [Given(@"a workspace with usage not exceeding its budget")]
    public async Task GivenAWorkspaceWithUsageNotExceedingItsBudget()
    {
        var workspace = new Datahub_Project()
        {
            Project_Acronym_CD = Testing.WorkspaceAcronym,
            Project_Name = "RD1",
            Project_Budget = 100,
        };

        var projectResource = new Project_Resources2()
        {
            ResourceType = "terraform:new-project-template",
            Project = workspace,
            Status = "Created",
        };

        var projectCredits = new Project_Credits()
        {
            Project = workspace,
            Current = 1,
        };

        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        await ctx.Projects.AddAsync(workspace);
        await ctx.Project_Resources2.AddAsync(projectResource);
        await ctx.Project_Credits.AddAsync(projectCredits);
        await ctx.SaveChangesAsync();
    }

    [Then(@"the result should be false")]
    public void ThenTheResultShouldBeFalse()
    {
        var result = scenarioContext.Get<bool>("result");
        result.Should().BeFalse();
    }

    [When(@"the notifier verifies overbudget is deleted")]
    public async Task WhenTheNotifierVerifiesOverbudgetIsDeleted()
    {
        var logger = Substitute.For<ILoggerFactory>();
        var pongService = Substitute.For<QueuePongService>(sendEndpointProvider);
        var emailValidator = Substitute.For<EmailValidator>();

        var projectNotifier = new ProjectUsageNotifier(
            logger,
            azureConfig,
            dbContextFactory,
            pongService,
            emailValidator,
            sendEndpointProvider,
            emailService,
            resourceMessagingService);

        await projectNotifier.VerifyOverBudgetIsDeleted(Testing.WorkspaceAcronym, CancellationToken.None);
    }

    [Then(@"the resources should be set to deleted")]
    public async Task ThenTheResourcesShouldBeSetToDeleted()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var resources = ctx.Project_Resources2
            .Where(r => r.Project.Project_Acronym_CD == Testing.WorkspaceAcronym)
            .ToList();

        resources.Should().NotBeEmpty();
        resources.Should().OnlyContain(r =>
            r.Status == TerraformStatus.DeleteRequested || r.Status == TerraformStatus.Deleted);
    }

    [Then(@"the resource messaging service should be notified")]
    public void ThenTheResourceMessagingServiceShouldBeNotified()
    {
        resourceMessagingService.Received().SendToTerraformQueue(Arg.Any<WorkspaceDefinition>());
    }

    [Given(@"the resources are already deleted")]
    public async Task GivenTheResourcesAreAlreadyDeleted()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var resources = ctx.Project_Resources2
            .Where(r => r.Project.Project_Acronym_CD == Testing.WorkspaceAcronym)
            .ToList();

        foreach (var resource in resources)
        {
            resource.Status = TerraformStatus.Deleted;
        }

        await ctx.SaveChangesAsync();
    }

    [Then(@"the resource messaging service should not be notified")]
    public void ThenTheResourceMessagingServiceShouldNotBeNotified()
    {
        resourceMessagingService.DidNotReceive().SendToTerraformQueue(Arg.Any<WorkspaceDefinition>());
    }

    [Given(@"the workspace has prevent auto delete set to true")]
    public async Task GivenTheWorkspaceHasPreventAutoDeleteSetToTrue()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var workspace = ctx.Projects
            .FirstOrDefault(p => p.Project_Acronym_CD == Testing.WorkspaceAcronym);

        workspace!.PreventAutoDelete = true;

        await ctx.SaveChangesAsync();
    }

    [Then(@"the resources should not be set to deleted")]
    public async Task ThenTheResourcesShouldNotBeSetToDeleted()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var resources = ctx.Project_Resources2
            .Where(r => r.Project.Project_Acronym_CD == Testing.WorkspaceAcronym)
            .ToList();

        resources.Should().NotBeEmpty();
        resources.Should().NotContain(r =>
            r.Status == TerraformStatus.DeleteRequested || r.Status == TerraformStatus.Deleted);
    }

    [Given(@"there is a workspace lead and (.*) admin users")]
    public async Task GivenThereIsAWorkspaceLeadAndAdminUsers(int p0)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var workspace = await ctx.Projects
            .FirstAsync(p => p.Project_Acronym_CD == Testing.WorkspaceAcronym);

        var workspaceLead = new PortalUser()
        {
            GraphGuid = Guid.NewGuid().ToString(),
            Email = "lead@email.com"
        };

        var adminUsers = new List<PortalUser>();
        for (var i = 0; i < p0; i++)
        {
            adminUsers.Add(new PortalUser()
            {
                GraphGuid = Guid.NewGuid().ToString(),
                Email = $"admin{i}@email.com",
            });
        }

        await ctx.PortalUsers.AddAsync(workspaceLead);
        await ctx.PortalUsers.AddRangeAsync(adminUsers);

        var projectUsers = new List<Datahub_Project_User>()
        {
            new()
            {
                Project = workspace,
                PortalUser = workspaceLead,
                RoleId = (int)Project_Role.RoleNames.WorkspaceLead
            }
        };

        foreach (var adminUser in adminUsers)
        {
            projectUsers.Add(new Datahub_Project_User()
            {
                Project = workspace,
                PortalUser = adminUser,
                RoleId = (int)Project_Role.RoleNames.Admin
            });
        }

        await ctx.Project_Users.AddRangeAsync(projectUsers);

        await ctx.SaveChangesAsync();
    }

    [Then(@"the (.*) admin users and workspace lead should be emailed")]
    public void ThenTheAdminUsersAndWorkspaceLeadShouldBeEmailed(int p0)
    {
        emailService.Received(5).BuildEmail(Arg.Is<string>(s => s.Equals("delete_notification.html")),
            Arg.Is<List<string>>(l => l.Count == p0 + 1),
            Arg.Any<List<string>>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>());
    }
}