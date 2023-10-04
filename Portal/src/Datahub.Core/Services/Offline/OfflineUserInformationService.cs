using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Services.UserManagement;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace Datahub.Core.Services.Offline;

public class OfflineUserInformationService : IUserInformationService
{
    private static readonly Guid UserGuid = new Guid(); 

    readonly ILogger<IUserInformationService> _logger;

    private User AnonymousUser => UserInformationServiceConstants.GetAnonymousUser();

    public OfflineUserInformationService(ILogger<IUserInformationService> logger)
    {
        _logger = logger;
    }


    public Task<bool> ClearUserSettingsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<User> GetCurrentGraphUserAsync()
    {
        return Task.FromResult(new User
        {
            DisplayName = "Offline User",
            Id = UserGuid.ToString(),
            UserPrincipalName = "me@me.com",
            Mail = "nabeel.bader@nrcan-rncan.gc.ca"
        });
    }

    public Task<string> GetDisplayName()
    {
        return Task.FromResult("Me");
    }

    public Task<string> GetUserEmail()
    {
        return Task.FromResult("me@me.com");
    }

    public Task<string> GetUserEmailDomain()
    {
        return Task.FromResult("me@me.com");
    }

    public Task<string> GetUserEmailPrefix()
    {
        return Task.FromResult("");
    }

    public Task<string> GetUserIdString()
    {
        return Task.FromResult(UserGuid.ToString());
    }

    public Task<string> GetUserLanguage()
    {
        return Task.FromResult("en-CA");
    }

    public Task<string> GetUserRootFolder()
    {
        return Task.FromResult("/");
    }

    public Task<bool> HasUserAcceptedTAC()
    {
        return Task.FromResult(true);
    }

    public Task<bool> IsFrench()
    {
        return Task.FromResult(false);
    }

    public Task<bool> RegisterUserTAC()
    {
        return Task.FromResult(true);
    }

    public Task<bool> RegisterUserLanguage(string language)
    {
        return Task.FromResult(true);
    }
        
    public bool SetLanguage(string language)
    {
        return true;
    }

    public Task<PortalUser> GetPortalUserWithAchievementsAsync(string userGraphId)
    {
        throw new NotImplementedException();
    }

    public Task<User> GetAnonymousGraphUserAsync()
    {
        return Task.FromResult(AnonymousUser);
    }

    public Task<string> GetAzureUserID()
    {
        return Task.FromResult(UserGuid.ToString());
    }

    public Task<User> GetGraphUserAsync(string userId)
    {
        return Task.FromResult(new User
        {
            DisplayName = "Offline User random",
            Id = userId,
            UserPrincipalName = "me@me.com"
        });
    }

    public Task<PortalUser> GetCurrentPortalUserAsync()
    {
        throw new NotImplementedException();
    }

    public Task<PortalUser> GetPortalUserAsync(string userGraphId)
    {
        throw new NotImplementedException();
    }

    public Task<PortalUser> GetCurrentPortalUserWithAchievementsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsUserWithoutInitiatives()
    {
        return Task.FromResult(false);
    }

    public Task<bool> IsViewingAsGuest()
    {
        return Task.FromResult(false);
    }

    public Task SetViewingAsGuest(bool isGuest)
    {
        // do nothing
        return Task.Delay(0);
    }

    public Task<bool> IsViewingAsVisitor()
    {
        return Task.FromResult(false);
    }

    public Task SetViewingAsVisitor(bool isVisitor)
    {
        return Task.CompletedTask;
    }

    public Task<ClaimsPrincipal> GetAuthenticatedUser(bool forceReload = false)
    {
        return Task.FromResult(new ClaimsPrincipal());
    }

    public Task<bool> IsUserProjectAdmin(string projectAcronym)
    {
        return Task.FromResult(false);
    }

    public Task<bool> IsUserProjectWorkspaceLead(string projectAcronym)
    {
        return Task.FromResult(false);
    }

    public Task<bool> IsUserDatahubAdmin()
    {
        return Task.FromResult(false);
    }

    public Task<bool> IsUserProjectMember(string projectAcronym)
    {
        return Task.FromResult(false);
    }

    public Task RegisterAuthenticatedPortalUser()
    {
        return Task.FromResult(true);
    }

    public Task CreatePortalUserAsync(string userGraphId)
    {
        throw new NotImplementedException();
    }

    public Task<PortalUser> GetAuthenticatedPortalUser()
    {
        return Task.FromResult(new PortalUser(){GraphGuid = AnonymousUser.Id});
    }
    
    public Task<bool> UpdatePortalUserAsync(PortalUser updatedUser)
    {
        PortalUserUpdated?.Invoke(this, new PortalUserUpdatedEventArgs(updatedUser));
        throw new NotImplementedException();
    }
    public event EventHandler<PortalUserUpdatedEventArgs> PortalUserUpdated;
    public Task<bool> IsDailyLogin()
    {
        return Task.FromResult(false);
    }
}