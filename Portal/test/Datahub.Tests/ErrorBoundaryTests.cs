using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Bunit;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Achievements;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data.Databricks;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Users;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Core.Services.UserManagement;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Portal.Layout;
using Datahub.Tests.Portal;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Datahub.Tests;

public class ErrorBoundaryTests
{
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbConextFactoryMock;
    private readonly IWebHostEnvironment _hostingMock;
    private readonly Mock<IDatahubCatalogSearch> _datahubCatalogSearchMock;
    private readonly Mock<IDatahubAuditingService> _auditingServiceMock;
    private readonly Mock<IUserInformationService> _userInformationMock;
    private readonly Mock<IUserSettingsService> _userSettingsMock;
    private readonly Mock<ICultureService> _cultureServiceMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IJSRuntime> _jsRuntimeMock;
    private readonly Mock<IJSObjectReference> _jsModuleMock;
    private readonly Mock<ILocalStorageService> _localStorageMock;
    private readonly Mock<NavigationManager> _navigationManagerMock;
    private readonly Mock<IMediator> _mediatrMock;
    private readonly Mock<ISnackbar> _snackBarMock;
    private readonly Mock<IPortalUserTelemetryService> _portalUserTelemetryServiceMock;
    private readonly Mock<IStringLocalizer> _stringLocalizerMock;
    private readonly Mock<ISessionStorageService> _sessionStorageMock;

    public ErrorBoundaryTests()
    {
        _dbConextFactoryMock = new MockProjectDbContextFactory();
        _auditingServiceMock = new Mock<IDatahubAuditingService>();
        _datahubCatalogSearchMock = new Mock<IDatahubCatalogSearch>();
        //_hostingMock = new Mock<IWebHostEnvironment>();
        _userInformationMock = new Mock<IUserInformationService>();
        _userSettingsMock = new Mock<IUserSettingsService>();
        _cultureServiceMock = new Mock<ICultureService>();
        _httpContextAccessorMock =new Mock<IHttpContextAccessor>() { CallBase = true };
        _jsRuntimeMock = new Mock<IJSRuntime>();
        _jsModuleMock = new Mock<IJSObjectReference> { CallBase = true };
        _localStorageMock = new Mock<ILocalStorageService>();
        _navigationManagerMock = new Mock<NavigationManager>();
        _mediatrMock = new Mock<IMediator>();
        _snackBarMock = new Mock<ISnackbar>();
        _portalUserTelemetryServiceMock = new Mock<IPortalUserTelemetryService>();
        _stringLocalizerMock = new Mock<IStringLocalizer> { CallBase = false };
        _sessionStorageMock = new Mock<ISessionStorageService> { CallBase = false };

        _hostingMock = Substitute.For<IWebHostEnvironment>();
    }

    [Fact]
    public async Task ReportIssue_ExceptionHandled()
    {
        // Arrange
        var ex = new Exception("test");
        var corrlationId = Guid.NewGuid().ToString();
        var fakePortalUser = new PortalUser
        {
            Id = 1,
            Email = "john.doe@ssc-spc.gc.ca",
            DisplayName = "Test User",
            EntraUser = new EntraUser
            {
                GraphGuid = Guid.NewGuid().ToString(),
                PortalUser = null!
            }
        };
        _snackBarMock.Setup(x => x.Configuration).Returns(new SnackbarConfiguration());
        _stringLocalizerMock.Setup(x => x[It.IsAny<string>()]).Returns(new LocalizedString("test","test"));

        _hostingMock.EnvironmentName.Returns("Hosting:PortalUnitTestingEnvironment");

        _jsModuleMock.Setup(x => x.InvokeAsync<string>(It.IsAny<string>(), 
            It.IsAny<object[]>())).ReturnsAsync("data");
        _jsRuntimeMock.Setup(x => x.InvokeAsync<IJSObjectReference>(It.IsAny<string>(),
            It.IsAny<object[]>())).ReturnsAsync(_jsModuleMock.Object);

        var context = new DefaultHttpContext();
        context.Request.Headers["User-Agent"] = "fake_user_agent"; 
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.test.json", optional: true)
            .Build();

        var datahubPortalConfiguration = new DatahubPortalConfiguration();
        configuration.Bind(datahubPortalConfiguration);

        using var ctx = new Bunit.BunitContext();
        ctx.Services.AddSingleton(_dbConextFactoryMock); 
        ctx.Services.AddSingleton(datahubPortalConfiguration);
        ctx.Services.AddSingleton(_datahubCatalogSearchMock.Object);
        ctx.Services.AddSingleton(_auditingServiceMock.Object);
        ctx.Services.AddSingleton(_hostingMock);
        ctx.Services.AddSingleton(_userInformationMock.Object);
        ctx.Services.AddSingleton(_userSettingsMock.Object);
        ctx.Services.AddSingleton(_cultureServiceMock.Object);
        ctx.Services.AddSingleton(_httpContextAccessorMock.Object);
        ctx.Services.AddSingleton(_jsRuntimeMock.Object);
        ctx.Services.AddSingleton(_localStorageMock.Object);
        ctx.Services.AddSingleton(_navigationManagerMock.Object);
        ctx.Services.AddSingleton<NavigationManager, FakeNavigationManager>();
        ctx.Services.AddSingleton(_mediatrMock.Object);
        ctx.Services.AddSingleton(_stringLocalizerMock.Object);
        ctx.Services.AddSingleton(_portalUserTelemetryServiceMock.Object);
        ctx.Services.AddSingleton(_sessionStorageMock.Object);
        ctx.Services.AddMudServices();

        // Act
        var cut = ctx.Render<PortalLayout>();

        await cut.Instance.ReportIssue(ex, corrlationId);

        await ctx.DisposeComponentsAsync();
        await ctx.DisposeAsync();
        _mediatrMock.Verify(m => m.Send(It.IsAny<BugReportMessage>(), default), Times.Once);
    }
}
