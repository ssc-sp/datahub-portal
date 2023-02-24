using MediatR;
namespace Datahub.Infrastructure.Queues.Messages;
public record StorageAccountMessage(int ProjectId, string ResourceGroup, string StorageAccount) : IRequest;
