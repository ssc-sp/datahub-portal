namespace Datahub.Core.Services.UserManagement;

public interface IGlobalSessionManager
{
    bool TryAddSession(string userId);
    void RemoveSession(string userId);
    int GetSessionCount(string userId);
}