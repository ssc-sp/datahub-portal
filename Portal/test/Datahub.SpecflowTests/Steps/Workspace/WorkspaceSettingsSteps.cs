using Bunit;
using Bunit.TestDoubles;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services.Projects;
using Datahub.Core.Services.UserManagement;
using Datahub.Infrastructure.Offline;
using Datahub.Portal.Pages.Workspace.Settings;
using Datahub.Shared.Entities;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps.Workspace;

[Binding]
public class WorkspaceSettingsSteps(
    ScenarioContext scenarioContext,
    IWebHostEnvironment hostingEnvironment
) : TestContext
{
    private const string RelativePathToSrc = "../../../../../src";

    [Given(@"a workspace settings page")]
    public async Task GivenAWorkspaceSettingsPage()
    {
        Services.AddSingleton(hostingEnvironment);
        var portalConfiguration = new DatahubPortalConfiguration()
        {
            CultureSettings =
            {
                ResourcesPath = $"{RelativePathToSrc}/Datahub.Portal/i18n",
                AdditionalResourcePaths = []
            }
        };

        Services.AddMudServices();
        Services.AddDatahubLocalization(portalConfiguration);

        Services.AddStub<ICultureService>();
        Services.AddStub<IDatahubAuditingService>();
        Services.AddStub<IUserInformationService>();

        var mockRequestManagementService = Substitute.For<IRequestManagementService>();
        Services.AddSingleton(mockRequestManagementService);

        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContextFactory = new SpecFlowDbContextFactory(options);
        Services.AddSingleton<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);

        var workspace = new Datahub_Project()
        {
            Project_Acronym_CD = Testing.WorkspaceAcronym,
        };

        await using var context = await dbContextFactory.CreateDbContextAsync();
        context.Projects.Add(workspace);
        await context.SaveChangesAsync();

        var mockAuthorizationPolicyProvider = Substitute.For<IAuthorizationPolicyProvider>();
        mockAuthorizationPolicyProvider.GetDefaultPolicyAsync()
            .Returns(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("TEST USER");
        authContext.SetRoles(RoleConstants.DATAHUB_ROLE_ADMIN);

        Services.AddSingleton(mockAuthorizationPolicyProvider);

        JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);

        var workspaceSettingsPage = RenderComponent<WorkspaceSettingsPage>(parameterCollection =>
            parameterCollection.Add(p => p.WorkspaceAcronym, Testing.WorkspaceAcronym));

        scenarioContext["workspaceSettingsPage"] = workspaceSettingsPage;
    }

    [When(@"the prevent auto delete is toggled")]
    public void WhenThePreventAutoDeleteIsToggled()
    {
        var workspaceSettingsPage =
            scenarioContext["workspaceSettingsPage"] as IRenderedComponent<WorkspaceSettingsPage>;
        workspaceSettingsPage!.Instance.HandlePreventAutoDeleteChange(true);
    }

    [When(@"the workspace settings page changes are saved")]
    public async Task WhenTheWorkspaceSettingsPageChangesAreSaved()
    {
        var workspaceSettingsPage =
            scenarioContext["workspaceSettingsPage"] as IRenderedComponent<WorkspaceSettingsPage>;
        await workspaceSettingsPage!.Instance.SaveChanges();
    }

    [Then(@"the workspace should have the prevent delete setting enabled")]
    public async Task ThenTheWorkspaceShouldHaveThePreventDeleteSettingEnabled()
    {
        await using var ctx = await Services.GetRequiredService<IDbContextFactory<DatahubProjectDBContext>>()
            .CreateDbContextAsync();

        var workspace = await ctx.Projects
            .AsNoTracking()
            .FirstAsync(p => p.Project_Acronym_CD == Testing.WorkspaceAcronym);

        workspace.PreventAutoDelete.Should().BeTrue();
    }

    [When(@"the budget is changed to (.*)")]
    public void WhenTheBudgetIsChangedTo(decimal p0)
    {
        var workspaceSettingsPage =
            scenarioContext["workspaceSettingsPage"] as IRenderedComponent<WorkspaceSettingsPage>;
        workspaceSettingsPage!.Instance.HandleBudgetChanged(p0);
    }

    [Then(@"the workspace should trigger a terraform run")]
    public void ThenTheWorkspaceShouldTriggerATerraformRun()
    {
        var mockRequestManagementService = Services.GetRequiredService<IRequestManagementService>();
        mockRequestManagementService
            .Received()
            .HandleTerraformRequestServiceAsync(
                Arg.Any<Datahub_Project>(),
                Arg.Any<TerraformTemplate>(),
                Arg.Any<PortalUser>());
    }

    [Then(@"the workspace should have a budget of (.*)")]
    public async Task ThenTheWorkspaceShouldHaveABudgetOf(decimal p0)
    {
        await using var ctx = await Services.GetRequiredService<IDbContextFactory<DatahubProjectDBContext>>()
            .CreateDbContextAsync();

        var workspace = await ctx.Projects
            .FirstAsync(p => p.Project_Acronym_CD == Testing.WorkspaceAcronym);

        workspace.Project_Budget.Should().Be(p0);
    }

    [Then(@"the workspace should not trigger a terraform run")]
    public void ThenTheWorkspaceShouldNotTriggerATerraformRun()
    {
        var mockRequestManagementService = Services.GetRequiredService<IRequestManagementService>();
        mockRequestManagementService
            .DidNotReceive()
            .HandleTerraformRequestServiceAsync(
                Arg.Any<Datahub_Project>(),
                Arg.Any<TerraformTemplate>(),
                Arg.Any<PortalUser>());
    }

    [Then(@"the audit track data event should not be logged")]
    public void ThenTheAuditTrackDataEventShouldNotBeLogged()
    {
        var mockAuditService = Services.GetRequiredService<IDatahubAuditingService>();
        mockAuditService
            .DidNotReceive()
            .TrackDataEvent(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<AuditChangeType>(),
                Arg.Any<bool>());
    }

    [Then(@"the workspace settings page should show a save changes button")]
    public void ThenTheWorkspaceSettingsPageShouldShowASaveChangesButton()
    {
        var workspaceSettingsPage =
            scenarioContext["workspaceSettingsPage"] as IRenderedComponent<WorkspaceSettingsPage>;
        workspaceSettingsPage!.Render();
        workspaceSettingsPage!.Find(".mud-button-label").TextContent.Should().Be("Save Changes");
    }
}