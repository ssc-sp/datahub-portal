using System.Threading.Tasks;
using Datahub.Portal.Services;
using Datahub.Tests.Portal;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Datahub.Tests.Registration;

public class RegistrationFlowUserTests
{
    private readonly MockProjectDbContextFactory _dbFactory;
    private readonly RegistrationService _registrationService;

    public RegistrationFlowUserTests()
    {
        _dbFactory = new MockProjectDbContextFactory();
        var logger = Mock.Of<ILogger<RegistrationService>>();

        _registrationService = new RegistrationService(_dbFactory, logger, null);
    }

    [Fact (Skip = "Need to run manually against the graph function")]
    public async Task RegistrationFlow_UserCreate_Test()
    {
        var userEmail = "yjmrobert@gmail.com";
        var graphId = await _registrationService.SendUserInvite(userEmail);
        Assert.NotNull(graphId);
    }
}