using Datahub.Shared.Entities;
using FluentValidation;

namespace ResourceProvisioner.Application.Common;


public class WorkspaceValidator : AbstractValidator<TerraformWorkspace>
{
    public WorkspaceValidator()
    {
        RuleFor(x => x.Acronym).NotNull();
        RuleFor(x => x.Version)
            .MaximumLength(100);
        RuleFor(x => x.TerraformOrganization)
            .NotNull()
            .SetValidator(new OrganizationValidator());
    }
}