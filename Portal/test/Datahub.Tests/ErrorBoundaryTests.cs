using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Bunit;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Achievements;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data.Databricks;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Core.Services.UserManagement;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Portal.Layout;
using Datahub.Tests.Portal;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
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
using Xunit;

namespace Datahub.Tests;

public class ErrorBoundaryTests
{
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbConextFactoryMock;
    private readonly IWebHostEnvironment _hostingMock;
    private readonly Mock<IDatahubCatalogSearch> _datahubCatalogSearchMock;
    private readonly Mock<IAuthorizationPolicyProvider> _authorizationPolicyProvider;
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
    private readonly Mock<IServiceAuthManager> _serviceAuthManager;
    private readonly Mock<ILockedUserManagementService> _lockedUserManagementServiceMock;

    public ErrorBoundaryTests()
    {
        _dbConextFactoryMock = new MockProjectDbContextFactory();
        _auditingServiceMock = new Mock<IDatahubAuditingService>();
        _datahubCatalogSearchMock = new Mock<IDatahubCatalogSearch>();
        _authorizationPolicyProvider = new Mock<IAuthorizationPolicyProvider>();
        //_hostingMock = new Mock<IWebHostEnvironment>();
        _userInformationMock = new Mock<IUserInformationService>();
        _userInformationMock.Setup(x => x.GetCurrentPortalUserAsync()).ReturnsAsync(new PortalUser
        {
            Id = 1,
            Email = "john.doe@ssc-spc.gc.ca",
            DisplayName = "Test User",
            EntraUser = new EntraUser
            {
                GraphGuid = Guid.NewGuid().ToString(),
                PortalUser = null!
            }
        });
        _userInformationMock.Setup(x => x.IsEntraUser()).ReturnsAsync(true);
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
        _serviceAuthManager = new Mock<IServiceAuthManager>();
        _lockedUserManagementServiceMock = new Mock<ILockedUserManagementService>();
        _lockedUserManagementServiceMock.Setup(x => x.IsUserLockedAsync(It.IsAny<int>(), null))
            .ReturnsAsync(false);
        _serviceAuthManager.Setup(x => x.GetEntraUserAuthorizations(It.IsAny<string>()))
            .ReturnsAsync(System.Collections.Immutable.ImmutableList<(Project_Role Role, Datahub_Project Project)>.Empty);
        _hostingMock = Substitute.For<IWebHostEnvironment>();
    }

    private sealed class TestAuthStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "test.user@ssc-spc.gc.ca")
            }, "TestAuthType");

            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }
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

        await using var ctx = new Bunit.BunitContext();
        ctx.Services.AddSingleton(_dbConextFactoryMock);
        ctx.Services.AddSingleton<IConfiguration>(configuration);
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
        ctx.Services.AddSingleton(_serviceAuthManager.Object);
        ctx.Services.AddSingleton(_lockedUserManagementServiceMock.Object);
        ctx.Services.AddSingleton(_authorizationPolicyProvider.Object);
        ctx.Services.AddScoped<AuthenticationStateProvider, TestAuthStateProvider>();
        ctx.Services.AddAuthorizationCore();
        var authContext = ctx.AddAuthorization();
        authContext.SetAuthorizing();
        ctx.Services.AddMudServices();

        // Act - wrap PortalLayout in CascadingAuthenticationState so AuthorizeView gets AuthenticationState
        var cut = ctx.Render<CascadingAuthenticationState>(parameters =>
        {
            parameters.AddChildContent<PortalLayout>();
        });

        var portalLayout = cut.FindComponent<PortalLayout>();
        await portalLayout.Instance.ReportIssue(ex, corrlationId);

        await ctx.DisposeComponentsAsync();
        // Note: Do not call ctx.Dispose() here as BunitContext is being asynchronously disposed via await using
        _mediatrMock.Verify(m => m.Send(It.IsAny<BugReportMessage>(), default), Times.Once);
    }
}
