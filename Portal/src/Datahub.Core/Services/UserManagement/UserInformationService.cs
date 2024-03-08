using System.Net.Mail;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Graph.Models;
using System.Security.Claims;
using Datahub.Core.Data;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Services.Security;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.CatalogSearch;
using Azure.Identity;

namespace Datahub.Core.Services.UserManagement;

public class UserInformationService : IUserInformationService
{
    private readonly ILogger<UserInformationService> _logger;
    private GraphServiceClient _graphServiceClient;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly NavigationManager _navigationManager;

    private readonly IConfiguration _configuration;
    private readonly ServiceAuthManager _serviceAuthManager;

    private readonly IDbContextFactory<DatahubProjectDBContext> _datahubContextFactory;

    private readonly CultureService _cultureService;
    private readonly IDatahubCatalogSearch _datahubCatalogSearch;

    private ClaimsPrincipal _authenticatedUser;

    public event EventHandler<PortalUserUpdatedEventArgs> PortalUserUpdated;

    private User _currentUser;

    private static User AnonymousUser => UserInformationServiceConstants.GetAnonymousUser();

    private bool _isViewingAsVisitor;

    public UserInformationService(
        ILogger<UserInformationService> logger,
        AuthenticationStateProvider authenticationStateProvider,
        NavigationManager navigationManager,
        IConfiguration configureOptions, ServiceAuthManager serviceAuthManager,
        IDatahubCatalogSearch datahubCatalogSearch,
        IDbContextFactory<DatahubProjectDBContext> datahubContextFactory, CultureService cultureService)
    {
        _logger = logger;
        _authenticationStateProvider = authenticationStateProvider;
        _navigationManager = navigationManager;
        _configuration = configureOptions;
        this._serviceAuthManager = serviceAuthManager;
        _datahubContextFactory = datahubContextFactory;
        _cultureService = cultureService;
        _datahubCatalogSearch = datahubCatalogSearch;
    }

    public async Task<ClaimsPrincipal> GetAuthenticatedUser(bool forceReload = false)
    {
        if (_authenticationStateProvider == null || forceReload)
            _authenticatedUser = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;
        return _authenticatedUser;
    }

    public async Task<string> GetUserIdString()
    {
        await CheckUser();
        return GetOid();
    }

    public async Task<string> GetUserEmail()
    {
        await CheckUser();
        return _currentUser.Mail;
    }

    public async Task<string> GetDisplayName()
    {
        await CheckUser();
        return _currentUser.DisplayName;
    }

    public async Task<string> GetUserEmailDomain()
    {
        await CheckUser();
        try
        {
            MailAddress email = new MailAddress(_currentUser.Mail);
            return email.Host.ToLower();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot parse email from {CurrentUserMail}", _currentUser?.Mail);
            return "?";
        }
    }

    public async Task<string> GetUserEmailPrefix()
    {
        await CheckUser();
        try
        {
            var email = new MailAddress(_currentUser.Mail);
            return email.User.ToLower();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot parse email from {CurrentUserMail}", _currentUser?.Mail);
            return "?";
        }
    }

    public async Task<string> GetUserRootFolder()
    {
        var domain = await GetUserEmailDomain();
        var prefix = await GetUserEmailPrefix();
        return $"{domain}/{prefix}";
    }

    public async Task<bool> IsUserWithoutInitiatives()
    {
        if (_isViewingAsVisitor)
            return true;
        var claims = (await GetAuthenticatedUser()).Claims.Where(c => c.Type == ClaimTypes.Role).ToList();

        return !claims.Any() || claims.Count == 1 && claims[0].Value == "default";
    }

    private async Task GetUserAsyncInternal()
    {
        if (_currentUser != null) return;
        try
        {
            var email = (await GetAuthenticatedUser())?.Identity?.Name;
            var userId = GetOid();
            if (email is null)
            {
                throw new InvalidOperationException("Cannot resolve user email");
            }

            PrepareAuthenticatedClient();
            _currentUser = await _graphServiceClient.Users[userId].GetAsync();
        }
        catch (ServiceException e)
        {
            if (e.InnerException is MsalUiRequiredException ||
                e.InnerException is MicrosoftIdentityWebChallengeUserException)
                throw;
            //_logger.LogError(e, "Error Loading User");
            throw new InvalidOperationException("Cannot retrieve user", e);
        }
        catch (Exception e)
        {
            //_logger.LogError(e, "Error Loading User"); redundant
            throw new InvalidOperationException("Cannot retrieve user list", e);
        }
    }

    private string GetOid()
    {
        // ReSharper disable once ConstantConditionalAccessQualifier
        return (_authenticatedUser?.Claims?
                    .FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier") ??
                throw new InvalidOperationException("Cannot access user claims")).Value;
    }

    public async Task<User> GetCurrentGraphUserAsync()
    {
        await CheckUser();
        return _currentUser;
    }


