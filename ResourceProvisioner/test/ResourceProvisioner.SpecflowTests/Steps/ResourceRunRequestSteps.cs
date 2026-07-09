using System.ComponentModel.DataAnnotations;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core.Amqp;
using Azure.Messaging.ServiceBus;
using Datahub.Shared;
using Datahub.Shared.Entities;
using Datahub.Shared.Entities.WorkspaceToolConfiguration;
using Reqnroll;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using ResourceProvisioner.Functions;
using Xunit;

namespace ResourceProvisioner.SpecflowTests.Steps;

[Binding]
[Collection("RepositoryAccess")]
public sealed class ResourceRunRequestSteps(
    ResourceRunRequest resourceRunRequest,
    ScenarioContext scenarioContext)
{
    [Given(@"a workspace definition with every required field")]
    public void GivenAWorkspaceDefinitionWithEveryRequiredField()
    {
        var WorkspaceDefinition = new WorkspaceDefinition()
        {
            Templates = [
                new TerraformTemplate("test", TerraformStatus.CreateRequested, DateTime.UtcNow),
                new TerraformTemplate("test2", TerraformStatus.CreateRequested, DateTime.UtcNow)
            ],
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
                    Framework = "test",
                    GitRepo = "test",
                    ComposePath = "test"
                }
            },
            RequestingUserEmail = "john@test.gc.ca",
            ResourceGroupName   = "test-rg"
        };

        scenarioContext["WorkspaceDefinition"] = WorkspaceDefinition;
    }

    [Given(@"the workspace app configuration is null")]
    public void GivenTheWorkspaceAppConfigurationIsNull()
    {
        var WorkspaceDefinition = scenarioContext["WorkspaceDefinition"] as WorkspaceDefinition;
        WorkspaceDefinition!.AppData = null!;
    }


    [Given(@"a workspace definition without every required field")]
    public void GivenAWorkspaceDefinitionWithoutEveryRequiredField()
    {
        var WorkspaceDefinition = new WorkspaceDefinition()
        {
            Templates = [],
            Workspace = new TerraformWorkspace(),
            AppData = new WorkspaceAppData(),
            RequestingUserEmail = "john@test.gc.ca",
            ResourceGroupName = "test-rg"
        };

        scenarioContext["WorkspaceDefinition"] = WorkspaceDefinition;
    }

    [When(@"a resource run request processes the workspace definition")]
    public async Task WhenAResourceRunRequestProcessesTheWorkspaceDefinition()
    {
        var WorkspaceDefinition = scenarioContext["WorkspaceDefinition"] as WorkspaceDefinition;

        var messageEnvelope = new JsonObject
        {
            ["message"] = JsonSerializer.SerializeToNode(WorkspaceDefinition)
        };

        var bodyBytes = Encoding.UTF8.GetBytes(messageEnvelope.ToJsonString());
        var amqpMessage = new AmqpAnnotatedMessage(new AmqpMessageBody(new List<ReadOnlyMemory<byte>>
        {
            bodyBytes
        }));

        amqpMessage.Header.DeliveryCount =1; // first delivery
        amqpMessage.Properties.MessageId = new AmqpMessageId(Guid.NewGuid().ToString());
        amqpMessage.MessageAnnotations["x-opt-enqueued-time"] = DateTime.UtcNow;
        amqpMessage.MessageAnnotations["x-opt-sequence-number"] =1L;

        var serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(
            amqpMessage,
            new BinaryData(Guid.NewGuid().ToString())); // lock token
        
        try
        {
            await resourceRunRequest.RunAsync(serviceBusReceivedMessage);
        }
        catch (Exception e)
        {
            scenarioContext["exception"] = ExceptionDispatchInfo.Capture(e);
        }
    }

    [Then(@"the resource run request should parse the workspace definition without errors")]
    public void ThenTheResourceRunRequestShouldParseTheWorkspaceDefinitionWithoutErrors()
    {
        if (scenarioContext.TryGetValue("exception", out object? value) && value is ExceptionDispatchInfo exception)
        {
            exception.Throw();
        }
    }


    [Then(@"the resource run request should parse the workspace definition with errors")]
    public void ThenTheResourceRunRequestShouldParseTheWorkspaceDefinitionWithErrors()
    {
        if (scenarioContext.TryGetValue("exception", out object? value) && value is not ExceptionDispatchInfo exception)
        {
            throw new Exception("Expected an Exception");
        }
    }
}