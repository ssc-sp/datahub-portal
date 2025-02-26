namespace Datahub.Application.Services;

public interface IQueuePongService
{
    Task<bool> Pong(string message);
}
