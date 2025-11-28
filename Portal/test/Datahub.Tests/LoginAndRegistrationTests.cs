using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Bunit;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Achievements;
using Datahub.Application.Services;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Context;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Core.Services.UserManagement;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Microsoft.JSInterop;
using Moq;
using MudBlazor;
using Xunit;
using Datahub.Tests.Portal;
using NSubstitute;
using MudBlazor.Services;
using Datahub.Portal.Pages.Public;
using MassTransit;
using Datahub.Application.Services.Metadata;
using Datahub.Core.Model.Users;
using Bunit.TestDoubles;

namespace Datahub.Tests
{
    public class LoginAndRegistrationTests
    {
        private IDbContextFactory<DatahubProjectDBContext> dbFactory;
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbConextFactoryMock;
        private readonly IWebHostEnvironment _hostingMock;
        private readonly Mock<IDatahubCatalogSearch> _datahubCatalogSearchMock;
        private readonly Mock<IDatahubAuditingService> _auditingServiceMock;
        private readonly Mock<IUserInformationService> _userInformationMock;
        private readonly Mock<IProjectUserManagementService> _projectUserManagementServiceMock;
        private readonly Mock<IResourceMessagingService> _resourceMessagingServiceMock;
        private readonly Mock<IUserEnrollmentService> _userEnrollmentServiceMock;
        private readonly Mock<IUserSettingsService> _userSettingsMock;
        private readonly Mock<ICultureService> _cultureServiceMock;
        private readonly Mock<IMetadataBrokerService> _metadataBrokerServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IJSRuntime> _jsRuntimeMock;
        private readonly Mock<IJSObjectReference> _jsModuleMock;
        private readonly Mock<ILocalStorageService> _localStorageMock;
        private readonly Mock<IMediator> _mediatrMock;
        private readonly Mock<ISnackbar> _snackBarMock;
        private readonly Mock<IPortalUserTelemetryService> _portalUserTelemetryServiceMock;
        private readonly Mock<IStringLocalizer> _stringLocalizerMock;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly RegisterPage _component;
        public LoginAndRegistrationTests()
        {
            _dbConextFactoryMock = new MockProjectDbContextFactory();
            _auditingServiceMock = new Mock<IDatahubAuditingService>();
            _datahubCatalogSearchMock = new Mock<IDatahubCatalogSearch>();
            _userInformationMock = new Mock<IUserInformationService>();
            _userEnrollmentServiceMock = new Mock<IUserEnrollmentService>();
            _userSettingsMock = new Mock<IUserSettingsService>();
            _cultureServiceMock = new Mock<ICultureService>();
            _metadataBrokerServiceMock = new Mock<IMetadataBrokerService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>() { CallBase = true };
            _jsRuntimeMock = new Mock<IJSRuntime>();
            _jsModuleMock = new Mock<IJSObjectReference> { CallBase = true };
            _localStorageMock = new Mock<ILocalStorageService>();
            _mediatrMock = new Mock<IMediator>();
            _snackBarMock = new Mock<ISnackbar>();
            _portalUserTelemetryServiceMock = new Mock<IPortalUserTelemetryService>();
            _projectUserManagementServiceMock = new Mock<IProjectUserManagementService>();
            _resourceMessagingServiceMock = new Mock<IResourceMessagingService>();
            _stringLocalizerMock = new Mock<IStringLocalizer> { CallBase = false };

            _sendEndpointProvider = Substitute.For<ISendEndpointProvider>();
            _hostingMock = Substitute.For<IWebHostEnvironment>();

            _component = new RegisterPage();
        }

