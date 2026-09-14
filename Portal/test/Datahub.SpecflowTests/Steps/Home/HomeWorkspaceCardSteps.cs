using Bunit;
using Datahub.Application.Services;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services.Projects;
using Datahub.Portal.Components.Projects;
using Datahub.Portal.Pages.Explore;
using Datahub.Portal.Pages.Home;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps.Home;

[Binding]
public sealed class HomeWorkspaceCardSteps(ScenarioContext scenarioContext) : BunitTestSteps
{
    private const string WorkspaceCardContextKey = "homeWorkspaceCard";
    private const string WorkspaceContextKey = "homeWorkspace";
    private const string WorkspaceUsersContextKey = "homeWorkspaceUsers";

    [Given(@"a home workspace card has one active user and one user with the (.*) role")]
    public void GivenAHomeWorkspaceCardHasOneActiveUserAndOneUserWithTheRole(string roleName)
    {
        var excludedRole = Enum.Parse<Project_Role.RoleNames>(roleName);

        scenarioContext[WorkspaceContextKey] = new Datahub_Project
        {
            Project_Acronym_CD = "TEST",
            Project_Name = "Test workspace"
        };
        scenarioContext[WorkspaceUsersContextKey] = new List<UserRoleLinks>
        {
            CreateUser(Project_Role.RoleNames.Collaborator),
            CreateUser(excludedRole)
        };
    }

    [When("the home workspace card is rendered")]
    public void WhenTheHomeWorkspaceCardIsRendered()
    {
        var workspace = scenarioContext.Get<Datahub_Project>(WorkspaceContextKey);
        var users = scenarioContext.Get<List<UserRoleLinks>>(WorkspaceUsersContextKey);

        Services.AddMudServices();
        Services.AddMemoryCache();
        Services.AddSingleton(Substitute.For<ICultureService>());
        Services.AddSingleton(Substitute.For<IDialogService>());
        Services.AddSingleton(Substitute.For<IResourceMessagingService>());
        Services.AddSingleton(Substitute.For<IRequestManagementService>());
        Services.AddSingleton(Substitute.For<IWorkspaceVersionService>());
        Services.AddSingleton(Substitute.For<ISnackbar>());
        Services.AddSingleton(Substitute.For<IStringLocalizer>());

        var projectUserManagementService = Substitute.For<IProjectUserManagementService>();
        projectUserManagementService.GetProjectUsersAsync(workspace.Project_Acronym_CD)
            .Returns(users);
        Services.AddSingleton(projectUserManagementService);

        var userInformationService = Substitute.For<IUserInformationService>();
        userInformationService.IsEntraUser().Returns(true);
        Services.AddSingleton(userInformationService);

        Services.AddSingleton(Substitute.For<IKeyInterceptorService>());
        ComponentFactories.AddStub<FeaturedProjectToggle>();
        this.AddAuthorization().SetAuthorized("Test user");

        scenarioContext[WorkspaceCardContextKey] = Render<HomeWorkspaceCard>(parameters => parameters
            .Add(component => component.Workspace, workspace));
    }

    [Then(@"the workspace user count should be (\d+)")]
    public void ThenTheWorkspaceUserCountShouldBe(int expectedCount)
    {
        var workspaceCard = scenarioContext.Get<IRenderedComponent<HomeWorkspaceCard>>(WorkspaceCardContextKey);
        var details = workspaceCard.FindComponent<ProjectPreviewCardIconDetails>();

        details.Instance.NumberOfUsers.Should().Be(expectedCount);
    }

    private static UserRoleLinks CreateUser(Project_Role.RoleNames roleName)
    {
        var role = Project_Role.GetAll().Single(role => role.Id == (int)roleName);
        return new UserRoleLinks
        {
            RoleId = role.Id,
            Role = role
        };
    }
}
