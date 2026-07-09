using Datahub.Shared;
using Datahub.Shared.Entities;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using FluentAssertions;
using FluentValidation;

namespace ResourceProvisioner.Application.IntegrationTests.ResourceRun.Commands;

using static Testing;
public class CreateResourceRunTests
{

    [Test]
    public async Task ShouldRequireMinimumFields()
    {
        var command = new WorkspaceDefinition()
        {
            RequestingUserEmail = "John@test.gc.ca",
            ResourceGroupName = "test-rg",
            Templates = new List<TerraformTemplate>
            {
                new("azure-storage-blob", TerraformStatus.CreateRequested, DateTime.UtcNow),
            },
            Workspace = null!,
            AppData = null!

        };

        await FluentActions.Invoking(() =>
            SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }
    
    [Test]    
    public async Task ShouldCreateResourceRun()
    {
        await RunAsDefaultUserAsync();
        
        var command = new WorkspaceDefinition
        {
            Templates = new List<TerraformTemplate>
            {
                new("azure-storage-blob", TerraformStatus.CreateRequested, DateTime.UtcNow),
            },
            Workspace = new TerraformWorkspace
            {
                Acronym = "TEST",
                Name = "Test Project",
                TerraformOrganization = new TerraformOrganization
                {
                    Name = "SBDA Number 42",
                    Code = "SBDA-42"
                }
            },
            RequestingUserEmail = "John@test.gc.ca",
            ResourceGroupName = "test-rg",
            AppData = new WorkspaceAppData()
        };
        
        var id = await SendAsync(command);
        Assert.That(id, Is.Not.Null);
    }
}
