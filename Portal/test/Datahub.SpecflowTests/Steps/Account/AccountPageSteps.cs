using System.Collections.Immutable;
using System.Security.Claims;
using Bunit;
using Datahub.Application.Authentication;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Achievements;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using Datahub.Infrastructure.Offline;
using Datahub.Portal.Pages.Account;
using Datahub.Portal.Pages.Account.PublicProfile;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.Identity.Web;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;
using Toolbelt.Blazor.Globalization;

namespace Datahub.SpecflowTests.Steps.Account;

[Binding]
public class AccountPageSteps : BunitTestSteps
{
    private readonly ScenarioContext _scenarioContext;
    private IRenderedComponent<CascadingAuthenticationState>? _accountPageRender;
    private readonly DatahubPortalConfiguration _portalConfig = new();

    private const string TEST_USER_EMAIL = "test@example.com";
    private const string TEST_USER_DISPLAY_NAME = "Test User";
    private const string TEST_USER_OID = "test-graph-id";

    public AccountPageSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private async Task SetupAuthenticationWithUser(bool gocUser)
    {
        var userClaimsIdentity = new ClaimsIdentity([
                new Claim(ClaimTypes.Name, TEST_USER_EMAIL),
                new Claim(ClaimConstants.ObjectId, TEST_USER_OID),
            ], "TestAuth");

        var tenantId = Guid.NewGuid().ToString();
        _portalConfig.AzureAd.TenantId = tenantId;

        if (gocUser)
        {
            var utid = Guid.NewGuid().ToString();
            userClaimsIdentity.AddClaim(new Claim(ClaimConstants.UniqueTenantIdentifier, utid));
            userClaimsIdentity.AddClaim(new Claim(ClaimConstants.TenantId, tenantId));

            var tenantIssuer = $"https://login.microsoftonline.com/{tenantId}/v2.0";
            var idProvider = $"https://sts.windows.net/{utid}/";
            userClaimsIdentity.AddClaim(new Claim(RoleClaimTransformer.IDENTITY_PROVIDER_CLAIM_TYPE, idProvider, ClaimValueTypes.String, tenantIssuer));
        }
        else
        {
            var externalIdp = " https://te.clegc-gckey.gc.ca";
            userClaimsIdentity.AddClaim(new Claim(RoleClaimTransformer.IDP_QUALIFIER_CLAIM, externalIdp));
            userClaimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "rDtRMzVvnuG-DPEfaOZMtPsn2_i-ayRIxIGvFzBIm-q"));
        }

        var userPrincipal = new ClaimsPrincipal(userClaimsIdentity);

        var serviceAuthManager = Substitute.For<IServiceAuthManager>();
        if (gocUser)
            serviceAuthManager.GetEntraUserAuthorizations(Arg.Any<string>()).Returns(Task.FromResult(ImmutableList<(Project_Role, Datahub_Project)>.Empty));
        else
            serviceAuthManager.GetExternalUserAuthorizations(Arg.Any<string>()).Returns(Task.FromResult(ImmutableList<(Project_Role, Datahub_Project)>.Empty));

        var featureManager = Substitute.For<IFeatureManagerSnapshot>();
        featureManager.IsEnabledAsync(Arg.Any<string>()).Returns(false);
        var transformer = new RoleClaimTransformer(serviceAuthManager, _portalConfig, featureManager, Substitute.For<ILogger<RoleClaimTransformer>>());
        userPrincipal = await transformer.TransformAsync(userPrincipal);

        var authContext = new TestAuthorizationContext
        {
            User = userPrincipal
        };

        Services.AddScoped<AuthenticationStateProvider>(sp => new TestAuthStateProvider(authContext));
        Services.AddScoped<IAuthorizationService>(sp => new TestAuthorizationService(authContext));
        Services.AddSingleton<IAuthorizationPolicyProvider, TestAuthorizationPolicyProvider>();
        JSInterop.SetupMudBlazor();
    }

    private IDbContextFactory<DatahubProjectDBContext> CreateDbContextWithMinimalNecessaryData(PortalUser portalUser)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatahubProjectDBContext>();
        optionsBuilder.EnableSensitiveDataLogging();

        var options = optionsBuilder
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContextFactory = new SpecFlowDbContextFactory(options);

        var context = dbContextFactory.CreateDbContext();

        var project = new Datahub_Project
        {
            Project_ID = 1,
            Project_Acronym_CD = "TEST1",
            Project_Name = "Test Project 1",
            Project_Status_Desc = "InProgress",
            Is_Private = false,
            Project_Icon = "database",
            Project_Summary_Desc = "Test"
        };

        context.UserRolesLinks.Add(new Core.Model.Projects.UserRoleLinks
        {
            PortalUser = portalUser,
            Project = project,
        });

        context.SaveChanges();

        return dbContextFactory;
    }

    private void AddRequiredServices()
    {
        Services.AddMudServices();
        Services.AddSingleton<NavigationManager>(new Bunit.TestDoubles.BunitNavigationManager(this));

        var testPortalUser = new PortalUser
        {
            EntraUser = new() { GraphGuid = TEST_USER_OID, PortalUser = null! },
            DisplayName = TEST_USER_DISPLAY_NAME,
            Email = TEST_USER_EMAIL,
            Id = 1
        };

        Services.AddSingleton<IDbContextFactory<DatahubProjectDBContext>>(CreateDbContextWithMinimalNecessaryData(testPortalUser));

        var mockUserInfo = Substitute.For<IUserInformationService>();

        mockUserInfo.GetCurrentPortalUserWithAchievementsAsync().Returns(Task.FromResult(testPortalUser));
        mockUserInfo.GetCurrentPortalUserAsync().Returns(Task.FromResult<PortalUser?>(testPortalUser));
        mockUserInfo.GetEntraUserAsync(Arg.Any<string>()).Returns(Task.FromResult(testPortalUser));
        mockUserInfo.GetCurrentUserEntraId().Returns(TEST_USER_OID);

        Services.AddSingleton<IUserInformationService>(mockUserInfo);
        Services.AddSingleton<ILogger<AccountPage>>(new LoggerFactory().CreateLogger<AccountPage>());

        Services.AddSingleton(_portalConfig);
        Services.AddDatahubLocalization(_portalConfig);

        var workspaceVersionService = Substitute.For<IWorkspaceVersionService>();
        workspaceVersionService.GetLatestVersionAsync().Returns(Task.FromResult("v1.0.0"));
        Services.AddSingleton(workspaceVersionService);

        Services.AddStub<IConfiguration>();

        // telemetry and timezone dependencies for UserCard
        var telemetryService = Substitute.For<IPortalUserTelemetryService>();
        telemetryService.GetUserLastLoginAsync().Returns(Task.FromResult<DateTime?>(null));
        Services.AddSingleton(telemetryService);

        var localTz = Substitute.For<ILocalTimeZone>();
        localTz.GetLocalTimeZoneAsync(null).Returns(ValueTask.FromResult(TimeZoneInfo.Utc));
        Services.AddSingleton(localTz);

        var cultureService = Substitute.For<ICultureService>();
        Services.AddSingleton(cultureService);

        JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
    }

    [Given(@"the user is authenticated with GOC login")]
    public async Task GivenTheUserIsAuthenticatedWithGocLogin()
    {
        AddRequiredServices();
        await SetupAuthenticationWithUser(true);
    }

    [Given(@"the user is authenticated with external login")]
    public async Task GivenTheUserIsAuthenticatedWithExternalLogin()
    {
        AddRequiredServices();
        await SetupAuthenticationWithUser(false);
    }

    [Given(@"the user is on the account page")]
    public void GivenTheUserIsOnTheAccountPage()
    {
        _accountPageRender = Render<CascadingAuthenticationState>(parameters =>
        {
            parameters.AddChildContent<AccountPage>();
        });

        _scenarioContext["accountPage"] = _accountPageRender;
    }

    [Then(@"the user should see their display name and email")]
    public void ThenTheUserShouldSeeTheirDisplayNameAndEmail()
    {
        var render = _scenarioContext["accountPage"] as IRenderedComponent<CascadingAuthenticationState>;
        render!.Find(".mud-typography").TextContent.Should().Contain("Test User");
        render!.Find(".mud-typography.mud-typography-caption").TextContent.Should().Contain("test@example.com");
    }

    private IRenderedComponent<MudChip<string>>? FindLoginProviderChip(IRenderedComponent<CascadingAuthenticationState> render, bool gocLogin)
    {
        var which = gocLogin ? AccountPublicProfile.TRUSTED_LOGIN_TAG : AccountPublicProfile.EXTERNAL_USER_TAG;

        var allChips = render.FindComponents<MudChip<string>>();
        var loginChip = allChips.FirstOrDefault(c => c.Instance.Tag is string tagStr && tagStr == which);
        return loginChip;
    }

    private void ThenTheUserShouldSeeTheirLoginProvider(bool gocLogin)
    {
        var render = _scenarioContext["accountPage"] as IRenderedComponent<CascadingAuthenticationState>;
        var loginChip = FindLoginProviderChip(render!, gocLogin);
        loginChip.Should().NotBeNull();
    }

    [Then(@"the user should see their GOC login provider chip")]
    public void ThenTheUserShouldSeeTheirGocLoginProvider()
    {
        ThenTheUserShouldSeeTheirLoginProvider(true);
    }

    [Then(@"the user should see their external login provider chip")]
    public void ThenTheUserShouldSeeTheirExternalLoginProvider()
    {
        ThenTheUserShouldSeeTheirLoginProvider(false);
    }

    // Helper auth/test classes copied from other test steps
    public class TestAuthStateProvider : AuthenticationStateProvider
    {
        private readonly TestAuthorizationContext _context;

        public TestAuthStateProvider(TestAuthorizationContext context)
        {
            _context = context;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_context.User));
        }
    }

    public class TestAuthorizationContext
    {
        public ClaimsPrincipal User { get; set; }
    }

    public class TestAuthorizationService : IAuthorizationService
    {
        private readonly TestAuthorizationContext _context;

        public TestAuthorizationService(TestAuthorizationContext context)
        {
            _context = context;
        }

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource,
            IEnumerable<IAuthorizationRequirement> requirements)
        {
            return Task.FromResult(AuthorizationResult.Success());
        }

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource,
            string policyName)
        {
            return Task.FromResult(AuthorizationResult.Success());
        }
    }

    public class TestAuthorizationPolicyProvider : IAuthorizationPolicyProvider
    {
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
            Task.FromResult(new AuthorizationPolicy(new[] { new DenyAnonymousAuthorizationRequirement() }, Array.Empty<string>()));

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName) =>
            Task.FromResult<AuthorizationPolicy?>(
                new AuthorizationPolicy(new[] { new DenyAnonymousAuthorizationRequirement() }, Array.Empty<string>()));

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
            Task.FromResult<AuthorizationPolicy?>(null);
    }
}
