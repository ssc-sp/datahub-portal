using Datahub.Core.Services.UserManagement;
using Microsoft.Extensions.Options;
using Moq;
using System;
using Xunit;

namespace Datahub.Tests.Sessions;

public class GlobalSessionManagerTests
{

    [Fact]
    public void TryAddSessionNoConfigAcceptsUnlimitedSessions()
    {
        var config = SetupConfig(new SessionsConfig());
        GlobalSessionManager manger = new(config);

        var userId = Guid.NewGuid().ToString();

        for (int i = 0; i < 100; i++)
        {
            Assert.True(manger.TryAddSession(userId));
        }
    }

    [Fact]
    public void TryAddSessionCountedSessionsAcceptslimitedSessions()
    {
        var config = SetupConfig(new SessionsConfig() { MaxSessionsPerUser = 2 });
        GlobalSessionManager manger = new(config);

        var userId = Guid.NewGuid().ToString();

        // first session allowed
        Assert.True(manger.TryAddSession(userId));

        // second session allowed
        Assert.True(manger.TryAddSession(userId));

        // third session not allowed
        Assert.False(manger.TryAddSession(userId));
    }


    [Fact]
    public void RemoveSessionForCountedSessionsAllowsNewSessions()
    {
        var config = SetupConfig(new SessionsConfig() { MaxSessionsPerUser = 2 });
        GlobalSessionManager manger = new(config);

        var userId = Guid.NewGuid().ToString();

        // first session allowed
        Assert.True(manger.TryAddSession(userId));

        // second session allowed
        Assert.True(manger.TryAddSession(userId));

        manger.RemoveSession(userId);

        // third session should be allowed
        Assert.True(manger.TryAddSession(userId));
    }

    [Fact]
    public void GetSessionCountForCountedSessionsShouldReturnExpected()
    {
        var config = SetupConfig(new SessionsConfig() { MaxSessionsPerUser = 3 });
        GlobalSessionManager manger = new(config);

        var userId = Guid.NewGuid().ToString();

        // add 3 sessions
        manger.TryAddSession(userId);
        manger.TryAddSession(userId);
        manger.TryAddSession(userId);
        // remove 1 session
        manger.RemoveSession(userId);
        // add 1 session
        manger.TryAddSession(userId);
        // remove 1 session
        manger.RemoveSession(userId);

        var expected = 2;
        var actual = manger.GetSessionCount(userId);

        Assert.Equal(expected, actual);
    }

    private IOptions<SessionsConfig> SetupConfig(SessionsConfig sessionsConfig)
    {
        var config = new Mock<IOptions<SessionsConfig>>();
        config.Setup(o => o.Value).Returns(sessionsConfig);
        return config.Object;
    }
}