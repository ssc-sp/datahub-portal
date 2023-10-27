using MediatR;
namespace Datahub.Infrastructure.Queues.Messages;
public record ProjectInactivityUpdateMessage(int ProjectId) : IRequest;