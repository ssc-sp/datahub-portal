using Datahub.Core.Data;
using Microsoft.Graph;

namespace Datahub.Application.Services.UserManagement;

/// <summary>
/// Provides methods for querying Microsoft Graph user data and retrieving user information.
/// This service abstracts direct Graph API interactions and is primarily used for bulk user lookups and
/// user detail retrieval by various identifiers (id, email, SAM account name).
/// </summary>
public interface IMSGraphService
{
    /// <summary>
    /// HTTP client name used for Graph API requests.
    /// </summary>
    const string HttpClientName = "MSGraphClient";

    /// <summary>
    /// Gets an authenticated <see cref="GraphServiceClient"/> for making Microsoft Graph API calls.
    /// </summary>
    /// <returns>An authenticated <see cref="GraphServiceClient"/> instance.</returns>
    Task<GraphServiceClient> GetAuthenticatedClient();

    /// <summary>
    /// Retrieves a user from Microsoft Graph by their object id (user id).
    /// </summary>
    /// <param name="userId">The Microsoft Graph user object id.</param>
    /// <param name="token">Cancellation token for the operation.</param>
    /// <returns>A <see cref="GraphUser"/> containing the user's Graph data, or throws if not found.</returns>

    /// <summary>
    /// Retrieves a user from Microsoft Graph by their object id (user id).
    /// </summary>
    /// <param name="userId">The Microsoft Graph user object id.</param>
    /// <param name="token">Cancellation token for the operation.</param>
    /// <returns>A <see cref="GraphUser"/> containing the user's Graph data, or throws if not found.</returns>
    Task<GraphUser> GetUserAsync(string userId, CancellationToken token = default);

    /// <summary>
    /// Retrieves a user from Microsoft Graph by their email address.
    /// </summary>
    /// <param name="email">The user's email address to search for.</param>
    /// <param name="token">Cancellation token for the operation.</param>
    /// <returns>A <see cref="GraphUser"/> matching the email, or throws if not found.</returns>
    Task<GraphUser> GetUserFromEmailAsync(string email, CancellationToken token);

    /// <summary>
    /// Retrieves a collection of users from Microsoft Graph matching a filter text (mail starts with).
    /// Results are returned as a dictionary keyed by user id for efficient lookup.
    /// </summary>
    /// <param name="filterText">The filter text to match against user email addresses (startswith).</param>
    /// <param name="token">Cancellation token for the operation.</param>
    /// <returns>A dictionary of <see cref="GraphUser"/> objects keyed by their id.</returns>
    Task<Dictionary<string, GraphUser>> GetUsersListAsync(string filterText, CancellationToken token);

    /// <summary>
    /// Gets the display name of a user from Microsoft Graph.
    /// </summary>
    /// <param name="userId">The Microsoft Graph user object id.</param>
    /// <param name="token">Cancellation token for the operation.</param>
    /// <returns>The user's display name, or "..." if the user is not found or has no display name.</returns>
    Task<string> GetUserName(string userId, CancellationToken token = default);

    /// <summary>
    /// Gets the email address of a user from Microsoft Graph.
    /// </summary>
    /// <param name="userId">The Microsoft Graph user object id.</param>
    /// <param name="token">Cancellation token for the operation.</param>
    /// <returns>The user's email address.</returns>
    Task<string> GetUserEmail(string userId, CancellationToken token);

    /// <summary>
    /// Retrieves the Microsoft Graph user object id for a user by their email address.
    /// </summary>
    /// <param name="email">The user's email address to search for.</param>
    /// <param name="token">Cancellation token for the operation.</param>
    /// <returns>The user's Graph object id, or an empty string if not found.</returns>
    Task<string> GetUserIdFromEmailAsync(string email, CancellationToken token);

    /// <summary>
    /// Retrieves a user from Microsoft Graph by their on-premises SAM account name (Active Directory sAMAccountName).
    /// Used for hybrid identity scenarios where users exist in both on-premises AD and Azure AD.
    /// </summary>
    /// <param name="userName">The SAM account name to search for.</param>
    /// <param name="token">Cancellation token for the operation.</param>
    /// <returns>A <see cref="GraphUser"/> matching the SAM account name, or throws if not found.</returns>
    Task<GraphUser> GetUserFromSamAccountNameAsync(string userName, CancellationToken token);
}
