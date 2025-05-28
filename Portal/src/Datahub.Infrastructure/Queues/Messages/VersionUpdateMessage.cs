using MediatR;
namespace Datahub.Infrastructure.Queues.Messages;
public record VersionUpdateMessage(List<string> projectIds) : IRequest;

