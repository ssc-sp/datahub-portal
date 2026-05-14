using Datahub.Core.Data;
using Datahub.Core.Model.Projects;
using Datahub.SpecflowTests.Utils;
using Datahub.SpecflowTests.Steps.Workspace;
using Microsoft.AspNetCore.Hosting;
using Reqnroll;
using System;
using System.Collections.Generic;
using System.Text;
using Bunit;
using Datahub.Core.Components.AuthViews;
using FluentAssertions;
using System.Security.Claims;

namespace Datahub.SpecflowTests.Steps.Security;

[Binding]
public sealed class DatahubAuthViewSteps(
    ScenarioContext scenarioContext,
    IWebHostEnvironment hostingEnvironment
    ) : BunitTestSteps
{
    private const string ProjectAcronymKey = "ProjectAcronym";
    private const string AuthLevelKey = "AuthLevel";
    private const string ComponentKey = "Component";
    private const string UserTypeKey = "UserType";

    [Given("a workspace lead for workspace (.*)")]
    public void GivenAWorkspaceLeadForWorkspace(string workspaceAcronym)
    {
        // Set up the logged-in user as a workspace lead for the specified workspace
        CommonCbrTestUtils.AddLoggedInUserAuthorization(
            this,
            workspaceAcronym,
            isCbrOwner: true,
            isDhAdmin: true
        );

        // Store workspace acronym and user type for use in subsequent steps
        scenarioContext[ProjectAcronymKey] = workspaceAcronym;
        scenarioContext[UserTypeKey] = "WorkspaceLead";
    }

    [Given("a contributor for workspace (.*)")]
    public void GivenAContributorForWorkspace(string workspaceAcronym)
    {
        // Set up the logged-in user as a contributor (collaborator) for the specified workspace
        // A contributor has the collaborator role but not the workspace lead role
        var roleNames = new List<string>
        {
            RoleConstants.TRUSTED_ENTRA_LOGIN,
            $"{workspaceAcronym}{RoleConstants.COLLABORATOR_SUFFIX}"
        };

        var authContext = this.AddAuthorization();
        authContext.SetAuthorized("TEST CONTRIBUTOR");
        authContext.SetRoles([..roleNames]);

        // Provide minimal required claims for Entra scenarios
        var oid = Guid.NewGuid().ToString();
        var email = "contributor@ssc-spc.gc.ca";
        authContext.SetClaims(
            new System.Security.Claims.Claim(Microsoft.Identity.Web.ClaimConstants.ObjectId, oid),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, email)
        );

        // Store workspace acronym and user type for use in subsequent steps
        scenarioContext[ProjectAcronymKey] = workspaceAcronym;
        scenarioContext[UserTypeKey] = "Contributor";
    }

    [Given("an external user for workspace (.*)")]
    public void GivenAnExternalUserForWorkspace(string workspaceAcronym)
    {
        // Set up the logged-in user as a contributor (collaborator) for the specified workspace
        // A contributor has the collaborator role but not the workspace lead role
        var roleNames = new List<string>
        {
            RoleConstants.EXTERNAL_LOGIN,
            $"{workspaceAcronym}{RoleConstants.EXTERNAL_GUEST_ROLE}"
        };

        var authContext = this.AddAuthorization();
        authContext.SetAuthorized("TEST EXTERNAL USER");
        authContext.SetRoles([.. roleNames]);

        // Provide minimal required claims for Entra scenarios
        var oid = Guid.NewGuid().ToString();
        var email = "contributor@example.com";
        authContext.SetClaims(
            new System.Security.Claims.Claim(Microsoft.Identity.Web.ClaimConstants.ObjectId, oid),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, email)
        );

        // Store workspace acronym and user type for use in subsequent steps
        scenarioContext[ProjectAcronymKey] = workspaceAcronym;
        scenarioContext[UserTypeKey] = "External User";
    }

    [Given("a DatahubAuthView for workspace (.*) and AuthLevel (.*)")]
    public void GivenADatahubAuthViewForWorkspaceAndAuthLevel(string workspaceAcronym, string authLevel)
    {
        // Parse the auth level enum value
        if (!Enum.TryParse<DatahubAuthView.AuthLevels>(authLevel, out var parsedAuthLevel))
        {
            throw new InvalidOperationException($"Invalid AuthLevel: {authLevel}");
        }

        // Store the configuration for the component
        scenarioContext[ProjectAcronymKey] = workspaceAcronym;
        scenarioContext[AuthLevelKey] = parsedAuthLevel;
    }

    [When("the lead views the component")]
    public void WhenTheLeadViewsTheComponent()
    {
        var workspaceAcronym = (string)scenarioContext[ProjectAcronymKey];
        var authLevel = (DatahubAuthView.AuthLevels)scenarioContext[AuthLevelKey];

        // Render the DatahubAuthView component with the specified authorization level
        var component = Render<DatahubAuthView>(options => options
            .Add(p => p.AuthLevel, authLevel)
            .Add(p => p.ProjectAcronym, workspaceAcronym)
            .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<div class=\"authorized-content\">Workspace Lead Content</div>"))
            .Add(p => p.NotAuthorized, builder => builder.AddMarkupContent(0, "<div class=\"not-authorized-content\">Access Denied</div>"))
        );

        // Store the rendered component for verification
        scenarioContext[ComponentKey] = component;
    }

    [When("the contributor views the component")]
    public void WhenTheContributorViewsTheComponent()
    {
        var workspaceAcronym = (string)scenarioContext[ProjectAcronymKey];
        var authLevel = (DatahubAuthView.AuthLevels)scenarioContext[AuthLevelKey];

        // Render the DatahubAuthView component with the specified authorization level
        var component = Render<DatahubAuthView>(options => options
            .Add(p => p.AuthLevel, authLevel)
            .Add(p => p.ProjectAcronym, workspaceAcronym)
            .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<div class=\"authorized-content\">Workspace Lead Content</div>"))
            .Add(p => p.NotAuthorized, builder => builder.AddMarkupContent(0, "<div class=\"not-authorized-content\">Access Denied</div>"))
        );

        // Store the rendered component for verification
        scenarioContext[ComponentKey] = component;
    }

    [When("the external user views the component")]
    public void WhenTheExternalUserViewsTheComponent()
    {
        var workspaceAcronym = (string)scenarioContext[ProjectAcronymKey];
        var authLevel = (DatahubAuthView.AuthLevels)scenarioContext[AuthLevelKey];

        // Render the DatahubAuthView component with the specified authorization level
        var component = Render<DatahubAuthView>(options => options
            .Add(p => p.AuthLevel, authLevel)
            .Add(p => p.ProjectAcronym, workspaceAcronym)
            .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<div class=\"authorized-content\">Workspace Lead Content</div>"))
            .Add(p => p.NotAuthorized, builder => builder.AddMarkupContent(0, "<div class=\"not-authorized-content\">Access Denied</div>"))
        );

        // Store the rendered component for verification
        scenarioContext[ComponentKey] = component;
    }

    [Then("they should be able to view it")]
    public void ThenTheyShouldBeAbleToViewIt()
    {
        var component = (IRenderedComponent<DatahubAuthView>)scenarioContext[ComponentKey];

        // Verify that the authorized content is visible
        var authorizedContent = component.Find(".authorized-content");
        authorizedContent.Should().NotBeNull("the authorized content should be rendered for the workspace lead");
        authorizedContent.TextContent.Should().Contain("Workspace Lead Content");
    }

    [Then("they should not be able to view it")]
    public void ThenTheyShouldNotBeAbleToViewIt()
    {
        var component = (IRenderedComponent<DatahubAuthView>)scenarioContext[ComponentKey];

        // Verify that the not-authorized content is visible instead
        var notAuthorizedContent = component.Find(".not-authorized-content");
        notAuthorizedContent.Should().NotBeNull("the not-authorized content should be rendered for users without sufficient access");
        notAuthorizedContent.TextContent.Should().Contain("Access Denied");

        // Verify the authorized content is NOT visible
        try {
            // Attempt to find the authorized content - it should not be present and throw an exception
            var authorizedContent = component.Find(".authorized-content");

            // If we found the authorized content, that's a failure
            authorizedContent.Should().BeNull("the authorized content should not be rendered for users without sufficient access");
        }
        catch (ElementNotFoundException)
        {
            // Expected exception when the element is not found
        }
    }
}

