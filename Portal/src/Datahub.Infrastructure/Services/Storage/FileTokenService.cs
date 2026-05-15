using System.Collections.Concurrent;
using Datahub.Core.Storage;

namespace Datahub.Infrastructure.Services.Storage;

/// <summary>
/// Thread-safe, in-memory implementation of <see cref="IFileTokenService"/>.
/// Expired entries are lazily removed on every access.
/// </summary>
public sealed class FileTokenService : IFileTokenService
{
    private readonly ConcurrentDictionary<string, FileTokenEntry> _tokens = new(StringComparer.Ordinal);

    public string CreateToken(ICloudStorageManager manager, string storageAccountName, string container, string filePath, TimeSpan expiry)
    {
        PurgeExpired();

        var token = Guid.NewGuid().ToString("N");
        var entry = new FileTokenEntry(manager, storageAccountName, container, filePath, DateTimeOffset.UtcNow.Add(expiry));
        _tokens[token] = entry;
        return token;
    }

    public FileTokenEntry? ResolveToken(string token)
    {
        if (!_tokens.TryGetValue(token, out var entry))
            return null;

        if (DateTimeOffset.UtcNow >= entry.ExpiresAt)
        {
            _tokens.TryRemove(token, out _);
            return null;
        }

        return entry;
    }

    private void PurgeExpired()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var kvp in _tokens)
        {
            if (now >= kvp.Value.ExpiresAt)
                _tokens.TryRemove(kvp.Key, out _);
        }
    }
}
