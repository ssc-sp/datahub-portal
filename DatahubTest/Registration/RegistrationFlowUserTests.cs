using System;
using System.Linq;
using System.Threading.Tasks;
using Datahub.Core.EFCore;
using Datahub.Portal.Data.Forms;
using Datahub.Portal.Services;
using Datahub.Tests.Portal;
using Microsoft.EntityFrameworkCore;
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
        _registrationService = new RegistrationService(_dbFactory, logger);
    }

    [Fact]
    public async Task RegistrationFlow_UserCreate_Test()
    {
        var userEmail = "RegistrationFlow_UserCreate_Test";
        var graphId = _registrationService.CreateUser(userEmail);
        Assert.NotNull(graphId);
    }
}