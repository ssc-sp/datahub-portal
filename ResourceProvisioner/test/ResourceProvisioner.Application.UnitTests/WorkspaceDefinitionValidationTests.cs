using Datahub.Shared;
using Datahub.Shared.Entities;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using FluentAssertions;

namespace ResourceProvisioner.Application.UnitTests;

public class WorkspaceDefinitionValidationTests
{


    [Test]
    public void ShouldRequireMinimumFields()
    {
        var command = new WorkspaceDefinition() {
            RequestingUserEmail = "John@test.gc.ca",
            ResourceGroupName = "test-rg",
            Templates = new List<TerraformTemplate>()
            {
                new(TerraformTemplate.NewProjectTemplate, TerraformStatus.CreateRequested, DateTime.UtcNow)            
            },
            Workspace = null!,
            AppData = null!
        };
        var validator = new WorkspaceDefinitionValidator();
        validator.Validate(command).Errors.Should().NotBeEmpty();
    }
    
    [Test]
    public void ShouldValidateIfMinimumFieldsAreProvided()
    {
        const string anyString = "abc";
        var command = new WorkspaceDefinition
        {
            Workspace = new TerraformWorkspace()
            {
                Acronym = anyString,
                Name = anyString,
                TerraformOrganization = new TerraformOrganization()
                {
                    Name = anyString,
                    Code = anyString
                },
            },
            Templates = new List<TerraformTemplate>()
            {
                new(TerraformTemplate.NewProjectTemplate, TerraformStatus.CreateRequested, DateTime.UtcNow)            
            },
            RequestingUserEmail = "john.doe@test.gc.ca",
            AppData = new WorkspaceAppData(),
            ResourceGroupName = anyString
        };
        var validator = new WorkspaceDefinitionValidator();
        validator.Validate(command).Errors.Should().BeEmpty();
    }
}
