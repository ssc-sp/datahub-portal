namespace Datahub.Core.Storage;

#nullable enable

/// <summary>
/// Maps short-lived GUID tokens to Azure storage download locations.
/// </summary>
public interface IFileTokenService
{
    /// <summary>
    /// Creates a token mapped to the given storage account, container, path and manager.
    /// </summary>
    /// <param name="manager">The storage manager that can produce the real download URI.</param>
    /// <param name="storageAccountName">Azure storage account name.</param>
    /// <param name="container">Blob/ADLS container name.</param>
    /// <param name="filePath">Path to the file within the container.</param>
    /// <param name="expiry">How long the token should remain valid.</param>
    /// <returns>A GUID token string.</returns>
    string CreateToken(ICloudStorageManager manager, string storageAccountName, string container, string filePath, TimeSpan expiry);

    /// <summary>
    /// Resolves a token to its storage location. Returns <c>null</c> if the token is
    /// unknown or has expired.
    /// </summary>
    /// <param name="token">The GUID token string previously returned by <see cref="CreateToken"/>.</param>
    /// <returns>The <see cref="FileTokenEntry"/> for the token, or <c>null</c> if invalid or expired.</returns>
    FileTokenEntry? ResolveToken(string token);
}

/// <summary>
/// Immutable record describing what a file token points to.
/// </summary>
public record FileTokenEntry(
    ICloudStorageManager Manager,
    string StorageAccountName,
    string Container,
    string FilePath,
    DateTimeOffset ExpiresAt);
