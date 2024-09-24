using System;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Security.Claims;
using Reqnroll;
using Datahub.Infrastructure.Services.ReverseProxy;
using Microsoft.Extensions.Logging;
using Datahub.Core.Data;
using Yarp.ReverseProxy.Transforms;
using FluentAssertions;

namespace Datahub.SpecflowTests.Steps
{
    [Binding]
    public class ApplyDatahubAdminRoleForProxyAccessStepDefinitions(ScenarioContext scenarioContext)
    {
        [Given("a user context with admin role {string}")]
        public void GivenAUserContextWithARoleOfTrue(string isDhAdmin)
        {
            var httpContextSubstitute = Substitute.For<HttpContext>();
            var workspace = bool.Parse(isDhAdmin) ? RoleConstants.DATAHUB_ADMIN_PROJECT: "RD1";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, workspace + RoleConstants.COLLABORATOR_SUFFIX)
            };
            httpContextSubstitute.User.Returns(new ClaimsPrincipal(new ClaimsIdentity(claims)));
            var logger = Substitute.For<ILogger<ContextRequestHeaderTransform>>();
            var contextTransform = new ContextRequestHeaderTransform("RD1", logger);
            scenarioContext["contextTransform"] = contextTransform;
            scenarioContext["httpContext"] = httpContextSubstitute;
        }

        [When("ContextRequestHeaderTransform processes the request")]
        public void WhenContextRequestHeaderTransformProcessesTheRequest()
        {
            var contextTransform = scenarioContext["contextTransform"] as ContextRequestHeaderTransform;
            var httpContext = scenarioContext["httpContext"] as HttpContext;
            var requestTransform = new RequestTransformContext() { HttpContext = httpContext};
  
            var result = contextTransform!.ApplyAsync(requestTransform);
            scenarioContext["resultCode"] = requestTransform.HttpContext.Response.StatusCode;
        }

        [Then("the status should be {int}")]
        public void ThenTheStatusShouldBe(int p0)
        {
            var resultCode = scenarioContext["resultCode"] as int?;
            resultCode.Should().Be(p0);
        }
    }
}
