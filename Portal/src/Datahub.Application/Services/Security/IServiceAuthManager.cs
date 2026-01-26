using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using System.Collections.Immutable;

namespace Datahub.Application.Services.Security;

/// <summary>
/// Provides methods to manage and query service-level authorization information such as
/// project administrators, user roles, cached admin lookups and tenant-specific flags.
/// Implementations are responsible for maintaining any authorization caches used to
/// speed up common lookups.
/// </summary>
public interface IServiceAuthManager
{
    /// <summary>
    /// Gets the list of all known project acronyms.
    /// </summary>
    /// <returns>A list of project acronyms.</returns>
    List<string> GetAllProjects();

    /// <summary>
    /// Sets whether the specified user is viewing the application as a guest.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="isGuest">True to mark the user as viewing as guest; otherwise false.</param>
    void SetViewingAsGuest(string userId, bool isGuest);

    /// <summary>
    /// Gets whether the specified user is currently marked as viewing as a guest.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>True if the user is viewing as a guest; otherwise false.</returns>
    bool GetViewingAsGuest(string userId);

    /// <summary>
    /// Gets the project-level admin roles assigned to the given user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>A list of role names that grant administrative privileges to projects.</returns>
    List<string> GetAdminProjectRoles(string userId);

    /// <summary>
    /// Invalidates any internal authorization cache maintained by the manager.
    /// </summary>
    /// <returns>True if the cache was successfully invalidated; otherwise false.</returns>
    bool InvalidateAuthCache();

    /// <summary>
    /// Determines whether the specified user id is an administrator of the given project.
    /// </summary>
    /// <param name="userid">The user's identifier (graph id).</param>
    /// <param name="projectAcronym">The project acronym to check.</param>
    /// <returns>A task that resolves to true if the user is an admin of the project; otherwise false.</returns>
    Task<bool> IsProjectAdmin(string userid, string projectAcronym);

    /// <summary>
    /// Determines whether the specified portal user is an administrator of the given project.
    /// </summary>
    /// <param name="portalUser">The portal user to check.</param>
    /// <param name="projectAcronym">The project acronym to check.</param>
    /// <returns>A task that resolves to true if the user is an admin of the project; otherwise false.</returns>
    Task<bool> IsProjectAdmin(PortalUser portalUser, string projectAcronym);

    /// <summary>
    /// Gets the email addresses of project administrators for the specified project.
    /// </summary>
    /// <param name="projectAcronym">The project acronym.</param>
    /// <returns>A list of administrator email addresses for the project.</returns>
    List<string> GetProjectAdminsEmails(string projectAcronym);

    /// <summary>
    /// Gets the mailbox email addresses associated with the specified project.
    /// </summary>
    /// <param name="projectAcronym">The project acronym.</param>
    /// <returns>A list of mailbox email addresses for the project.</returns>
    List<string> GetProjectMailboxEmails(string projectAcronym);

    /// <summary>
    /// Checks the internal cache for admin entries and returns a mapping of project acronyms
    /// to the list of administrator identifiers or roles found for each project.
    /// </summary>
    /// <returns>
    /// A task that resolves to a dictionary mapping project acronyms to lists of admin identifiers or roles.
    /// </returns>
    Task<Dictionary<string, List<string>>> CheckCacheForAdmins();

    /// <summary>
    /// Gets the authorizations (project role and project) for the specified user graph id.
    /// </summary>
    /// <param name="userGraphId">The user's graph id.</param>
    /// <returns>
    /// A task that resolves to an immutable list of tuples containing the role and project
    /// the user is authorized for.
    /// </returns>
    Task<ImmutableList<(Project_Role Role, Datahub_Project Project)>> GetEntraUserAuthorizations(string userGraphId);

    /// <summary>
    /// Gets the authorizations (project role and project) for the specified user external id.
    /// </summary>
    /// <param name="externalId">The user's name identifier.</param>
    /// <returns>
    /// A task that resolves to an immutable list of tuples containing the role and project
    /// the user is authorized for.
    /// </returns>
    Task<ImmutableList<(Project_Role Role, Datahub_Project Project)>> GetExternalUserAuthorizations(string externalId);


    /// <summary>
    /// Determines whether the specified user (by email) is an owner of any CBR resources.
    /// </summary>
    /// <param name="userEmail">The user's email address.</param>
    /// <returns>A task that resolves to true if the user is a CBR owner; otherwise false.</returns>
    Task<bool> IsUserCbrOwner(string userEmail);

    /// <summary>
    /// Gets the list of CBR workspace acronyms that the specified user (by email) owns or has access to.
    /// </summary>
    /// <param name="userEmail">The user's email address.</param>
    /// <returns>A task that resolves to a list of CBR workspace acronyms.</returns>
    Task<List<string>> GetUserCbrWorkspaceAcronyms(string userEmail);
}
