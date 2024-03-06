﻿using System.Security.Claims;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Services.UserManagement;
using Microsoft.Graph.Models;

namespace Datahub.Core.Services;

public interface IUserInformationService
{
    Task<User> GetCurrentGraphUserAsync();
    Task<User> GetGraphUserAsync(string userId);

    /// <summary>
    /// Gets the current portal user asynchronously. Will contain the <see cref="UserSettings"/> object.
    /// </summary>
    /// <returns>The current portal user.</returns>
    Task<PortalUser> GetCurrentPortalUserAsync();

    Task<PortalUser> GetPortalUserAsync(string userGraphId);

    Task<PortalUser> GetCurrentPortalUserWithAchievementsAsync();
    Task<PortalUser> GetPortalUserWithAchievementsAsync(string userGraphId);

    Task<User> GetAnonymousGraphUserAsync();
    Task<string> GetUserIdString();
    Task<string> GetDisplayName();
    Task<string> GetUserEmail();
    Task<string> GetUserEmailDomain();
    Task<string> GetUserEmailPrefix();
    Task<string> GetUserRootFolder();
    Task<bool> IsUserWithoutInitiatives();
    Task<bool> IsViewingAsGuest();
    Task<bool> IsViewingAsVisitor();
    Task SetViewingAsGuest(bool isGuest);
    Task SetViewingAsVisitor(bool isVisitor);
    Task<ClaimsPrincipal> GetAuthenticatedUser(bool forceReload = false);
    Task<bool> IsUserProjectAdmin(string projectAcronym);
    Task<bool> IsUserProjectWorkspaceLead(string projectAcronym);
    Task<bool> IsUserProjectMember(string projectAcronym);
    Task<bool> IsUserDatahubAdmin();
    Task RegisterAuthenticatedPortalUser();
    public Task CreatePortalUserAsync(string userGraphId);
    Task<bool> UpdatePortalUserAsync(PortalUser updatedUser);
    public event EventHandler<PortalUserUpdatedEventArgs> PortalUserUpdated;
    Task<bool> IsDailyLogin();
}

public static class UserInformationServiceConstants
{
    public static readonly string ANONYMOUSUSERID = "c90acba3-26e4-471d-bbdf-544906e6a980";
    public static readonly string ANONYMOUSUSERNAME = "Anonymous User";
    public static readonly string ANONYMOUSUSEREMAIL = "anyone@example.com";

    private static User anonymousUser;
    public static User GetAnonymousUser()
    {
        if (anonymousUser == null)
        {
            anonymousUser = new User()
            {
                Id = ANONYMOUSUSERID,
                Mail = ANONYMOUSUSEREMAIL,
                DisplayName = ANONYMOUSUSERNAME,
                UserPrincipalName = ANONYMOUSUSEREMAIL
            };
        }
        return anonymousUser;
    }
}