using Datahub.Core.Model.Projects;
using Datahub.Portal.Pages.Workspace;
using Datahub.Portal.Pages.Workspace.Settings;
using Reqnroll;
using FluentAssertions;

namespace Datahub.SpecflowTests.Steps;

[Binding]
public class SupportAccessToWorkspacesSteps (
    ScenarioContext scenarioContext
)
{
    [Given(@"the user has created a workspace")]
    public void GivenTheUserHasCreatedAWorkspace()
    {
        // Arrange
        var workspace = new Datahub_Project()
        {
            Project_ID = 1,
            Project_Acronym_CD = "TEST",
            Project_Name = "Test Project",
        };

        scenarioContext.Set(workspace);
    }

    [When(@"the user has not requested support for the workspace")]
    public void WhenTheUserHasNotRequestedSupportForTheWorkspace()
    {
        // Act
        var workspace = scenarioContext.Get<Datahub_Project>();
        workspace.AllowDatahubSupport = new DateTime(2000, 6, 5);
        scenarioContext.Set(workspace);
    }

    [Then(@"the admin team should not have access to the workspace")]
    public void ThenTheAdminTeamShouldNotHaveAccessToTheWorkspace()
    {
        // Assert
        var workspace = scenarioContext.Get<Datahub_Project>();
        bool hasAccess = WorkspacePage.DisplayToSupport(workspace);
        hasAccess.Should().BeFalse();
    }

    [When(@"the user requests support for the workspace")]
    public void WhenTheUserRequestsSupportForTheWorkspace()
    {
        // Act
        var workspace = scenarioContext.Get<Datahub_Project>();
        workspace = WorkspaceSettingsPage.ExtendAdminAccess(workspace);
        scenarioContext.Set(workspace);
    }

    [Then(@"the admin team should have access to the workspace")]
    public void ThenTheAdminTeamShouldHaveAccessToTheWorkspace()
    {
        // Assert
        var workspace = scenarioContext.Get<Datahub_Project>();
        bool hasAccess = WorkspacePage.DisplayToSupport(workspace);
        hasAccess.Should().BeTrue();
    }

    [Given(@"the user has requested support for a workspace")]
    public void GivenTheUserHasRequestedSupportForAWorkspace()
    {
        // Arrange
        var workspace = new Datahub_Project()
        {
            Project_ID = 1,
            Project_Acronym_CD = "TEST",
            Project_Name = "Test Project",
            AllowDatahubSupport = new DateTime(2000, 6, 5),
        };

        workspace = WorkspaceSettingsPage.ExtendAdminAccess(workspace);

        scenarioContext.Set(workspace);
    }

    [When(@"the user revokes access to the workspace")]
    public void WhenTheUserRevokesSupportForTheWorkspace()
    {
        // Act
        var workspace = scenarioContext.Get<Datahub_Project>();
        workspace = WorkspaceSettingsPage.CancelAdminAccess(workspace);
        scenarioContext.Set(workspace);
    }
}