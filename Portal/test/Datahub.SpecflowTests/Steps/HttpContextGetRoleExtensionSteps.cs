using System.Security.Claims;
using Datahub.Infrastructure.Services.ReverseProxy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps;

[Binding]
public class HttpContextGetRoleExtensionSteps(ScenarioContext scenarioContext)
{
    [Given(@"a user context with a claim of (.*) and (.*)")]
    public void GivenAUserContextWithAClaimOf(string workspaceRoleSuffix, string workspaceAcronym)
    {
        var httpContextSubstitute = Substitute.For<HttpContext>();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, workspaceAcronym + workspaceRoleSuffix)
        };
        httpContextSubstitute.User.Returns(new ClaimsPrincipal(new ClaimsIdentity(claims)));
        scenarioContext["httpContext"] = httpContextSubstitute;
    }

    [When(@"the user requests their role in the workspace (.*)")]
    public void WhenTheUserRequestsTheirRoleInTheWorkspace(string workspaceAcronym)
    {
        var httpContext = scenarioContext["httpContext"] as HttpContext;
        var workspaceRole = httpContext?.GetWorkspaceRole(workspaceAcronym);
        scenarioContext["workspaceRole"] = workspaceRole;
    }

    [Then(@"the role should be (.*)")]
    public void ThenTheRoleShouldBe(string role)
    {
        var workspaceRole = scenarioContext["workspaceRole"] as string;
        
        if (role == "null")
        {
            workspaceRole.Should().BeNull();
            return;
        }
        
        workspaceRole.Should().Be(role);
    }
}