using System;
using Bunit;
using Datahub.Application.Services;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Users;
using Datahub.Portal.Components.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Datahub.Tests.Components.Auth;

public class LockedUserGatekeeperTests
{
    [Fact]
    public void Redirects_To_AccountLocked_When_User_Is_Locked()
    {
        using var ctx = new BunitContext();

        var userInfo = new Mock<IUserInformationService>();
        userInfo
            .Setup(s => s.GetCurrentPortalUserAsync())
            .ReturnsAsync(new PortalUser
            {
                Id = 42,
                Email = "locked@example.gc.ca",
                DisplayName = "Locked User",
                EntraUser = new EntraUser { GraphGuid = Guid.NewGuid().ToString(), PortalUser = null! }
            });

        var lockService = new Mock<ILockedUserManagementService>();
        lockService.Setup(s => s.IsUserLockedAsync(42)).ReturnsAsync(true);

        ctx.Services.AddSingleton(userInfo.Object);
        ctx.Services.AddSingleton(lockService.Object);
        ctx.Services.AddSingleton(Mock.Of<ILogger<LockedUserGatekeeper>>());

        ctx.Render<LockedUserGatekeeper>(parameters =>
            parameters.Add(p => p.ChildContent, _ => { }));

        var navigation = ctx.Services.GetRequiredService<NavigationManager>();
        Assert.Equal("http://localhost/account/locked", navigation.Uri);
    }

    [Fact]
    public void Does_Not_Redirect_When_User_Is_Not_Locked()
    {
        using var ctx = new BunitContext();

        var userInfo = new Mock<IUserInformationService>();
        userInfo
            .Setup(s => s.GetCurrentPortalUserAsync())
            .ReturnsAsync(new PortalUser
            {
                Id = 7,
                Email = "active@example.gc.ca",
                DisplayName = "Active User",
                EntraUser = new EntraUser { GraphGuid = Guid.NewGuid().ToString(), PortalUser = null! }
            });

        var lockService = new Mock<ILockedUserManagementService>();
        lockService.Setup(s => s.IsUserLockedAsync(7)).ReturnsAsync(false);

        ctx.Services.AddSingleton(userInfo.Object);
        ctx.Services.AddSingleton(lockService.Object);
        ctx.Services.AddSingleton(Mock.Of<ILogger<LockedUserGatekeeper>>());

        ctx.Render<LockedUserGatekeeper>(parameters =>
            parameters.Add(p => p.ChildContent, _ => { }));

        var navigation = ctx.Services.GetRequiredService<NavigationManager>();
        Assert.Equal("http://localhost/", navigation.Uri);
    }

    [Fact]
    public void Skips_Lock_Check_On_Locked_Page()
    {
        using var ctx = new BunitContext();

        var userInfo = new Mock<IUserInformationService>();
        var lockService = new Mock<ILockedUserManagementService>();

        ctx.Services.AddSingleton(userInfo.Object);
        ctx.Services.AddSingleton(lockService.Object);
        ctx.Services.AddSingleton(Mock.Of<ILogger<LockedUserGatekeeper>>());

        var navigation = ctx.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/account/locked");

        ctx.Render<LockedUserGatekeeper>(parameters =>
            parameters.Add(p => p.ChildContent, _ => { }));

        lockService.Verify(s => s.IsUserLockedAsync(It.IsAny<int>()), Times.Never);
        Assert.Equal("http://localhost/account/locked", navigation.Uri);
    }
}
