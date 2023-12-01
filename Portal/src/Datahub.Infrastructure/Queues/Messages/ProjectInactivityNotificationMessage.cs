using MediatR;
namespace Datahub.Infrastructure.Queues.Messages;
public record ProjectInactivityNotificationMessage(int ProjectId) : IRequest;
