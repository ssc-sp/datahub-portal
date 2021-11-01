namespace Datahub.Core.Services
{
    public interface IGlobalSessionManager
    {
        bool TryAddSession(string userId);
        void RemoveSession(string userId);
        int GetSessionCount(string userId);
    }
}