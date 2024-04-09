using Datahub.Core.Model.Projects;
using FluentValidation;

namespace Datahub.Application.Validators;

public class ProjectUserValidator : AbstractValidator<Datahub_Project_User>
{
        public ProjectUserValidator()
        {
            RuleFor(x => x.ProjectUser_ID)
                .NotEmpty();

            RuleFor(x => x.User_ID)
                .NotEmpty();

            RuleFor(x => x.ApprovedUser)
                .NotEmpty();

            RuleFor(x => x.Approved_DT)
                .NotEmpty()
                .Must(d => d!.Value.Date <= DateTime.Now.Date);

            RuleFor(x => x.User_Name)
                .NotEmpty()
                .MaximumLength(200)
                .EmailAddress();
        }
}