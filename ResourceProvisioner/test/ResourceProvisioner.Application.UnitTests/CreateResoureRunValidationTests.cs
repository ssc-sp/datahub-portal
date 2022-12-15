using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using ResourceProvisioner.Domain.Entities;
using FluentAssertions;

namespace ResourceProvisioner.Application.UnitTests;

public class CreateResourceRunValidationTests
{


    [Test]
    public void ShouldRequireMinimumFields()
    {
        var command = new CreateResourceRunCommand();
        var validator = new CreateResourceRunCommandValidator();
        validator.Validate(command).Errors.Should().NotBeEmpty();
    }
    
    [Test]
    public void ShouldValidateIfMinimumFieldsAreProvided()
    {
        const string anyString = "abc";
        var command = new CreateResourceRunCommand
        {
            Workspace = new Workspace()
            {
                Acronym = anyString,
                Name = anyString,
                Organization = new Organization()
                {
                    Name = anyString,
                    Code = anyString
                },
            },
            Templates = new List<DataHubTemplate>()
            {
                new()
                {
                    Name = anyString,
                    Version = anyString
                }
            }
        };
        var validator = new CreateResourceRunCommandValidator();
        validator.Validate(command).Errors.Should().BeEmpty();
    }
}