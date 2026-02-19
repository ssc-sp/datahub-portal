using System.Security.Claims;
using Datahub.Core.Model.Users;
using Datahub.Core.Services.UserManagement;
using Microsoft.Graph.Models;

namespace Datahub.Application.Services.UserManagement;

/// <summary>
/// Provides methods for retrieving and managing user information from Microsoft Graph and the portal database.
/// Implementations must handle graph calls, portal user creation, and user state queries used by the application.
/// </summary>
public interface IUserInformationService
{
    /// <summary>
    /// Gets the currently signed-in Azure AD graph user (Microsoft Graph <see cref="User"/>).
    /// </summary>
    /// <returns>A <see cref="User"/> representing the current Graph user.</returns>
    Task<User> GetCurrentGraphUserAsync();

    /// <summary>
    /// Retrieves a Microsoft Graph user by their graph id.
    /// </summary>
    /// <param name="userId">The Graph user id (object id) to lookup.</param>
    /// <returns>The <see cref="User"/> if found; otherwise <c>null</c>.</returns>
    Task<User?> GetGraphUserAsync(string userId);

    /// <summary>
    /// Gets the current portal user if one exists. The returned <see cref="PortalUser"/> will include
    /// related <see cref="UserSettings"/> when available.
    /// </summary>
    /// <returns>The current <see cref="PortalUser"/>, or <c>null</c> when no user is authenticated.</returns>
    Task<PortalUser?> GetCurrentPortalUserAsync();

    /// <summary>
    /// Retrieves a portal user associated with an Entra (Azure AD) Graph user id. If the portal user does not exist,
    /// implementations will create a corresponding portal user record.
    /// </summary>
    /// <param name="userGraphId">The Azure AD object id (Graph id) of the user.</param>
    /// <returns>The matching <see cref="PortalUser"/> instance.</returns>
    Task<PortalUser> GetEntraUserAsync(string userGraphId);

    /// <summary>
    /// Finds an extended view of a portal user by email. The returned <see cref="ExtendedPortalUser"/>
    /// contains additional flags such as <see cref="ExtendedPortalUser.IsDeleted"/> or <see cref="ExtendedPortalUser.IsLocked"/>.
    /// </summary>
    /// <param name="email">The user's email address to search for.</param>
    /// <returns>An <see cref="ExtendedPortalUser"/> or <c>null</c> if no match was found.</returns>
    Task<ExtendedPortalUser?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Handles updating portal state when an Entra user registration collision was caused by a deleted account.
    /// This typically updates an existing portal user record with a new graph id and other bookkeeping fields.
    /// </summary>
    /// <param name="email">The email of the user.</param>
    /// <param name="graphId">The new graph id for the portal user.</param>
    /// <param name="portalUserId">The portal user primary key id to update.</param>
    Task HandleDeletedEntraUserRegistration(string email, string graphId, int portalUserId);

    /// <summary>
    /// Gets the current portal user including achievements information.
    /// </summary>
    /// <returns>The current portal user's <see cref="PortalUser"/> with achievements loaded.</returns>
    Task<PortalUser> GetCurrentPortalUserWithAchievementsAsync();

    /// <summary>
    /// Loads a portal user (including achievements) for the specified graph user id.
    /// </summary>
    /// <param name="userGraphId">The Azure AD graph id for the user.</param>
    /// <returns>The portal user with achievements.</returns>
    Task<PortalUser> GetEntraUserWithAchievementsAsync(string userGraphId);

    /// <summary>
    /// Gets a lightweight anonymous Graph <see cref="User"/> representation used when no real user is available.
    /// </summary>
    /// <returns>A <see cref="User"/> representing an anonymous user.</returns>
    Task<User> GetAnonymousGraphUserAsync();

    /// <summary>
    /// Gets the current authenticated user's Azure AD object id (Entra id) if available.
    /// </summary>
    /// <returns>The object id string or <c>null</c> when not available.</returns>
    Task<string?> GetCurrentUserEntraId();

    /// <summary>
    /// Gets the current authenticated user's display name.
    /// </summary>
    Task<string> GetDisplayName();

    /// <summary>
    /// Gets the current authenticated user's email address.
    /// </summary>
    Task<string> GetUserEmail();

    /// <summary>
    /// Gets the domain portion of the current user's email address (lowercased).
    /// </summary>
    Task<string> GetUserEmailDomain();

    /// <summary>
    /// Gets the prefix (local-part) of the current user's email address (lowercased).
    /// </summary>
    Task<string> GetUserEmailPrefix();

    /// <summary>
    /// Returns a string representing the user's root folder based on their email (domain/prefix).
    /// </summary>
    Task<string> GetUserRootFolder();

    /// <summary>
    /// Determines if the user has no workspace roles and therefore effectively has no workspaces available.
    /// </summary>
    Task<bool> IsUserWithoutWorkspaces();

    /// <summary>
    /// Returns true when the current user is being viewed in a 'guest' or restricted presentation mode.
    /// </summary>
    Task<bool> IsViewingAsGuest();

