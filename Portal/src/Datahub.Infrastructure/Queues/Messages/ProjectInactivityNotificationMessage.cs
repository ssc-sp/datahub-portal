using MediatR;
namespace Datahub.Infrastructure.Queues.Messages;
public record ProjectInactivityNotificationMessage(int ProjectId, int LastLogin, bool Whitelisted, DateTime? RetirementDate) : IRequest;
