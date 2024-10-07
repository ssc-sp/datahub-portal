using MediatR;

namespace Datahub.Infrastructure.Queues.Messages;

public record ProjectInactiveMessage(string WorkspaceAcronym) : IRequest
{
        
}