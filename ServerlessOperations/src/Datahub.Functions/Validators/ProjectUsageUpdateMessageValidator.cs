using Datahub.Infrastructure.Queues.Messages;
using FluentValidation;
using Microsoft.Azure.Cosmos.Linq;

namespace Datahub.Functions.Validators
{
    public class ProjectUsageUpdateMessageValidator : AbstractValidator<ProjectUsageUpdateMessage>
    {
        public ProjectUsageUpdateMessageValidator()
        {
            RuleFor(x => x.ProjectAcronym)
                .NotEmpty();

            RuleFor(x => x.CostsBlobName)
                .NotEmpty()
                .Must(x => x.EndsWith(".json"));
        }
    }
}