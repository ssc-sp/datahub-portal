using FluentValidation;
using ResourceProvisioner.Domain.Entities;

namespace ResourceProvisioner.Application.Common;

public class DataHubTemplateValidator : AbstractValidator<DataHubTemplate>
{
    public DataHubTemplateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
        
        RuleFor(x => x.Version)
            .NotEmpty()
            .MaximumLength(100);
    }
}