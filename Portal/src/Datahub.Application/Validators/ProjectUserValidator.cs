using Datahub.Core.Model.Projects;
using FluentValidation;

namespace Datahub.Application.Validators;

public class ProjectUserValidator : AbstractValidator<DatahubProjectUser>
{
    public ProjectUserValidator()
    {
        RuleFor(x => x.ProjectUserID)
            .NotEmpty();

        RuleFor(x => x.UserID)
            .NotEmpty();

        RuleFor(x => x.ApprovedUser)
            .NotEmpty();

        RuleFor(x => x.ApprovedDT)
            .NotEmpty()
            .Must(d => d!.Value.Date <= DateTime.Now.Date);

        RuleFor(x => x.UserName)
            .NotEmpty()
            .MaximumLength(200)
            .EmailAddress();
    }
}