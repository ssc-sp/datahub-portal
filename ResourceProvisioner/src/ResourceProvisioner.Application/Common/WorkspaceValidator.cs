using FluentValidation;
using ResourceProvisioner.Domain.Entities;

namespace ResourceProvisioner.Application.Common;


public class WorkspaceValidator : AbstractValidator<TerraformWorkspace>
{
    public WorkspaceValidator()
    {
        RuleFor(x => x.Acronym).NotNull();
        RuleFor(x => x.Organization)
            .NotNull()
            .SetValidator(new OrganizationValidator());
    }
}