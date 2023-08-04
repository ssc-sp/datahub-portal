using Datahub.Shared.Entities;
using FluentValidation;

namespace ResourceProvisioner.Application.Common;

public class TerraformTemplateValidator : AbstractValidator<TerraformTemplate>
{
    public TerraformTemplateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
        

    }
}