using Datahub.Portal.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Reqnroll;
using System.Text;
using FluentAssertions;

namespace Datahub.SpecflowTests.Steps;

[Binding]
public class HostingServicesControllerSteps(
    ScenarioContext scenarioContext)
{
    [Given(@"I have a Hello World test message")]
    public void GivenIHaveAMessageHelloWorld()
    {
        // Arrange
        var message = "Hello, World!";
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(message));
        scenarioContext.Set(request);
    }

    [When(@"I send a POST request to the echo endpoint")]
    public async Task WhenISendTheMessageToTheEchoEndpoint()
    {
        // Act
        var controller = new HostingServicesController();
        var request = scenarioContext.Get<HttpRequest>();
        var response = await controller.ProcessRequest(request);
        scenarioContext.Set(response);
    }

    [Then(@"the response should have a Hello, World! message")]
    public void ThenTheResponseShouldHaveAMessageHelloWorld()
    {
        // Assert
        var response = scenarioContext.Get<IActionResult>();
        var result = response as OkObjectResult;
        result.Should().NotBeNull();
        result.Value.Should().Be("Hello, World!");
    }
}
