using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Announcements;
using Datahub.Core.Model.Announcements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazor;
using Microsoft.Extensions.Caching.Memory;

namespace Datahub.Infrastructure.Services.Announcements;

public class AnnouncementService : IAnnouncementService
{
    private readonly DatahubPortalConfiguration _datahubPortalConfiguration;
    private readonly IDbContextFactory<DatahubProjectDBContext> _datahubProjectDbFactory;
    private readonly IDatahubAuditingService _auditingService;
    private readonly ILogger<AnnouncementService> _logger;
    private readonly IMemoryCache _cache;

    private static readonly string ActivePreviewsCacheKeyEn = "AnnouncementService.ActivePreviews.En";
    private static readonly string ActivePreviewsCacheKeyFr = "AnnouncementService.ActivePreviews.Fr";

    public AnnouncementService(DatahubPortalConfiguration datahubPortalConfiguration, IDbContextFactory<DatahubProjectDBContext> datahubProjectDbFactory,
        IDatahubAuditingService auditingService, ILogger<AnnouncementService> logger, IMemoryCache cache)
    {
        _datahubPortalConfiguration = datahubPortalConfiguration;
        _datahubProjectDbFactory = datahubProjectDbFactory;
        _logger = logger;
        _auditingService = auditingService;
        _cache = cache;
    }
    public async Task<List<Announcement>> GetAnnouncementsAsync()
    {
        _logger.LogInformation("Getting announcements");
        await using var context = await _datahubProjectDbFactory.CreateDbContextAsync();
        var announcements = await context.Announcements
            .AsNoTracking()
            .Include(a => a.CreatedBy)
            .Include(a => a.UpdatedBy)
            .OrderByDescending(a => a.StartDateTime)
            .ToListAsync();
        
        return announcements;
    }

    public async Task<Announcement?> GetAnnouncementAsync(int id)
    {
        _logger.LogInformation("Getting announcement with id {Id}", id);
        await using var context = await _datahubProjectDbFactory.CreateDbContextAsync();
        var article = await context.Announcements
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        return article;
    }

    public async Task<bool> SaveAnnouncementAsync(Announcement announcement)
    {
        try
        {
            await using var context = await _datahubProjectDbFactory.CreateDbContextAsync();
            if (announcement.Id == 0)
            {
                context.Announcements.Add(announcement);
            }
            else
            {
                context.Announcements.Update(announcement);
            }
            await context.TrackSaveChangesAsync(_auditingService);
            ClearPreviewsCache();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving announcement");
            return false;
        }
    }

    public async Task<bool> DeleteAnnouncementAsync(int id)
    {
        try
        {
            await using var context = await _datahubProjectDbFactory.CreateDbContextAsync();
            var announcement = await context.Announcements.FindAsync(id);
            if (announcement == null)
                return false;

            announcement.IsDeleted = true;
            announcement.ForceHidden = true;
            context.Announcements.Update(announcement);
            await context.TrackSaveChangesAsync(_auditingService);
            ClearPreviewsCache();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting announcement");
            return false;
        }
    }



    public async Task<List<AnnouncementPreview>> GetActivePreviews(bool isFrench)
    {
        var cacheKey = isFrench ? ActivePreviewsCacheKeyFr : ActivePreviewsCacheKeyEn;
        if (_cache.TryGetValue(cacheKey, out List<AnnouncementPreview>? cached) && cached is not null)
        {
            return cached;
        }

        await using var ctx = await _datahubProjectDbFactory.CreateDbContextAsync();

        var today = DateTime.Now.Date;

        var articles = await ctx.Announcements
            .Where(e => !e.ForceHidden && today > e.StartDateTime && (!e.EndDateTime.HasValue || today < e.EndDateTime.Value))
            .OrderByDescending(e => e.StartDateTime)
            .Select(e => new AnnouncementPreview(e.Id, isFrench ? e.PreviewFr : e.PreviewEn, e.Severity))
            .ToListAsync();

        // Cache with a reasonable expiration so updates show up eventually even if invalidation is missed.
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };
        _cache.Set(cacheKey, articles, options);

        return articles;
    }

    public void ClearPreviewsCache()
    {
        _cache.Remove(ActivePreviewsCacheKeyEn);
        _cache.Remove(ActivePreviewsCacheKeyFr);
    }

    public Severity GetSeverity(int n)
    {
        if (n == 0)
        {
            return Severity.Error;
        }
        return n == 1 ? Severity.Warning : Severity.Normal;
    }

    public Color GetColor(int n, bool isVisible=true)
    {
        if (isVisible)
        {
            switch (n)
            {
                case 0:
                    return Color.Error;
                case 1:
                    return Color.Warning;
                case 2:
                    return Color.Dark;
            }
        }
        return Color.Transparent;
    }

    public string GetIcon(int n)
    {
        switch (n)
        {
            case 0:
                return Icons.Material.Outlined.ErrorOutline as string;
            case 1:
                return Icons.Material.Outlined.WarningAmber as string;
            case 2:
                return Icons.Material.Outlined.Info as string;
        }
        return Icons.Material.Outlined.Info as string;
    }

    public string GetText(int n)
    {
        switch (n)
        {
            case 0:
                return "IMPORTANT";
            case 1:
                return "NOTICE";
            case 2:
                return "INFO";
        }
        return "INFO";
    }
}