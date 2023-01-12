using Datahub.Shared.Entities;
using FluentValidation;

namespace ResourceProvisioner.Application.Common;

public class OrganizationValidator : AbstractValidator<TerraformOrganization>
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