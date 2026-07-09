using Datahub.Shared.Entities;
using FluentValidation;
using ResourceProvisioner.Application.Common;

namespace ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;

public class WorkspaceDefinitionValidator : AbstractValidator<WorkspaceDefinition>
{
    public WorkspaceDefinitionValidator()
    {
        RuleFor(x => x.Workspace)
            .NotNull()
            .SetValidator(new WorkspaceValidator());

        RuleFor(x => x.Templates)
            .NotEmpty();

        RuleForEach(x => x.Templates)
            .SetValidator(new TerraformTemplateValidator());

        RuleFor(x => x.RequestingUserEmail)
            .EmailAddress();
    }    
}

