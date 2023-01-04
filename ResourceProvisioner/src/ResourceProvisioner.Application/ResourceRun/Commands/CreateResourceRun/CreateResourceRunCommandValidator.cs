using FluentValidation;
using ResourceProvisioner.Application.Common;

namespace ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;

public class CreateResourceRunCommandValidator : AbstractValidator<CreateResourceRunCommand>
{
    public CreateResourceRunCommandValidator()
    {
        RuleFor(x => x.TerraformWorkspace)
            .NotNull()
            .SetValidator(new WorkspaceValidator());

        RuleFor(x => x.Templates)
            .NotEmpty();

        RuleForEach(x => x.Templates)
            .SetValidator(new DataHubTemplateValidator());

        RuleFor(x => x.RequestingUserEmail)
            .EmailAddress();
    }    
}

