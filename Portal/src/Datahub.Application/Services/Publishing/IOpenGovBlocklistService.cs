using Datahub.Core.Model.Datahub;

namespace Datahub.Application.Services.Publishing;

public interface IOpenGovBlocklistService
{
    /// <summary>
    /// Gets only active blocklist entries
    /// </summary>
    Task<List<OpenGovPublishingBlocklist>> GetActiveBlocklistEntriesAsync();

    /// <summary>
    /// Gets a specific blocklist entry by ID
    /// </summary>
    Task<OpenGovPublishingBlocklist> GetBlocklistEntryAsync(int id);

    /// <summary>
    /// Checks if a user is blocked based on email hostname or department
    /// </summary>
    Task<bool> IsUserBlockedAsync(string emailDomain, string? departmentName = null);

    /// <summary>
    /// Adds a new blocklist entry
    /// </summary>
    Task<OpenGovPublishingBlocklist> AddBlocklistEntryAsync(string departmentName, string emailHostname, string notes);

    /// <summary>
    /// Updates an existing blocklist entry
    /// </summary>
    Task<OpenGovPublishingBlocklist> UpdateBlocklistEntryAsync(int id, string departmentName, string emailHostname, string notes);

    /// <summary>
    /// Soft deletes a blocklist entry (marks as deleted)
    /// </summary>
    Task DeleteBlocklistEntryAsync(int id);
}
