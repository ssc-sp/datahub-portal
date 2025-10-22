using Datahub.Application.Services.Publishing;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Services.Publishing;

public class OpenGovBlocklistService : IOpenGovBlocklistService
{
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
    private readonly IUserInformationService _userInformationService;

    public OpenGovBlocklistService(
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        IUserInformationService userInformationService)
    {
        _dbContextFactory = dbContextFactory;
        _userInformationService = userInformationService;
    }

    public async Task<List<OpenGovPublishingBlocklist>> GetActiveBlocklistEntriesAsync()
    {
        await using var ctx = await _dbContextFactory.CreateDbContextAsync();
        
        return await ctx.OpenGovPublishingBlocklist
            .Include(b => b.AddedByUser)
            .Where(b => b.Status == BlocklistStatus.Active)
            .OrderByDescending(b => b.DateAdded)
            .ToListAsync();
    }

    public async Task<OpenGovPublishingBlocklist> GetBlocklistEntryAsync(int id)
    {
        await using var ctx = await _dbContextFactory.CreateDbContextAsync();
        
        var entry = await ctx.OpenGovPublishingBlocklist
            .Include(b => b.AddedByUser)
            .Include(b => b.RemovedByUser)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (entry == null)
            throw new InvalidOperationException($"Blocklist entry with ID {id} not found");

        return entry;
    }

    public async Task<bool> IsUserBlockedAsync(string emailDomain, string? departmentName = null)
    {
        if (string.IsNullOrWhiteSpace(emailDomain))
            return false;

        await using var ctx = await _dbContextFactory.CreateDbContextAsync();

        // Check if the email domain is blocked (department is informational only, not used for matching)
        return await ctx.OpenGovPublishingBlocklist
            .Where(b => b.Status == BlocklistStatus.Active && b.EmailHostname == emailDomain)
            .AnyAsync();
    }

    public async Task<OpenGovPublishingBlocklist> AddBlocklistEntryAsync(string departmentName, string emailHostname, string notes)
    {
        await using var ctx = await _dbContextFactory.CreateDbContextAsync();
        
        var currentUser = await _userInformationService.GetCurrentPortalUserAsync();
        if (currentUser == null)
            throw new InvalidOperationException("Current user not found");

        var now = DateTime.UtcNow;

        var entry = new OpenGovPublishingBlocklist
        {
            DepartmentName = string.IsNullOrWhiteSpace(departmentName) ? string.Empty : departmentName.Trim(),
            EmailHostname = string.IsNullOrWhiteSpace(emailHostname) ? string.Empty : emailHostname.Trim().ToLowerInvariant(),
            Status = BlocklistStatus.Active,
            DateAdded = now,
            AddedByUserId = currentUser.Id,
            Notes = notes ?? string.Empty
        };

        ctx.OpenGovPublishingBlocklist.Add(entry);
        await ctx.SaveChangesAsync();

        return entry;
    }

    public async Task<OpenGovPublishingBlocklist> UpdateBlocklistEntryAsync(int id, string departmentName, string emailHostname, string notes)
    {
        await using var ctx = await _dbContextFactory.CreateDbContextAsync();
        
        var entry = await ctx.OpenGovPublishingBlocklist.FindAsync(id);
        if (entry == null)
            throw new InvalidOperationException($"Blocklist entry with ID {id} not found");

        entry.DepartmentName = string.IsNullOrWhiteSpace(departmentName) ? null : departmentName.Trim();
        entry.EmailHostname = string.IsNullOrWhiteSpace(emailHostname) ? null : emailHostname.Trim().ToLowerInvariant();
        entry.Notes = notes;

        await ctx.SaveChangesAsync();

        return entry;
    }

    public async Task DeleteBlocklistEntryAsync(int id)
    {
        await using var ctx = await _dbContextFactory.CreateDbContextAsync();
        
        var entry = await ctx.OpenGovPublishingBlocklist.FindAsync(id);
        if (entry == null)
            throw new InvalidOperationException($"Blocklist entry with ID {id} not found");

        var currentUser = await _userInformationService.GetCurrentPortalUserAsync();
        if (currentUser == null)
            throw new InvalidOperationException("Current user not found");

        entry.Status = BlocklistStatus.Deleted;
        entry.DateRemoved = DateTime.UtcNow;
        entry.RemovedByUserId = currentUser.Id;

        await ctx.SaveChangesAsync();
    }
}
