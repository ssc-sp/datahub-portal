using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Functions;
using Datahub.Functions.Services;
using Datahub.Functions.Validators;
using Datahub.Infrastructure.Services;
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
    ScenarioContext scenarioContext)
{
    [Given(@"a workspace with usage exceeding its budget")]
    public async Task GivenAWorkspaceWithUsageExceedingItsBudget()
    {
        var workspace = new Datahub_Project()
        {
            Project_Acronym_CD = Testing.WorkspaceAcronym,
            Project_Name = "RD1",
            Project_Budget = 1,
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
            Current = 10000,
        };
        
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        await ctx.Projects.AddAsync(workspace);
        await ctx.Project_Resources2.AddAsync(projectResource);
        await ctx.Project_Credits.AddAsync(projectCredits);
        await ctx.SaveChangesAsync();
    }

    [When(@"the notifier checks if a delete is required")]
    public async Task WhenTheNotifierChecksIfADeleteIsRequired()
    {
        var logger = Substitute.For<ILoggerFactory>();
        var sendEndpointProvider = Substitute.For<ISendEndpointProvider>();
        var pongService = Substitute.For<QueuePongService>(sendEndpointProvider);
        var emailValidator = Substitute.For<EmailValidator>();
        var emailService = Substitute.For<IEmailService>();
        var resourceMessagingService = Substitute.For<IResourceMessagingService>();

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
        var result = await projectNotifier.VerifyDeleteIsRequired(Testing.WorkspaceAcronym, CancellationToken.None, ctx);
        
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
}