        private Bunit.BunitContext SetupBunitContext(bool includeHttpClientFactory = false)
        {
            var workSpaces = new[] { "AAA", "BBB" };
            _snackBarMock.Setup(x => x.Configuration).Returns(new SnackbarConfiguration());
            _stringLocalizerMock.Setup(x => x["/locked"]).Returns(new LocalizedString("/locked", "/locked"));
            _stringLocalizerMock.Setup(x => x["/register"]).Returns(new LocalizedString("/register", "/register"));
            _stringLocalizerMock.Setup(x => x["/login"]).Returns(new LocalizedString("/login", "/login"));
            _projectUserManagementServiceMock.Setup(x => x.GetProjectListForPortalUser(It.IsAny<int>()))
                .ReturnsAsync(new List<string>(workSpaces));
            _projectUserManagementServiceMock.Setup(x => x.RunWorkspaceSync(It.IsAny<string>()))
                .ReturnsAsync(true);

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
            ctx.Services.AddSingleton(_userEnrollmentServiceMock.Object);
            ctx.Services.AddSingleton(_cultureServiceMock.Object);
            ctx.Services.AddSingleton(_metadataBrokerServiceMock.Object);
            ctx.Services.AddSingleton(_httpContextAccessorMock.Object);
            ctx.Services.AddSingleton(_jsRuntimeMock.Object);
            ctx.Services.AddSingleton(_localStorageMock.Object);
            ctx.Services.AddSingleton(_mediatrMock.Object);
            ctx.Services.AddSingleton(_stringLocalizerMock.Object);
            ctx.Services.AddSingleton(_portalUserTelemetryServiceMock.Object);
            ctx.Services.AddSingleton(_projectUserManagementServiceMock.Object);
            ctx.Services.AddSingleton(_sendEndpointProvider);

            ctx.Services.AddMudServices();

            if(includeHttpClientFactory)
            {
                ctx.Services.AddHttpClient();
            }

            return ctx;
        }

