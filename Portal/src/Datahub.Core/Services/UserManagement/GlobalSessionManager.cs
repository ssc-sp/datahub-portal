using Microsoft.Extensions.Options;

namespace Datahub.Core.Services.UserManagement;

public class GlobalSessionManager : IGlobalSessionManager
{
    private readonly Dictionary<string, int> sessions;
    private readonly int maxSessions;

    public GlobalSessionManager(IOptions<SessionsConfig> config)
    {
        sessions = new();
        maxSessions = config.Value?.MaxSessionsPerUser ?? -1;
    }

    public bool TryAddSession(string userId)
    {
        lock (sessions)
        {
            var count = 0;
            if (sessions.TryGetValue(userId, out count))
            {
                if (maxSessions > 0 && count >= maxSessions)
                    return false;
            }
            sessions[userId] = count + 1;
            return true;
        }
    }

    public void RemoveSession(string userId)
    {
        lock (sessions)
        {
            if (sessions.TryGetValue(userId, out int count))
            {
                sessions[userId] = Math.Max(0, count - 1);
            }
        }
    }

    public int GetSessionCount(string userId)
    {
        lock (sessions)
        {
            return sessions.TryGetValue(userId, out int count) ? count : 0;
        }
    }
}

public class SessionsConfig
{
    public int MaxSessionsPerUser { get; set; }
}