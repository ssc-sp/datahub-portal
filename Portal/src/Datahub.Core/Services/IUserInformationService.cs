using System;
using Datahub.Core.Model.Achievements;
using Microsoft.Graph;
using System.Security.Claims;
using System.Threading.Tasks;
using Datahub.Core.Services.UserManagement;

namespace Datahub.Core.Services;

public interface IUserInformationService
{
    Task<bool> ClearUserSettingsAsync();
    [Obsolete("Use GetCurrentPortalUserAsync instead")]
    Task<User> GetCurrentGraphUserAsync();
    [Obsolete("Use GetPortalUserAsync instead")]
    Task<User> GetGraphUserAsync(string userId);
    Task<PortalUser> GetCurrentPortalUserAsync();
    Task<PortalUser> GetPortalUserAsync(string userGraphId);
    
    Task<PortalUser> GetCurrentPortalUserWithAchievementsAsync();
    Task<PortalUser> GetPortalUserWithAchievementsAsync(string userGraphId);
    
    [Obsolete("Use GetAnonymousPortalUserAsync instead")]
    Task<User> GetAnonymousGraphUserAsync();
    [Obsolete("Use GetCurrentPortalUserAsync instead to get the user's id")]
    Task<string> GetUserIdString();
    [Obsolete("Use GetCurrentPortalUserAsync instead to check if the user has accepted the TAC")]
    Task<bool> HasUserAcceptedTAC();
    Task<bool> RegisterUserTAC();
    [Obsolete("Use SetLanguage instead")]
    Task<bool> RegisterUserLanguage(string language);
    [Obsolete("Use GetCurrentPortalUserAsync instead to get the user's language")]
    Task<string> GetUserLanguage();
    [Obsolete("Use GetCurrentPortalUserAsync instead to get the user's language")]
    Task<bool> IsFrench();
    [Obsolete("Use GetCurrentPortalUserAsync")]
    Task<string> GetDisplayName();
    [Obsolete("Use GetCurrentPortalUserAsync")]
    Task<string> GetUserEmail();
    [Obsolete("Use GetCurrentPortalUserAsync")]
    Task<string> GetUserEmailDomain();
    [Obsolete("Use GetCurrentPortalUserAsync")]
    Task<string> GetUserEmailPrefix();
    bool SetLanguage(string language);
    [Obsolete("This feature is no longer supported")]
    Task<string> GetUserRootFolder();
    [Obsolete("Use GetCurrentPortalUserAsync")]
    Task<bool> IsUserWithoutInitiatives();
    Task<bool> IsViewingAsGuest();
    Task<bool> IsViewingAsVisitor();
    Task SetViewingAsGuest(bool isGuest);
    Task SetViewingAsVisitor(bool isVisitor);
    Task<ClaimsPrincipal> GetAuthenticatedUser(bool forceReload = false);
    Task<bool> IsUserProjectAdmin(string projectAcronym);

    Task<bool> IsUserProjectMember(string projectAcronym);

    Task<bool> IsUserDatahubAdmin();

    Task RegisterAuthenticatedPortalUser();
    Task<bool> UpdatePortalUserAsync(PortalUser updatedUser);
    public event EventHandler<PortalUserUpdatedEventArgs> PortalUserUpdated;
    Task<bool> IsDailyLogin();
}

public static class UserInformationServiceConstants
{
    public static readonly string ANONYMOUS_USER_ID = "c90acba3-26e4-471d-bbdf-544906e6a980";
    public static readonly string ANONYMOUS_USER_NAME = "Anonymous User";
    public static readonly string ANONYMOUS_USER_EMAIL = "anyone@example.com";

    private static User _anonymousUser;
    public static User GetAnonymousUser()
    {
        if (_anonymousUser == null)
        {
            _anonymousUser = new User()
            {
                Id = ANONYMOUS_USER_ID,
                Mail = ANONYMOUS_USER_EMAIL,
                DisplayName = ANONYMOUS_USER_NAME,
                UserPrincipalName = ANONYMOUS_USER_EMAIL
            };
        }
        return _anonymousUser;
    }
}