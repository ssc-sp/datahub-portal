using System.Text;
using System.Text.Json;
using Azure.Core.Amqp;
using Azure.Messaging.ServiceBus;
using Datahub.Shared.Entities;
using Reqnroll;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using ResourceProvisioner.Functions;

namespace ResourceProvisioner.SpecflowTests.Steps;

[Binding]
public sealed class ResourceRunRequestSteps(
    ResourceRunRequest resourceRunRequest,
    ScenarioContext scenarioContext)
{

    [Given(@"a workspace definition with every required field")]
    public void GivenAWorkspaceDefinitionWithEveryRequiredField()
    {
        var createResourceRunCommand = new CreateResourceRunCommand()
        {
            Templates = [new TerraformTemplate() { Name = "test" }, new TerraformTemplate() { Name = "test2" }],
            Workspace = new TerraformWorkspace()
            {
                TerraformOrganization = new TerraformOrganization()
                {
                    Name = "test",
                    Code = "test"
                },
                Acronym = "test"
            },
            AppData = new WorkspaceAppData()
            {
                DatabricksHostUrl = "test",
                AppServiceConfiguration = new AppServiceConfiguration()
                {
                    Framework = "test"
                }
            }
        };

        scenarioContext["createResourceRunCommand"] = createResourceRunCommand;
    }
    
    [Given(@"the workspace app configuration is null")]
    public void GivenTheWorkspaceAppConfigurationIsNull()
    {
        var createResourceRunCommand = scenarioContext["createResourceRunCommand"] as CreateResourceRunCommand;
        createResourceRunCommand!.AppData = null!;
    }
    
    
    [Given(@"a workspace definition without every required field")]
    public void GivenAWorkspaceDefinitionWithoutEveryRequiredField()
    {
        var createResourceRunCommand = new CreateResourceRunCommand()
        {
            Templates = [],
            Workspace = new TerraformWorkspace(),
            AppData = new WorkspaceAppData()
        };

        scenarioContext["createResourceRunCommand"] = createResourceRunCommand;
    }
    
    [When(@"a resource run request processes the workspace definition")]
    public async Task WhenAResourceRunRequestProcessesTheWorkspaceDefinition()
    {
        var createResourceRunCommand = scenarioContext["createResourceRunCommand"] as CreateResourceRunCommand;
        var createResourceRunCommandString = JsonSerializer.Serialize(createResourceRunCommand);
        
        var serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(
            new AmqpAnnotatedMessage(new AmqpMessageBody(new List<ReadOnlyMemory<byte>>
            {
                Encoding.UTF8.GetBytes(createResourceRunCommandString)
            })), new BinaryData(Encoding.UTF8.GetBytes(createResourceRunCommandString)));
        try
        {
            await resourceRunRequest.RunAsync(serviceBusReceivedMessage);
        }
        catch (Exception e)
        {
            scenarioContext["exception"] = e;
        }
    }

    [Then(@"the resource run request should parse the workspace definition without errors")]
    public void ThenTheResourceRunRequestShouldParseTheWorkspaceDefinitionWithoutErrors()
    {
        if (scenarioContext.TryGetValue("exception", out object? value) && value is Exception exception)
        {
            throw exception;
        }
    }


    [Then(@"the resource run request should parse the workspace definition with errors")]
    public void ThenTheResourceRunRequestShouldParseTheWorkspaceDefinitionWithErrors()
    {
        if (!scenarioContext.TryGetValue("exception", out object? value) || value is not Exception exception)
        {
            throw new Exception("Expected exception was not thrown");
        }
    }


}