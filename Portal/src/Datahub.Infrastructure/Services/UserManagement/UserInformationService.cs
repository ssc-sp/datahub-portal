using System.ComponentModel;
using System.Net.Mail;
using System.Security.Claims;
using Azure.Identity;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Core.Services.UserManagement;
using J2N.Numerics;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;

namespace Datahub.Infrastructure.Services.UserManagement;

public class UserInformationService(
    ILogger<UserInformationService> logger,
    AuthenticationStateProvider authenticationStateProvider,
    IConfiguration configureOptions,
    IServiceAuthManager serviceAuthManager,
    IDatahubCatalogSearch datahubCatalogSearch,
    IDbContextFactory<DatahubProjectDBContext> datahubContextFactory)
    : IUserInformationService
{
    private GraphServiceClient graphServiceClient;
    private ClaimsPrincipal authenticatedUser;
    public event EventHandler<PortalUserUpdatedEventArgs> PortalUserUpdated;
    private User currentUser;
    private static User AnonymousUser => UserInformationServiceConstants.GetAnonymousUser();
    private bool _isViewingAsVisitor;

    public async Task<ClaimsPrincipal> GetAuthenticatedUser(bool forceReload = false)
    {
        if (authenticationStateProvider == null || forceReload)
            authenticatedUser = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;
        return authenticatedUser;
    }

    public async Task<string> GetUserIdString()
    {
        await CheckUser();
        return GetOid();
    }

    public async Task<string> GetUserEmail()
    {
        await CheckUser();
        return currentUser.Mail;
    }

    public async Task<string> GetDisplayName()
    {
        await CheckUser();
        return currentUser.DisplayName;
    }

    public async Task<string> GetUserEmailDomain()
    {
        await CheckUser();
        try
        {
            MailAddress email = new MailAddress(currentUser.Mail);
            return email.Host.ToLower();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cannot parse email from {CurrentUserMail}", currentUser?.Mail);
            return "?";
        }
    }

    public async Task<string> GetUserEmailPrefix()
    {
        await CheckUser();
        try
        {
            var email = new MailAddress(currentUser.Mail);
            return email.User.ToLower();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cannot parse email from {CurrentUserMail}", currentUser?.Mail);
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

        return !claims.Any() || (claims.Count == 1 && claims[0].Value == "default");
    }

    private async Task GetUserAsyncInternal()
    {
        if (currentUser != null) return;
        try
        {
            var email = (await GetAuthenticatedUser())?.Identity?.Name;
            var userId = GetOid();
            if (email is null)
            {
                throw new InvalidOperationException("Cannot resolve user email");
            }

            PrepareAuthenticatedClient();
            currentUser = await graphServiceClient.Users[userId].GetAsync();
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
        return (authenticatedUser?.Claims?
                    .FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier") ??
                throw new InvalidOperationException("Cannot access user claims")).Value;
    }

    public async Task<User> GetCurrentGraphUserAsync()
    {
        await CheckUser();
        return currentUser;
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
        if (currentUser == null)
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
            currentUser = await graphServiceClient.Users[userId].GetAsync();

            return currentUser;
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
        return serviceAuthManager.GetViewingAsGuest((await GetCurrentGraphUserAsync()).Id);
    }

    public async Task SetViewingAsGuest(bool isGuest)
    {
        serviceAuthManager.SetViewingAsGuest((await GetCurrentGraphUserAsync()).Id, isGuest);
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
    public async Task CreatePortalUserAsync(string userGraphId)
    {
        await using var ctx = await datahubContextFactory.CreateDbContextAsync();
        var exists = await ctx.PortalUsers
            .FirstOrDefaultAsync(p => p.GraphGuid == userGraphId);

        if (exists is not null)
        {
            logger.LogInformation("User with GraphId: {GraphId} already exists", userGraphId);
            return;
        }

        try
        {
            var graphUser = await graphServiceClient.Users[userGraphId].GetAsync();
            var portalUser = new PortalUser
            {
                GraphGuid = userGraphId,
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
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error Loading User from Graph with GraphId: {GraphId}. It's possible they no longer exist",
                userGraphId);
        }
    }

    private async Task UpdatePortalUserLastLogin(string userGraphId)
    {
        await using var ctx = await datahubContextFactory.CreateDbContextAsync();
        var portalUser = await ctx.PortalUsers.FirstOrDefaultAsync(p => p.GraphGuid == userGraphId);

        if (portalUser is not null)
        {
            portalUser.LastLoginDateTime = DateTime.UtcNow;
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
        var portalUser = await ctx.PortalUsers.FirstOrDefaultAsync(p => p.GraphGuid == userGraphId);

        if (portalUser is not null)
        {
            portalUser.FirstLoginDateTime = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
        }
        else
        {
            logger.LogWarning("User with GraphId: {GraphId} does not exist", userGraphId);
        }
    }

    public async Task<ExtendedPortalUser?> GetPortalUserByEmailAsync(string email)
    {
        ExtendedPortalUser? extendedPortalUser = new ExtendedPortalUser { Email=email };
        PortalUser? portalUser;
        await using (var ctx = await datahubContextFactory.CreateDbContextAsync())
        {
            portalUser = await ctx.PortalUsers
                .AsNoTracking()
                .Include(u => u.UserSettings)
                .FirstOrDefaultAsync(p => p.Email.ToLower() == email.ToLower());
            if (portalUser is not null)
            {
                try
                {
                    extendedPortalUser = new ExtendedPortalUser(portalUser);
                    if (extendedPortalUser == null)
                    {
                        throw new InvalidCastException("The portal user is not of type ExtendedPortalUser.");
                    }
                    PrepareAuthenticatedClient();
                    var graphUser = await graphServiceClient.Users[portalUser.GraphGuid].GetAsync();

                    if (graphUser == null)
                    {
                        extendedPortalUser.IsDeleted = true;
                    }
                    else
                    {
                        // Check if the user is locked out
                        extendedPortalUser.IsLocked = graphUser.AccountEnabled.HasValue && !graphUser.AccountEnabled.Value;
                    }
                }
                catch (ServiceException e)
                {
                    if (e.InnerException is MsalUiRequiredException ||
                        e.InnerException is MicrosoftIdentityWebChallengeUserException)
                        throw;

                    logger.LogError(e, "Error Loading User");
                    extendedPortalUser.IsDeleted = true;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error Loading User");
                    extendedPortalUser.IsDeleted = true;
                }
                return extendedPortalUser;
            }
            return null;
        }
    }

    public async Task HandleDeletedUserRegistration(string email, string graphId)
    {
        // update portal user with new graph id
        await using (var ctx = await datahubContextFactory.CreateDbContextAsync())
        {
            var portalUser = await ctx.PortalUsers.FirstOrDefaultAsync(p => p.Email == email);
            if (portalUser != null)
            {
                portalUser.GraphGuid = graphId;
                ctx.Update(portalUser);
            }
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
            await using var ctx = await datahubContextFactory.CreateDbContextAsync();

            ctx.PortalUsers.Attach(updatedUser);
            ctx.Entry(updatedUser).State = EntityState.Modified;
            ctx.Entry(updatedUser.UserSettings).State = EntityState.Modified;
            await ctx.SaveChangesAsync();
            PortalUserUpdated?.Invoke(this, new PortalUserUpdatedEventArgs(updatedUser));
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating portal user");
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
        await using (var ctx = await datahubContextFactory.CreateDbContextAsync())
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

        logger.LogInformation("User with GraphId: {GraphId} does not exist", userGraphId);
        await CreatePortalUserAsync(userGraphId);

        await using (var ctx = await datahubContextFactory.CreateDbContextAsync())
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
        await using var ctx = await datahubContextFactory.CreateDbContextAsync();

        var portalUser = await ctx.PortalUsers
            .AsNoTracking()
            .Include(p => p.UserSettings)
            .Include(p => p.Achievements)
            .ThenInclude(a => a.Achievement)
            .FirstOrDefaultAsync(p => p.GraphGuid == userGraphId);

        return portalUser;
    }
}