    /// <summary>
    /// Returns true when the current user is being viewed as a visitor (limited capability).
    /// </summary>
    Task<bool> IsViewingAsVisitor();

    /// <summary>
    /// Sets whether the current context should present the user as a guest.
    /// </summary>
    Task SetViewingAsGuest(bool isGuest);

    /// <summary>
    /// Sets whether the current context should present the user as a visitor.
    /// </summary>
    Task SetViewingAsVisitor(bool isVisitor);

    /// <summary>
    /// Gets the underlying authenticated <see cref="ClaimsPrincipal"/> used by the application.
    /// </summary>
    /// <param name="forceReload">When true, forces re-evaluation of the authentication state provider.</param>
    Task<ClaimsPrincipal> GetAuthenticatedUser(bool forceReload = false);

    /// <summary>
    /// Returns true if the current authenticated user has the project admin role for the given project acronym.
    /// </summary>
    /// <param name="projectAcronym">Project acronym (prefix used in role names).</param>
    Task<bool> IsUserProjectAdmin(string projectAcronym);

    /// <summary>
    /// Returns true if the current authenticated user is a workspace lead for the given project.
    /// </summary>
    /// <param name="projectAcronym">Project acronym (prefix used in role names).</param>
    Task<bool> IsUserProjectWorkspaceLead(string projectAcronym);

    /// <summary>
    /// Returns true if the current authenticated user is a member of the specified project (admin or member role).
    /// </summary>
    /// <param name="projectAcronym">Project acronym (prefix used in role names).</param>
    Task<bool> IsUserProjectMember(string projectAcronym);

    /// <summary>
    /// Returns true when the current authenticated user has the DataHub administrator role.
    /// </summary>
    Task<bool> IsUserDatahubAdmin();

    /// <summary>
    /// Ensures that the authenticated graph user has a corresponding portal user record and updates
    /// first/last login timestamps appropriately.
    /// </summary>
    Task RegisterAuthenticatedPortalUser();

    /// <summary>
    /// Creates a new <see cref="PortalUser"/> with an associated <see cref="EntraUser"/> if one does not already exist.
    /// </summary>
    /// <param name="userGraphId">Azure AD object id (Graph id) of the user to create.</param>
    /// <returns>The newly created <see cref="PortalUser"/>, or <c>null</c> when a portal record already exists or creation fails.</returns>
    public Task<PortalUser?> CreatePortalEntraUserAsync(string userGraphId);

    /// <summary>
    /// Creates a new <see cref="PortalUser"/> with an associated <see cref="ExternalUser"/> if one does not already exist.
    /// Use this for users that are external to the tenant and tracked via an external OID.
    /// </summary>
    /// <param name="userOid">The external user's OID used to identify them.</param>
    /// <param name="first">The first name of the external user.</param>
    /// <param name="last">The last name of the external user.</param>
    /// <param name="org">The organization of the external user.</param>
    /// <param name="jobTitle">The job title of the external user.</param>
    /// <param name="email">The email address of the external user.</param>
    /// <returns>The newly created <see cref="PortalUser"/>, or <c>null</c> when a portal record already exists or creation fails.</returns>
    public Task<PortalUser?> CreatePortalExternalUserAsync(string userOid, string first, string last, string org, string jobTitle, string email);

    /// <summary>
    /// Persists changes to the provided <see cref="PortalUser"/> instance.
    /// </summary>
    /// <param name="updatedUser">The portal user with modifications to save.</param>
    /// <returns><c>true</c> when the update succeeded; otherwise <c>false</c>.</returns>
    Task<bool> UpdatePortalUserAsync(PortalUser updatedUser);

    /// <summary>
    /// Event raised after a portal user has been updated. Handlers receive a <see cref="PortalUserUpdatedEventArgs"/>.
    /// </summary>
    public event EventHandler<PortalUserUpdatedEventArgs> PortalUserUpdated;

    /// <summary>
    /// Returns true when the user's sign-in on this day should be considered a daily login (used for analytics/achievements).
    /// </summary>
    Task<bool> IsDailyLogin();

    /// <summary>
    /// Checks whether a user with the specified email exists in the current tenant (Azure AD).
    /// </summary>
    /// <param name="email">Email address to check.</param>
    Task<bool> CheckUserInTenant(string email);
}

public static class UserInformationServiceConstants
{
    public static readonly string ANONYMOUS_USER_ID = "c90acba3-26e4-471d-bbdf-544906e6a980";
    public static readonly string ANONYMOUS_USER_NAME = "Anonymous User";
    public static readonly string ANONYMOUS_USER_EMAIL = "anyone@example.com";

    private static User anonymousUser;
    public static User GetAnonymousUser()
    {
        if (anonymousUser == null)
        {
            anonymousUser = new User()
            {
                Id = ANONYMOUS_USER_ID,
                Mail = ANONYMOUS_USER_EMAIL,
                DisplayName = ANONYMOUS_USER_NAME,
                UserPrincipalName = ANONYMOUS_USER_EMAIL
            };
        }
        return anonymousUser;
    }
}
