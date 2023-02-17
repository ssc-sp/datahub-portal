using MediatR;
namespace Datahub.Infrastructure.Queues.Messages;
public record ProjectUsageNotificationMessage(int ProjectId) : IRequest;
