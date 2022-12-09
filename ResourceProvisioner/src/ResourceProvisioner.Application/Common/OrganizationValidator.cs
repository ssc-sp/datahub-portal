using FluentValidation;
using ResourceProvisioner.Domain.Entities;

namespace ResourceProvisioner.Application.Common;

public class OrganizationValidator : AbstractValidator<Organization>
{
    public OrganizationValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(10);
        
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}