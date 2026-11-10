namespace Datahub.Application.Services.Security;

/// <summary>
/// Notifies active application scopes when a portal user's workspace role changes.
/// </summary>
public interface IUserRoleChangeNotifier
{
    /// <summary>
    /// Subscribes to changes for a single portal user.
    /// </summary>
    IDisposable Subscribe(int portalUserId, Func<CancellationToken, Task> handler);
}
