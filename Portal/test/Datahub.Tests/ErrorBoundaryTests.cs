using Microsoft.EntityFrameworkCore;
using Moq;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Datahub.Core.Model.Datahub;
using Xunit;
using Microsoft.Extensions.Hosting;
using Datahub.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System;
using Datahub.Portal.Layout;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Engine.ClientProtocol;
using Bunit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Datahub.Application.Configuration;
using Datahub.Core.Services.UserManagement;
using MudBlazor;
using Microsoft.IdentityModel.Abstractions;
using Datahub.Core.Services.Achievements;
using MudBlazor.Services;
using Microsoft.AspNetCore.Components.Routing;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using Datahub.Tests.Portal;

namespace Datahub.Tests;

public class ErrorBoundaryTests
{
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbConextFactoryMock;
    private readonly Mock<DatahubPortalConfiguration> _datahubPortalConfigurationMock;
    private readonly Mock<IDatahubAuditingService> _auditingServiceMock;
    private readonly Mock<IWebHostEnvironment> _hostingMock;
    private readonly Mock<IUserInformationService> _userInformationMock;
    private readonly Mock<IUserSettingsService> _userSettingsMock;
    private readonly Mock<CultureService> _cultureServiceMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IJSRuntime> _jsRuntimeMock;
    private readonly Mock<ILocalStorageService> _localStorageMock;
    private readonly Mock<NavigationManager> _navigationManagerMock;
    private readonly Mock<IMediator> _mediatrMock;
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly Mock<ISnackbar> _snackBarMock;
    private readonly Mock<IPopoverService> _popoverServiceMock;
    private readonly Mock<IMudPopoverService> _mudPopoverServiceMock;
    private readonly Mock<MudPopoverProvider> _mudPopoverProvider;
    private readonly Mock<IBreakpointService> _breakpointServiceMock;
    private readonly Mock<IBrowserViewportService> _browserViewportServiceMock;
    private readonly Mock<IScrollManager> _scrollManagerMock;
    private readonly Mock<IPortalUserTelemetryService> _portalUserTelemetryServiceMock;
    private readonly Mock<IStringLocalizer> _stringLocalizerMock;

    public ErrorBoundaryTests()
    {
        _dbConextFactoryMock = new MockProjectDbContextFactory();
        _datahubPortalConfigurationMock = new Mock<DatahubPortalConfiguration>();
        _auditingServiceMock = new Mock<IDatahubAuditingService>();
        _hostingMock = new Mock<IWebHostEnvironment>();
        _userInformationMock = new Mock<IUserInformationService>();
        _userSettingsMock = new Mock<IUserSettingsService>();
        _cultureServiceMock = new Mock<CultureService>();
        _httpContextAccessorMock =new Mock<IHttpContextAccessor>() { CallBase = true };
        _jsRuntimeMock = new Mock<IJSRuntime>();
        _localStorageMock = new Mock<ILocalStorageService>();
        _navigationManagerMock = new Mock<NavigationManager>();
        _mediatrMock = new Mock<IMediator>();
        _dialogServiceMock= new Mock<IDialogService>();
        _snackBarMock = new Mock<ISnackbar>();
        _mudPopoverServiceMock = new Mock<IMudPopoverService>();
        _popoverServiceMock = new Mock<IPopoverService>();
        _mudPopoverProvider = new Mock<MudPopoverProvider> { CallBase = true };
        _breakpointServiceMock = new Mock<IBreakpointService> { CallBase = true };
        _browserViewportServiceMock = new Mock<IBrowserViewportService>();
        _scrollManagerMock = new Mock<IScrollManager>();
        _portalUserTelemetryServiceMock = new Mock<IPortalUserTelemetryService>();
        _stringLocalizerMock = new Mock<IStringLocalizer> { CallBase = true };
    }

    [Fact]
    public async Task ReportIssue_ExceptionHandled()
    {
        // Arrange
        var ex = new Exception("test");
        var corrlationId = Guid.NewGuid().ToString();

        _snackBarMock.Setup(x => x.Configuration).Returns(new SnackbarConfiguration());
        _stringLocalizerMock.Setup(x => x[It.IsAny<string>()]).Returns(new LocalizedString("test","test"));
         
        // _datahubPortalConfigurationMock.Setup(x => x.SupportFormUrl).Returns("");
        // _hostingMock.Setup(x => x.IsDevelopment()).Returns(false);

        using var ctx = new TestContext();
        ctx.Services.AddSingleton(_dbConextFactoryMock);
        ctx.Services.AddSingleton<DatahubPortalConfiguration>(new DatahubPortalConfiguration());
        //ctx.Services.AddSingleton(_datahubPortalConfigurationMock.Object);
        ctx.Services.AddSingleton(_auditingServiceMock.Object);
        ctx.Services.AddSingleton(_hostingMock.Object);
        ctx.Services.AddSingleton(_userInformationMock.Object);
        ctx.Services.AddSingleton(_userSettingsMock.Object);
        ctx.Services.AddSingleton(_cultureServiceMock.Object);
        ctx.Services.AddSingleton(_httpContextAccessorMock.Object);
        ctx.Services.AddSingleton(_jsRuntimeMock.Object);
        ctx.Services.AddSingleton(_localStorageMock.Object);
        ctx.Services.AddSingleton(_navigationManagerMock.Object);
        ctx.Services.AddSingleton<NavigationManager, FakeNavigationManager>();
        ctx.Services.AddSingleton(_mediatrMock.Object);
        ctx.Services.AddSingleton(_dialogServiceMock.Object);
        ctx.Services.AddSingleton(_snackBarMock.Object);
        ctx.Services.AddSingleton(_portalUserTelemetryServiceMock.Object);
        ctx.Services.AddSingleton(_popoverServiceMock.Object);
        ctx.Services.AddSingleton(_mudPopoverServiceMock.Object);
        ctx.Services.AddSingleton(new MudPopoverProvider());
        ctx.Services.AddSingleton(_breakpointServiceMock.Object);
        ctx.Services.AddSingleton(_browserViewportServiceMock.Object);
        ctx.Services.AddSingleton(_scrollManagerMock.Object);
        ctx.Services.AddLocalization();

        // Act
        var cut = ctx.RenderComponent<PortalLayout>();

        await cut.Instance.ReportIssue(ex, corrlationId);

    }
}
public class FakeNavigationManager : NavigationManager
{
    public event EventHandler<LocationChangedEventArgs> LocationChanged;

    public FakeNavigationManager()
    {
        Initialize("https://example.com/", "https://example.com/");
    }

    public void SimulateLocationChange(string newUri)
    {
        bool IsNavigationIntercepted = false;
        var args = new LocationChangedEventArgs(newUri, IsNavigationIntercepted);
        LocationChanged?.Invoke(this, args);
    }
}
public class FakePortalUserTelemetryService : IPortalUserTelemetryService
{
    public event EventHandler<AchievementsEarnedEventArgs> OnAchievementsEarned;

    public FakePortalUserTelemetryService()
    {
    }

    public void SimulatechievementsEarned(List<string> events)
    { 
        var args = new AchievementsEarnedEventArgs(events, false);
        OnAchievementsEarned?.Invoke(this, args);
    }

    public Task LogTelemetryEvent(string eventName)
    {
        throw new NotImplementedException();
    }
}