using MediatR;
namespace Datahub.Infrastructure.Queues.Messages;
public record UserInactivityNotificationMessage(int UserId) : IRequest;