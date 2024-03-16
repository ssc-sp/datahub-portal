using MediatR;
namespace Datahub.Infrastructure.Queues.Messages;
public record ProjectUsageNotificationMessage(string ProjectAcronym) : IRequest;
