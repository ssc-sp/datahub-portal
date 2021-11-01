using Datahub.Core.Services;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Datahub.Tests.Sessions
{
    public class UserCircuitCounterServiceTests
    {
        [Fact]
        public async Task IsSessionEnabled_ForCountedSessions_ShouldReturnExpected()
        {
            GlobalSessionManager globalSessionManager = new(SetupConfig(new SessionsConfig() { MaxSessionsPerUser = 2 }));
            IUserInformationService userInformationService = GetUserInformationService("sample_user_id");

            UserCircuitCounterService counter1 = new(globalSessionManager, userInformationService);
            var actual = await counter1.IsSessionEnabled();
            Assert.True(actual);

            UserCircuitCounterService counter2 = new(globalSessionManager, userInformationService);
            actual = await counter2.IsSessionEnabled();
            Assert.True(actual);

            UserCircuitCounterService counter3 = new(globalSessionManager, userInformationService);
            actual = await counter3.IsSessionEnabled();
            Assert.False(actual);
        }

        private IUserInformationService GetUserInformationService(string userId)
        {
            var service = new Mock<IUserInformationService>();
            service.Setup(s => s.GetUserIdString()).Returns(Task.FromResult(userId));
            return service.Object;
        }

        private IOptions<SessionsConfig> SetupConfig(SessionsConfig sessionsConfig)
        {
            var config = new Mock<IOptions<SessionsConfig>>();
            config.Setup(o => o.Value).Returns(sessionsConfig);
            return config.Object;
        }
    }
}