    private void PrepareAuthenticatedClient()
    {
        //if (graphServiceClient != null) return;
        try
        {

            //see https://learn.microsoft.com/en-us/graph/sdks/choose-authentication-providers?tabs=csharp
            // using Azure.Identity;
            var options = new ClientSecretCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            };
            var clientCertCredential = new ClientSecretCredential(
                _configuration.GetSection("AzureAd").GetValue<string>("TenantId"),
                _configuration.GetSection("AzureAd").GetValue<string>("ClientId"),
                _configuration.GetSection("AzureAd").GetValue<string>("ClientSecret"), options);
            _graphServiceClient = new(clientCertCredential);
        }
        catch (Exception e)
        {
            _logger.LogError($"Error preparing authentication client: {e.Message}");
            Console.WriteLine($"Error preparing authentication client: {e.Message}");
            throw;
        }
    }

    private async Task CheckUser()
    {
        if (_currentUser == null)
        {
            await GetUserAsyncInternal();
        }
    }

    public Task<User> GetAnonymousGraphUserAsync()
    {
        return Task.FromResult(AnonymousUser);
    }

    public async Task<User> GetGraphUserAsync(string userId)
    {
        try
        {
            PrepareAuthenticatedClient();
            _currentUser = await _graphServiceClient.Users[userId].GetAsync();


            return _currentUser;
        }
        catch (ServiceException e)
        {
            if (e.InnerException is MsalUiRequiredException ||
                e.InnerException is MicrosoftIdentityWebChallengeUserException)
                throw;
            _logger.LogError(e, "Error Loading User");
            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error Loading User");
            return null;
        }
    }

    public async Task<bool> IsViewingAsGuest()
    {
        return _serviceAuthManager.GetViewingAsGuest((await GetCurrentGraphUserAsync()).Id);
    }

    public async Task SetViewingAsGuest(bool isGuest)
    {
        _serviceAuthManager.SetViewingAsGuest((await GetCurrentGraphUserAsync()).Id, isGuest);
    }


    public Task<bool> IsViewingAsVisitor()
    {
        return Task.FromResult(_isViewingAsVisitor);
    }

    public Task SetViewingAsVisitor(bool isVisitor)
    {
        _isViewingAsVisitor = isVisitor;
        return Task.CompletedTask;
    }

    private async Task<bool> IsUserInDataHubAdminRole()
    {
        if ((await IsViewingAsGuest()) || _isViewingAsVisitor)
            return false;
        return await IsUserDatahubAdmin();
    }

    public async Task<bool> IsUserProjectAdmin(string projectAcronym)
    {
        if (string.IsNullOrWhiteSpace(projectAcronym))
            throw new ArgumentException("projectAcronym expected");

        if (await IsUserInDataHubAdminRole())
            return true;
        return (await GetAuthenticatedUser()).IsInRole($"{projectAcronym}{RoleConstants.ADMIN_SUFFIX}");
    }

    public async Task<bool> IsUserProjectWorkspaceLead(string projectAcronym)
    {
        if (string.IsNullOrWhiteSpace(projectAcronym))
            throw new ArgumentException("projectAcronym expected");

        if (await IsUserInDataHubAdminRole())
            return true;
        return (await GetAuthenticatedUser()).IsInRole($"{projectAcronym}{RoleConstants.WORKSPACE_LEAD_SUFFIX}");

    }

    public async Task<bool> IsUserDatahubAdmin()
    {
        return (await GetAuthenticatedUser()).IsInRole(RoleConstants.DATAHUB_ROLE_ADMIN);
    }

    public async Task<bool> IsUserProjectMember(string projectAcronym)
    {
        if (string.IsNullOrWhiteSpace(projectAcronym))
            throw new ArgumentException("projectAcronym expected");

        return ((await IsUserProjectAdmin(projectAcronym)) ||
                (await GetAuthenticatedUser()).IsInRole($"{projectAcronym}"));
    }

    /// <summary>
    /// Creates a new portal user if one does not exist
    /// </summary>
    /// <param name="userGraphId"></param>
    /// <returns>Portal User</returns>
    public async Task CreatePortalUserAsync(string userGraphId)
    {
        await using var ctx = await _datahubContextFactory.CreateDbContextAsync();
        var exists = await ctx.PortalUsers
            .FirstOrDefaultAsync(p => p.GraphGuid == userGraphId);

        if (exists is not null)
        {
            _logger.LogInformation("User with GraphId: {GraphId} already exists", userGraphId);
            return;
        }

        try
        {
            var graphUser = await _graphServiceClient.Users[userGraphId].GetAsync();
            var portalUser = new PortalUser
            {
                GraphGuid = userGraphId,
                Email = graphUser.Mail,
                DisplayName = graphUser.DisplayName,
            };

            ctx.PortalUsers.Add(portalUser);
            await ctx.SaveChangesAsync();
            _logger.LogInformation("Created new Portal User with GraphId: {GraphId}", userGraphId);

            var catalogObject = new Core.Model.Catalog.CatalogObject()
            {
                ObjectType = Core.Model.Catalog.CatalogObjectType.User,
                ObjectId = userGraphId,
                Name_English = graphUser.DisplayName,
                Name_French = graphUser.DisplayName,
                Desc_English = graphUser.Department,
                Desc_French = graphUser.Department
            };

            await _datahubCatalogSearch.AddCatalogObject(catalogObject);
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Error Loading User from Graph with GraphId: {GraphId}. It's possible they no longer exist",
                userGraphId);
        }
    }

    private async Task UpdatePortalUserLastLogin(string userGraphId)
    {
        await using var ctx = await _datahubContextFactory.CreateDbContextAsync();
        var portalUser = await ctx.PortalUsers.FirstOrDefaultAsync(p => p.GraphGuid == userGraphId);

        if (portalUser is not null)
        {
            portalUser.LastLoginDateTime = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
        }
        else
        {
            _logger.LogWarning("User with GraphId: {GraphId} does not exist", userGraphId);
        }
    }

    private async Task UpdatePortalUserFirstLogin(string userGraphId)
    {
        await using var ctx = await _datahubContextFactory.CreateDbContextAsync();
        var portalUser = await ctx.PortalUsers.FirstOrDefaultAsync(p => p.GraphGuid == userGraphId);

        if (portalUser is not null)
        {
            portalUser.FirstLoginDateTime = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
        }
        else
        {
            _logger.LogWarning("User with GraphId: {GraphId} does not exist", userGraphId);
        }
    }

    public async Task RegisterAuthenticatedPortalUser()
    {
        var graphId = await GetUserIdString();

        var portalUser = await GetPortalUserAsync(graphId);
        if (portalUser is null)
        {
            await CreatePortalUserAsync(graphId);
            await UpdatePortalUserFirstLogin(graphId);
        }
        else
        {
            if (portalUser.FirstLoginDateTime is null)
            {
                await UpdatePortalUserFirstLogin(graphId);
            }
            await UpdatePortalUserLastLogin(graphId);
        }
    }

    public async Task<bool> UpdatePortalUserAsync(PortalUser updatedUser)
    {
        try
        {
            await using var ctx = await _datahubContextFactory.CreateDbContextAsync();

            ctx.PortalUsers.Attach(updatedUser);
            ctx.Entry(updatedUser).State = EntityState.Modified;
            ctx.Entry(updatedUser.UserSettings).State = EntityState.Modified;
            await ctx.SaveChangesAsync();
            PortalUserUpdated?.Invoke(this, new PortalUserUpdatedEventArgs(updatedUser));
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating portal user");
            return false;
        }
    }

    public async Task<PortalUser> GetCurrentPortalUserAsync()
    {
        var graphId = await GetUserIdString();
        return await GetPortalUserAsync(graphId);
    }

    public async Task<PortalUser> GetPortalUserAsync(string userGraphId)
    {
        PortalUser portalUser;
        await using (var ctx = await _datahubContextFactory.CreateDbContextAsync())
        {
            portalUser = await ctx.PortalUsers
                .AsNoTracking()
                .Include(u => u.UserSettings)
                .FirstOrDefaultAsync(p => p.GraphGuid == userGraphId);

            if (portalUser is not null)
            {
                return portalUser;
            }
        }

        _logger.LogInformation("User with GraphId: {GraphId} does not exist", userGraphId);
        await CreatePortalUserAsync(userGraphId);

        await using (var ctx = await _datahubContextFactory.CreateDbContextAsync())
        {
            portalUser = await ctx.PortalUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.GraphGuid == userGraphId);

            return portalUser;
        }
    }

    public async Task<bool> IsDailyLogin()
    {
        var graphId = await GetUserIdString();
        var portalUser = await GetPortalUserAsync(graphId);

        if (portalUser is null)
            return false;

        var isFirstLoginDay = portalUser.FirstLoginDateTime.GetValueOrDefault().Date == DateTime.UtcNow.Date;
        var isLastLoginDay = portalUser.LastLoginDateTime.GetValueOrDefault().Date == DateTime.UtcNow.Date;

        return !isFirstLoginDay && !isLastLoginDay;
    }

    public async Task<PortalUser> GetCurrentPortalUserWithAchievementsAsync()
    {
        var graphId = await GetUserIdString();
        return await GetPortalUserWithAchievementsAsync(graphId);
    }

    public async Task<PortalUser> GetPortalUserWithAchievementsAsync(string userGraphId)
    {
        await using var ctx = await _datahubContextFactory.CreateDbContextAsync();

        var portalUser = await ctx.PortalUsers
            .AsNoTracking()
            .Include(p => p.UserSettings)
            .Include(p => p.Achievements)
            .ThenInclude(a => a.Achievement)
            .FirstOrDefaultAsync(p => p.GraphGuid == userGraphId);

        return portalUser;
    }
}