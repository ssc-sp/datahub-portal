using MediatR;
namespace Datahub.Infrastructure.Queues.Messages;
public record ProjectUsageUpdateMessage(int ProjectId, string ResourceGroup) : IRequest;
