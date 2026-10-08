using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Datahub.Application.Commands;
using Datahub.Core.Model.Projects;
using Xunit;

namespace Datahub.Tests.Commands;

public class ProjectUserAddExternalUserCommandTests
{
    [Fact]
    public void Validate_Fails_WhenTrimmedNamesOrOrganizationAreTooShort()
    {
        var command = CreateValidCommand();
        command.FirstName = " A ";
        command.LastName = "B";
        command.Organization = " C ";

        var validationResults = Validate(command);

        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(ProjectUserAddExternalUserCommand.FirstName)));
        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(ProjectUserAddExternalUserCommand.LastName)));
        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(ProjectUserAddExternalUserCommand.Organization)));
    }

    [Fact]
    public void Validate_Fails_WhenEmailIsInvalid()
    {
        var command = CreateValidCommand();
        command.Email = "not-an-email";

        var validationResults = Validate(command);

        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(ProjectUserAddExternalUserCommand.Email)));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Validate_Fails_WhenAccountExpiryIsNotInTheFuture(int dayOffset)
    {
        var command = CreateValidCommand();
        command.AccountExpiry = global::System.DateTime.Today.AddDays(dayOffset);

        var validationResults = Validate(command);

        var result = Assert.Single(validationResults,
            result => result.MemberNames.Contains(nameof(ProjectUserAddExternalUserCommand.AccountExpiry)));
        Assert.Equal("Account expiry date must be in the future.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_Succeeds_WhenAccountExpiryIsTomorrow()
    {
        var command = CreateValidCommand();
        command.AccountExpiry = global::System.DateTime.Today.AddDays(1);

        var validationResults = Validate(command);

        Assert.Empty(validationResults);
    }

    [Fact]
    public void Validate_Succeeds_ForValidCommand()
    {
        var command = CreateValidCommand();

        var validationResults = Validate(command);

        Assert.Empty(validationResults);
    }

    private static ValidationResult[] Validate(ProjectUserAddExternalUserCommand command)
    {
        var validationResults = new global::System.Collections.Generic.List<ValidationResult>();

        Validator.TryValidateObject(command, new ValidationContext(command), validationResults, validateAllProperties: true);

        return [.. validationResults];
    }

    private static ProjectUserAddExternalUserCommand CreateValidCommand()
    {
        return new ProjectUserAddExternalUserCommand
        {
            ProjectAcronym = "TEST",
            Email = "external.user@example.com",
            FirstName = "Jane",
            LastName = "Doe",
            Organization = "SSC",
            Role = Project_Role.GetAll().First(role => role.IsExternalRole),
            AccountExpiry = global::System.DateTime.Today.AddDays(1),
            CollaborationObjectives = "Collaborate on a shared deliverable."
        };
    }
}