        [Fact(Skip = "Login logic needs to be refactored a bit for this test to work")]
        public async Task Test_NewUserLogin()
        {
            // Arrange
            var email = "fake_user@gc.ca";
            var fakeIdentity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock");
            var fakeClaimsPrincipal = new ClaimsPrincipal(fakeIdentity); 
            
            var ctx = SetupBunitContext();
            var navigationManager = ctx.Services.GetRequiredService<NavigationManager>();
            _userInformationMock.Setup(s => s.GetPortalUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(null as ExtendedPortalUser);

            _userInformationMock.Setup(s => s.GetAuthenticatedUser(It.IsAny<bool>()))
                .ReturnsAsync(fakeClaimsPrincipal);

            // Act
            var cut = ctx.Render<Login>(parameters => parameters.Add(p => p.redirectUri, "https://sso_url"));

            var emailInput = cut.Find("#Email");
            emailInput.Change(email);

            cut.Instance.HandleLogin();

            // verify redirect to register page
            Assert.Equal($"/register?email={email}", navigationManager.Uri);
        }

        [Fact]
        public async Task Test_LockedUserLogin()
        {
            // Arrange
            var email = "fake_user@gc.ca";
            var fakeIdentity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock");
            var fakeClaimsPrincipal = new ClaimsPrincipal(fakeIdentity);
            var fakePortalUser = new ExtendedPortalUser
            {
                GraphGuid = Guid.NewGuid().ToString(),
                DisplayName = "Test User",
                Email = email,
                IsDeleted = false,
                IsLocked = true
            };

            var ctx = SetupBunitContext(includeHttpClientFactory: true);
            var navigationManager = ctx.Services.GetRequiredService<NavigationManager>();

            _userInformationMock.Setup(s => s.GetPortalUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(fakePortalUser);

            _userInformationMock.Setup(s => s.GetAuthenticatedUser(It.IsAny<bool>()))
                .ReturnsAsync(fakeClaimsPrincipal);

            // Act
            var navManager = ctx.Services.GetRequiredService<NavigationManager>();
            var uri = navManager.GetUriWithQueryParameter("redirectUri", "https://sso_url");
            navManager.NavigateTo(uri);
            var cut = ctx.Render<Login>();

            var emailInput = cut.Find("#Email");
            emailInput.Change(email);

            cut.Instance.HandleLogin();

            // verify redirect to locked user page
            Assert.Equal("http://localhost/locked", navigationManager.Uri);
        }


        [Fact]
        public async Task Test_ActiveUserLogin()
        {
            // Arrange
            var email = "fake_user@gc.ca";
            var fakeIdentity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, null);
            var fakeClaimsPrincipal = new ClaimsPrincipal(fakeIdentity);
            var fakePortalUser = new ExtendedPortalUser
            {
                GraphGuid = Guid.NewGuid().ToString(),
                DisplayName = "Test User",
                Email = email,
                IsDeleted = false,
                IsLocked = false
            };

            var ctx = SetupBunitContext(includeHttpClientFactory: true);

            var conf = ctx.Services.GetRequiredService<DatahubPortalConfiguration>();
            conf.ShowLoginPage = false;

            _userInformationMock.Setup(s => s.GetPortalUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(fakePortalUser);

            _userInformationMock.Setup(s => s.GetAuthenticatedUser(It.IsAny<bool>()))
                .ReturnsAsync(fakeClaimsPrincipal);

            // Act
            var navManager = ctx.Services.GetRequiredService<NavigationManager>();
            var uri = navManager.GetUriWithQueryParameter("redirectUri", "https://sso_url");
            navManager.NavigateTo(uri);
            var cut = ctx.Render<Login>();
            

            var emailInput = cut.Find("#Email");
            emailInput.Change(email);
            cut.Render(); // re-render to reflect the input change

            cut.Instance.HandleLogin();

            // verify redirect to register
            var ssoRedirect = $"http://localhost/MicrosoftIdentity/Account/Challenge?redirectUri=https%3A%2F%2Fsso_url&scope=user.read%20openid%20offline_access%20profile&loginHint={email}&domainHint=&claims=&policy=";

            Assert.Equal(ssoRedirect, navManager.Uri);
        }

        [Fact]
        public async Task Test_DeletedUserRegistration()
        {
            // Arrange
            var email = "fake_user@gc.ca";
            var fakePortalUser = new ExtendedPortalUser
            {
                GraphGuid = Guid.NewGuid().ToString(),
                DisplayName = "Test User",
                Email = email,
                IsDeleted = true,
            };

            var ctx = SetupBunitContext();
            _userInformationMock.Setup(s => s.GetPortalUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(fakePortalUser);
            _userEnrollmentServiceMock.Setup(x => x.SendUserDatahubPortalInvite(It.IsAny<string>(),It.IsAny<string>()))
                .ReturnsAsync("1");


            var navManager = ctx.Services.GetRequiredService<NavigationManager>();
            var uri = navManager.GetUriWithQueryParameter("Email", email);
            navManager.NavigateTo(uri);
            var cut = ctx.Render<RegisterPage>();

            await cut.Instance.HandleValidSubmit();
            cut.Render();

            // Verify that GetProjectListForPortalUser was called
            _projectUserManagementServiceMock.Verify(x => x.GetProjectListForPortalUser(It.IsAny<int>()), Times.Once);

            // Verify that RunWorkspaceSync was  called
            _projectUserManagementServiceMock.Verify(x => x.RunWorkspaceSync(It.IsAny<string>()), Times.Exactly(2));

            // verify redirect to login
            Assert.Equal("http://localhost/login", navManager.Uri);
        }

        [Fact]
        public async Task Test_NewUserRegistration()
        {
            // Arrange
            var email = "fake_user@gc.ca"; 

            var ctx = SetupBunitContext();
            var navManager = ctx.Services.GetRequiredService<NavigationManager>();

            _userInformationMock.Setup(s => s.GetPortalUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(null as ExtendedPortalUser);
            _userEnrollmentServiceMock.Setup(x => x.SendUserDatahubPortalInvite(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("1");

            var uri = navManager.GetUriWithQueryParameter("Email", email);
            navManager.NavigateTo(uri);
            // Act
            var cut = ctx.Render<RegisterPage>();

            await cut.Instance.HandleValidSubmit();

            cut.Render();

            // Verify that GetProjectListForPortalUser was not called
            _projectUserManagementServiceMock.Verify(x => x.GetProjectListForPortalUser(It.IsAny<int>()), Times.Never);
            
            // verify redirect to login
            Assert.Equal("http://localhost/login", navManager.Uri);
        }
    }  
}
