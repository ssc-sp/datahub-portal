using Azure.Identity;
using Datahub.Application.Services;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Configuration;
using Datahub.Core.Data;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Users;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Core.Services.UserManagement;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System.ComponentModel;
using System.Net.Mail;
using System.Security.Claims;

namespace Datahub.Infrastructure.Services.UserManagement;

public class UserInformationService(
    ILogger<UserInformationService> logger,
    AuthenticationStateProvider authenticationStateProvider,
    IConfiguration configureOptions,
    IServiceAuthManager serviceAuthManager,
    IDatahubCatalogSearch datahubCatalogSearch,
    IFeatureManagerSnapshot featureManager,
    IUserEnrollmentService userEnrollmentService,
    IDbContextFactory<DatahubProjectDBContext> datahubContextFactory)
    : IUserInformationService
{
    private GraphServiceClient graphServiceClient = null!;
    private ClaimsPrincipal authenticatedUser = null!;
    public event EventHandler<PortalUserUpdatedEventArgs> PortalUserUpdated = null!;
    private User? currentEntraUser = null;
    private static User AnonymousUser => UserInformationServiceConstants.GetAnonymousUser();
    private bool _isViewingAsVisitor;
    private PortalUser? _userWithAchievements;

    public async Task<ClaimsPrincipal> GetAuthenticatedUser(bool forceReload = false)
    {
        if (authenticatedUser == null || forceReload)
        {
            authenticatedUser = (await authenticationStateProvider!.GetAuthenticationStateAsync()).User;
            var traceClaims = await featureManager.IsEnabledAsync(Features.Trace_Claims);
            if (traceClaims)
            {
                logger.LogDebug("User:{User} Authentication Status: {IsAuthenticated}", authenticatedUser?.Identity?.Name ?? "Unknown", authenticatedUser?.Identity?.IsAuthenticated);
                if (authenticatedUser == null)
                {
                    logger.LogDebug("No authenticated user found.");
                    throw new InvalidOperationException("No authenticated user found.");
                }
                foreach (var claim in authenticatedUser.Claims)
                {
                    logger.LogDebug("Claim: {Type} - {Value}", claim.Type, claim.Value);
                }
                logger.LogDebug("Is External? {IsExternal}", authenticatedUser.HasClaim(ClaimTypes.Role, RoleConstants.EXTERNAL_LOGIN) ? "Yes" : "No");
                logger.LogDebug("User ID: {UserId}", authenticatedUser.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown");
            }
        }
        if (authenticatedUser.HasClaim(ClaimTypes.Role, RoleConstants.EXTERNAL_LOGIN) && !await IsGCCFEnabled())
            throw new InvalidOperationException("External users are not allowed when GCCF is disabled");

        return authenticatedUser;
    }

    private async Task<bool> IsGCCFEnabled()
    {
        return await featureManager.IsEnabledAsync(Features.GCCF_Feature);
    }

    public async Task<string?> GetCurrentUserEntraId()
    {
        await CheckUser();
        return GetEntraOid();
    }

    /// <summary>
    /// Gets the current authenticated user's subject claim ("sub" or NameIdentifier) from the authentication claims.
    /// Returns null when the claim is not present or the user is not authenticated.
    /// </summary>
    public async Task<string?> GetCurrentUserNameIdentifier()
    {
        var user = await GetAuthenticatedUser();
        if (user == null) return null;
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId;
    }

    public async Task<string> GetUserEmail()
    {
        await CheckUser();
        return currentEntraUser.Mail ?? throw new InvalidOperationException("Email is not available for current user");
    }

    public async Task<string> GetDisplayName()
    {
        await CheckUser();
        return currentEntraUser.DisplayName ?? string.Empty;
    }

    public async Task<string> GetUserEmailDomain()
    {
        try
        {
            MailAddress email = new MailAddress(await GetUserEmail());
            return email.Host.ToLower();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cannot parse email from {CurrentUserMail}", currentEntraUser?.Mail);
            return "?";
        }
    }

    public async Task<string> GetUserEmailPrefix()
    {
        try
        {
            var email = new MailAddress(await GetUserEmail());
            return email.User.ToLower();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cannot parse email from {CurrentUserMail}", currentEntraUser?.Mail);
            return "?";
        }
    }

    public async Task<string> GetUserRootFolder()
    {
        var domain = await GetUserEmailDomain();
        var prefix = await GetUserEmailPrefix();
        return $"{domain}/{prefix}";
    }

    public async Task<bool> IsUserWithoutWorkspaces()
    {
        if (_isViewingAsVisitor)
            return true;
        var claims = (await GetAuthenticatedUser()).Claims.Where(c => c.Type == ClaimTypes.Role).ToList();

        return !claims.Any() || (claims.Count == 1 && claims[0].Value == "default");
    }

    private async Task LoadUserAsyncInternal()
    {
        if (currentEntraUser != null) return;
        var authenticatedUser = await GetAuthenticatedUser();
        if (authenticatedUser.HasClaim(ClaimTypes.Role, RoleConstants.EXTERNAL_LOGIN))
        {
            // Throw501 (Not Implemented) if external login
            throw new HttpRequestException("External login is not allowed", null, System.Net.HttpStatusCode.NotImplemented);
        }
        try
        {
            var email = authenticatedUser?.Identity?.Name;
            var userId = GetEntraOid();
            if (email is null)
            {
                throw new InvalidOperationException("Cannot resolve user email");
            }

            PrepareAuthenticatedClient();
            currentEntraUser = await graphServiceClient.Users[userId].GetAsync() ?? throw new InvalidOperationException("Cannot retrieve user from graph");
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

    private bool HasEntraOid()
    {
        return authenticatedUser?.Claims?
                   .Any(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier") ?? false;
    }

    private string GetEntraOid()
    {
        // ReSharper disable once ConstantConditionalAccessQualifier
        return (authenticatedUser?.Claims?
                    .FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier") ??
                throw new InvalidOperationException("Cannot access user claims")).Value;
    }

    public async Task<User> GetCurrentGraphUserAsync()
    {
        await CheckUser();
        return currentEntraUser;
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
                configureOptions.GetSection("AzureAd").GetValue<string>("TenantId"),
                configureOptions.GetSection("AzureAd").GetValue<string>("ClientId"),
                configureOptions.GetSection("AzureAd").GetValue<string>("ClientSecret"), options);
            graphServiceClient = new(clientCertCredential);
        }
        catch (Exception e)
        {
            logger.LogError($"Error preparing authentication client: {e.Message}");
            Console.WriteLine($"Error preparing authentication client: {e.Message}");
            throw;
        }
    }

    private async Task CheckUser()
    {
        if (currentEntraUser == null)
        {
            await LoadUserAsyncInternal();
        }
    }

    public Task<User> GetAnonymousGraphUserAsync()
    {
        return Task.FromResult(AnonymousUser);
    }

    public async Task<User?> GetGraphUserAsync(string userId)
    {
        try
        {
            PrepareAuthenticatedClient();
            currentEntraUser = await graphServiceClient.Users[userId].GetAsync() ?? throw new InvalidOperationException("Cannot retrieve user from graph");

            return currentEntraUser;
        }
        catch (ServiceException e)
        {
            if (e.InnerException is MsalUiRequiredException ||
                e.InnerException is MicrosoftIdentityWebChallengeUserException)
                throw;
            logger.LogError(e, "Error Loading User");
            return null;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error Loading User");
            return null;
        }
    }

    public async Task<bool> IsViewingAsGuest()
    {
        return serviceAuthManager.GetViewingAsGuest((await GetCurrentGraphUserAsync()).Id ?? throw new InvalidOperationException("Cannot access graph user Id"));
    }

    public async Task SetViewingAsGuest(bool isGuest)
    {
        serviceAuthManager.SetViewingAsGuest((await GetCurrentGraphUserAsync()).Id ?? throw new InvalidOperationException("Cannot access graph user Id"), isGuest);
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

        return (await IsUserProjectAdmin(projectAcronym)) ||
               (await GetAuthenticatedUser()).IsInRole($"{projectAcronym}");
    }

    /// <summary>
    /// Creates a new portal user if one does not exist
    /// </summary>
    /// <param name="userGraphId"></param>
    /// <returns>Portal User</returns>
    public async Task<PortalUser?> CreatePortalEntraUserAsync(string userGraphId)
    {
        await using var ctx = await datahubContextFactory.CreateDbContextAsync();
        var exists = await ctx.EntraUsers
            .FirstOrDefaultAsync(p => p.GraphGuid == userGraphId);

        if (exists is not null)
        {
            logger.LogInformation("User with GraphId: {GraphId} already exists", userGraphId);
            return null;
        }

        try
        {
            PrepareAuthenticatedClient();
            var graphUser = await graphServiceClient.Users[userGraphId].GetAsync() ?? throw new InvalidOperationException("Cannot retrieve user from graph");
            var portalUser = new PortalUser
            {
                EntraUser = new EntraUser
                {
                    GraphGuid = userGraphId,
                    PortalUser = null!,
                },
                Email = graphUser.Mail,
                DisplayName = graphUser.DisplayName,
            };

            ctx.PortalUsers.Add(portalUser);
            await ctx.SaveChangesAsync();
            logger.LogInformation("Created new Portal User with GraphId: {GraphId}", userGraphId);

            var catalogObject = new Core.Model.Catalog.CatalogObject()
            {
                ObjectType = Core.Model.Catalog.CatalogObjectType.User,
                ObjectId = userGraphId,
                Name_English = graphUser.DisplayName,
                Name_French = graphUser.DisplayName,
                Desc_English = graphUser.Department,
                Desc_French = graphUser.Department
            };

            await datahubCatalogSearch.AddCatalogObject(catalogObject);
            await userEnrollmentService.InviteUserToGroup(userGraphId);
            return portalUser;
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error Loading User from Graph with GraphId: {GraphId}. It's possible they no longer exist",
                userGraphId);
            return null;
        }
    }

    private async Task UpdatePortalUserLastLogin(string userGraphId)
    {
        await using var ctx = await datahubContextFactory.CreateDbContextAsync();
        var entraUser = await ctx.EntraUsers.Include(p => p.PortalUser).FirstOrDefaultAsync(p => p.GraphGuid == userGraphId);

        if (entraUser is not null)
        {
            entraUser.PortalUser.LastLoginDateTime = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
        }
        else
        {
            logger.LogWarning("User with GraphId: {GraphId} does not exist", userGraphId);
        }
    }

    private async Task UpdatePortalUserFirstLogin(string userGraphId)
    {
        await using var ctx = await datahubContextFactory.CreateDbContextAsync();
        var entraUser = await ctx.EntraUsers.Include(p => p.PortalUser).FirstOrDefaultAsync(p => p.GraphGuid == userGraphId);

        if (entraUser is not null)
        {
            entraUser.PortalUser.FirstLoginDateTime = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
        }
        else
        {
            logger.LogWarning("User with GraphId: {GraphId} does not exist", userGraphId);
        }
    }

    public async Task<ExtendedPortalUser?> GetUserByEmailAsync(string email)
    {
        await using var ctx = await datahubContextFactory.CreateDbContextAsync();
        var matchingUsers = await ctx.PortalUsers
            .Include(u => u.EntraUser)
            .AsNoTracking()
            .Where(p => p.EntraUser != null && p.Email != null && p.Email.ToLower() == email.ToLower())
            .ToListAsync();
        List<ExtendedPortalUser> extendedUsers = [];

        PrepareAuthenticatedClient();

        // Check the state of each matching user accounts
        foreach (var portalUser in matchingUsers)
        {
            var extendedPortalUser = new ExtendedPortalUser(portalUser);
            if (extendedPortalUser == null)
            {
                logger.LogError("Unable to cast portalUser to ExtendedPortalUser");
                throw new InvalidCastException("The portal user is not of type ExtendedPortalUser.");
            }

            try
            {
                logger.LogInformation("Making MS graph request...");
                var graphUser = await graphServiceClient.Users[portalUser.EntraUser!.GraphGuid].GetAsync(
                    request => request.QueryParameters.Select = ["accountEnabled"]);
                if (graphUser is not null)
                {
                    extendedPortalUser.IsLocked =
                        graphUser.AccountEnabled.HasValue && !graphUser.AccountEnabled.Value;
                    logger.LogInformation("Found user. Account enabled: {AccountEnabled}",
                        !extendedPortalUser.IsLocked);
                }
            }
            catch (ODataError e)
            {
                if (e.ResponseStatusCode == 404)
                {
                    logger.LogWarning("User with provided GraphGUID not found. User account was deleted");
                    extendedPortalUser.IsDeleted = true;
                }
                else
                {
                    logger.LogError(e, "Unexpected error occurred");
                    throw;
                }
            }
            catch (ServiceException e)
            {
                if (e.InnerException is MsalUiRequiredException ||
                    e.InnerException is MicrosoftIdentityWebChallengeUserException)
                {
                    logger.LogError(e, "Unexpected error occurred");
                    throw;
                }

                logger.LogWarning(e, "Error loading user, marking user as deleted");
                extendedPortalUser.IsDeleted = true;
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Error loading user, marking user as deleted");
                extendedPortalUser.IsDeleted = true;
            }
            finally
            {
                extendedUsers.Add(extendedPortalUser);
            }
        }

        // If there is no matching accounts, return null
        if (extendedUsers.Count == 0)
        {
            return null;
        }

        // If all matching accounts were deleted, return the one with the latest sign-in
        if (extendedUsers.All(u => u.IsDeleted))
        {
            return extendedUsers.OrderByDescending(u => u.LastLoginDateTime).First();
        }

        // If one or more of the accounts is not deleted, return the one with the latest sign-in
        return extendedUsers.Where(u => !u.IsDeleted).OrderByDescending(u => u.LastLoginDateTime).First();
    }

    public async Task HandleDeletedEntraUserRegistration(string email, string graphId, int portalUserId)
    {
        // update portal user with new graph id
        await using var ctx = await datahubContextFactory.CreateDbContextAsync();
        var portalUser = await ctx.PortalUsers.Include(u => u.EntraUser).FirstAsync(p => p.EntraUser != null && p.Id == portalUserId);
        portalUser.EntraUser!.GraphGuid = graphId;
        ctx.Update(portalUser);
        await ctx.SaveChangesAsync();
    }

    public async Task RegisterAuthenticatedPortalUser()
    {
        var graphId = await GetCurrentUserEntraId();

        var portalUser = await GetEntraUserAsync(graphId);
        if (portalUser is null)
        {
            await CreatePortalEntraUserAsync(graphId);
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
            await using var ctx = await datahubContextFactory.CreateDbContextAsync();

            ctx.PortalUsers.Attach(updatedUser);
            ctx.Entry(updatedUser).State = EntityState.Modified;
            if (updatedUser.UserSettings is not null)
            {
                ctx.Entry(updatedUser.UserSettings).State = EntityState.Modified;
            }
            await ctx.SaveChangesAsync();
            PortalUserUpdated?.Invoke(this, new PortalUserUpdatedEventArgs(updatedUser));
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating portal user");
        }
        return false;
    }

    public async Task<PortalUser?> GetCurrentPortalUserAsync()
    {
        if (!HasEntraOid())
        {
            return null;
        }
        var graphId = await GetCurrentUserEntraId();
        return await GetEntraUserAsync(graphId);
    }

    public async Task<PortalUser> GetEntraUserAsync(string userGraphId)
    {
        PortalUser? portalUser;
        await using (var ctx = await datahubContextFactory.CreateDbContextAsync())
        {
            portalUser = await ctx.PortalUsers
                .AsNoTracking()
                .Include(u => u.UserSettings)
                .Include(u => u.EntraUser)
                .FirstOrDefaultAsync(p => p.EntraUser != null && p.EntraUser.GraphGuid == userGraphId);

            if (portalUser is not null)
            {
                return portalUser;
            }
        }

        logger.LogInformation("User with GraphId: {GraphId} does not exist", userGraphId);
        return await CreatePortalEntraUserAsync(userGraphId) ?? throw new InvalidOperationException("Failed to create portal user");
    }

    public async Task<bool> IsDailyLogin()
    {
        var portalUser = await GetCurrentPortalUserAsync();

        if (portalUser is null)
            return false;

        var isFirstLoginDay = portalUser.FirstLoginDateTime.GetValueOrDefault().Date == DateTime.UtcNow.Date;
        var isLastLoginDay = portalUser.LastLoginDateTime.GetValueOrDefault().Date == DateTime.UtcNow.Date;

        return !isFirstLoginDay && !isLastLoginDay;
    }

    public async Task<PortalUser> GetCurrentPortalUserWithAchievementsAsync()
    {
        if (_userWithAchievements != null)
            return _userWithAchievements;
        _userWithAchievements = await LoadUserWithAchievementsAsync(await GetCurrentUserEntraId(), await GetCurrentUserNameIdentifier());

        return _userWithAchievements;
    }

    private async Task<PortalUser> LoadUserWithAchievementsAsync(string? entraId = null, string? userOID = null)
    {
        if (entraId is null && userOID is null)
            throw new ArgumentException("Either entraId or userOID must be provided");
        await using var ctx = await datahubContextFactory.CreateDbContextAsync();

        var query = ctx.PortalUsers
            .AsNoTracking()
            .Include(p => p.EntraUser)
            .Include(p => p.UserSettings)
            .Include(p => p.Achievements)
            .ThenInclude(a => a.Achievement)
            .AsSingleQuery();

        PortalUser portalUser = null!;
        if (entraId is not null)
        {
            portalUser = await query.FirstAsync(p => p.EntraUser != null && p.EntraUser.GraphGuid == entraId);
        } else if (userOID is not null)
        {
            portalUser = await query.FirstAsync(p => p.ExternalUser != null && p.ExternalUser.ExternalSubject == userOID);
        }
        return portalUser;
    }

    public async Task<PortalUser> GetEntraUserWithAchievementsAsync(string userGraphId)
    {
        if (userGraphId == (await GetCurrentUserEntraId()))
            return await GetCurrentPortalUserWithAchievementsAsync();
        return await LoadUserWithAchievementsAsync(userGraphId);
    }

    public async Task<bool> CheckUserInTenant(string email)
    {
        PrepareAuthenticatedClient();
        var users = await graphServiceClient.Users.GetAsync(
            request => request.QueryParameters.Filter = $"mail eq '{email}'");
        if (users?.Value != null) return users.Value.Count > 0;
        return false;
    }

    public async Task<PortalUser?> CreatePortalExternalUserAsync(string userOid, string first, string last, string org, string email, DateTimeOffset expiry)
    {
        await using var ctx = await datahubContextFactory.CreateDbContextAsync();
        var exists = await ctx.ExternalUsers
            .FirstOrDefaultAsync(p => p.ExternalSubject == userOid);

        if (exists is not null)
        {
            logger.LogInformation("External user with OID: {Oid} already exists", userOid);
            return null;
        }

        try
        {
            PrepareAuthenticatedClient();
            var displayName = $"{first} {last}";
            var portalUser = new PortalUser
            {
                ExternalUser = new ExternalUser
                {
                    ExternalSubject = userOid,
                    PortalUser = null!,
                    FirstName = first,
                    LastName = last,
                    Organization = org,
                    UserExpiryDate = expiry,
                },
                Email = email,
                DisplayName = displayName,
            };

            ctx.PortalUsers.Add(portalUser);
            await ctx.SaveChangesAsync();
            logger.LogInformation("Created new External Portal User with OID: {Oid}", userOid);

            var catalogObject = new Core.Model.Catalog.CatalogObject()
            {
                ObjectType = Core.Model.Catalog.CatalogObjectType.User,
                ObjectId = userOid.ToString(),
                Name_English = displayName,
                Name_French = displayName,
                Desc_English = "External User",
                Desc_French = "Utilisateur externe"
            };

            await datahubCatalogSearch.AddCatalogObject(catalogObject);
            return portalUser;
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error Loading External User from Graph with OID: {Oid}. It's possible they no longer exist",
                userOid);
            return null;
        }
    }
}
