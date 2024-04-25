using FluentValidation;

namespace Datahub.Application.Validators
{
    public class NameValidator : AbstractValidator<(string FirstName, string LastName)>
    {
        public NameValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name cannot be empty.")
                .MaximumLength(50).WithMessage("First name cannot be more than 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name cannot be empty.")
                .MaximumLength(50).WithMessage("Last name cannot be more than 50 characters.");
        }
    }
}