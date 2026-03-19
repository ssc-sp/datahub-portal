using Datahub.Application.Services.Notification;
using Datahub.Core.Model.Context;
using Datahub.Infrastructure.Services.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Reqnroll;
using Reqnroll.BoDi;

namespace Datahub.SpecflowTests.Hooks;

[Binding]
public class ExternalUserInvitationHook
{
    [BeforeScenario("ExternalUserInvitationService")]
    public void BeforeScenario(IObjectContainer objectContainer)
    {
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var dbContextFactory = new SpecFlowDbContextFactory(options);
        objectContainer.RegisterInstanceAs<IDbContextFactory<DatahubProjectDBContext>>(dbContextFactory);

        var gcNotifyService = Substitute.For<IGCNotifyService>();
        gcNotifyService
            .SendExternalUserInviteNotification(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>())
            .Returns(Task.CompletedTask);
        objectContainer.RegisterInstanceAs(gcNotifyService);

        var logger = Substitute.For<ILogger<ExternalUserInvitationService>>();
        var service = new ExternalUserInvitationService(dbContextFactory, gcNotifyService, logger);
        objectContainer.RegisterInstanceAs<ExternalUserInvitationService>(service);
    }
}
