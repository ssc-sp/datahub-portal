using Datahub.Shared;
using Datahub.Shared.Entities;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using FluentAssertions;

namespace ResourceProvisioner.Application.UnitTests;

public class CreateResourceRunValidationTests
{


    [Test]
    public void ShouldRequireMinimumFields()
    {
        var command = new CreateResourceRunCommand() {
            RequestingUserEmail = "John@test.gc.ca",
            ResourceGroupName = "test-rg"
        };
        var validator = new CreateResourceRunCommandValidator();
        validator.Validate(command).Errors.Should().NotBeEmpty();
    }
    
    [Test]
    public void ShouldValidateIfMinimumFieldsAreProvided()
    {
        const string anyString = "abc";
        var command = new CreateResourceRunCommand
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
                new(TerraformTemplate.NewProjectTemplate, TerraformStatus.CreateRequested)
            },
            RequestingUserEmail = anyString,
            ResourceGroupName = anyString
        };
        var validator = new CreateResourceRunCommandValidator();
        validator.Validate(command).Errors.Should().BeEmpty();
    }
}