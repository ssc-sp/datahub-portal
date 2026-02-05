using System.Collections.Immutable;
using System.Security.Claims;
using Datahub.Application.Configuration;
using Datahub.Application.RoleManagement;
using Datahub.Application.Services.Security;
using Datahub.Core.Data;
using Datahub.Core.Model.Projects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.Identity.Web;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps.Security;

[Binding]
public class RoleClaimTransformerSteps
{
    private readonly ScenarioContext _ctx;
    private readonly IServiceAuthManager _authManager = Substitute.For<IServiceAuthManager>();
    private readonly DatahubPortalConfiguration _config = new()
    {
        AzureAd = new() { TenantId = "test-tenant" }
    };

    private ClaimsPrincipal? _principal;
    private ClaimsPrincipal? _result;

    public RoleClaimTransformerSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    [Given("an authorization store with workspaces and roles")]
    public void GivenFakeAuthStore()
    {
        var prj1 = new Datahub_Project { Project_Acronym_CD = "PRJ1" };
        var prj2 = new Datahub_Project { Project_Acronym_CD = "PRJ2" };

        var guestRole = new Project_Role { Id = (int)Project_Role.RoleNames.Guest, Name = RoleConstants.GUEST_ROLE, Description = RoleConstants.GUEST_ROLE };
        var adminRole = new Project_Role { Id = (int)Project_Role.RoleNames.Admin, Name = RoleConstants.ADMIN_ROLE, Description = RoleConstants.ADMIN_ROLE };
        var collabRole = new Project_Role { Id = (int)Project_Role.RoleNames.Collaborator, Name = RoleConstants.COLLABORATOR_ROLE, Description = RoleConstants.COLLABORATOR_ROLE };

        // External authorizations
        _authManager.GetExternalUserAuthorizations("ext-123").Returns(
        ImmutableList.Create<(Project_Role, Datahub_Project)>((guestRole, prj1), (adminRole, prj2))
        );

        // Entra authorizations
        _authManager.GetEntraUserAuthorizations("entra-456").Returns(
        ImmutableList.Create<(Project_Role, Datahub_Project)>((collabRole, prj1), (adminRole, prj2))
        );

        _authManager.IsAdminModeEnabled("entra-456").Returns(false);
        _authManager.IsUserCbrOwner("user@example.com").Returns(false);
    }

    [Given("an external user with name identifier \"(.*)\"")]
    public void GivenExternalUser(string id)
    {
        var identity = new ClaimsIdentity("external");
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, id));
        identity.AddClaim(new Claim(ClaimTypes.Role, RoleConstants.EXTERNAL_LOGIN));
        _principal = new ClaimsPrincipal(identity);
    }

    [Given("an entra user with object id \"(.*)\" and email \"(.*)\"")]
    public void GivenEntraUser(string oid, string email)
    {
        var identity = new ClaimsIdentity("entra");
        identity.AddClaim(new Claim(ClaimConstants.ObjectId, oid));
        identity.AddClaim(new Claim(ClaimTypes.Email, email));
        identity.AddClaim(new Claim(ClaimTypes.Role, RoleConstants.TRUSTED_ENTRA_LOGIN));
        _principal = new ClaimsPrincipal(identity);
    }

    [When("claims are transformed")]
    public async Task WhenClaimsTransformed()
    {
        var loggerFactory = LoggerFactory.Create(builder => { });
        var logger = loggerFactory.CreateLogger<RoleClaimTransformer>();
        var featureManager = Substitute.For<IFeatureManagerSnapshot>();
        featureManager.IsEnabledAsync(Arg.Any<string>()).Returns(false);
        var transformer = new RoleClaimTransformer(_authManager, _config, featureManager, logger);
        _result = await transformer.TransformAsync(_principal!);
    }

    [Then("the user should have role \"(.*)\"")]
    public void ThenUserShouldHaveRole(string role)
    {
        _result.Should().NotBeNull();
        _result!.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == role).Should().BeTrue();
    }
}
