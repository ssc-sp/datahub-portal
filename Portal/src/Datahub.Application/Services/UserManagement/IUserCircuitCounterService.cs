namespace Datahub.Application.Services.UserManagement;

public interface IUserCircuitCounterService
{
    Task<bool> IsSessionEnabled();
    int GetSessionCount();
}