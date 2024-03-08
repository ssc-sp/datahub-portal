using Microsoft.Extensions.Options;

namespace Datahub.Core.Services.UserManagement;

public class GlobalSessionManager : IGlobalSessionManager
{
    private readonly Dictionary<string, int> _sessions;
    private readonly int _maxSessions;

    public GlobalSessionManager(IOptions<SessionsConfig> config)
    {
        _sessions = new();
        _maxSessions = config.Value?.MaxSessionsPerUser ?? -1;
    }

    public bool TryAddSession(string userId)
    {
        lock (_sessions)
        {
            var count = 0;
            if (_sessions.TryGetValue(userId, out count))
            {
                if (_maxSessions > 0 && count >= _maxSessions)
                    return false;
            }
            _sessions[userId] = count + 1;
            return true;
        }
    }

    public void RemoveSession(string userId)
    {
        lock (_sessions)
        {
            if (_sessions.TryGetValue(userId, out int count))
            {
                _sessions[userId] = Math.Max(0, count - 1);
            }
        }
    }

    public int GetSessionCount(string userId)
    {
        lock (_sessions)
        {
            return _sessions.TryGetValue(userId, out int count) ? count : 0;
        }
    }
}

public class SessionsConfig
{
    public int MaxSessionsPerUser { get; set; }
}