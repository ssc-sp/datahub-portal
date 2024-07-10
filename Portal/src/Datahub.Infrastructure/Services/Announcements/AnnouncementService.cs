using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Announcements;
using Datahub.Core.Model.Announcements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services.Announcements;

public class AnnouncementService : IAnnouncementService
{
    private readonly DatahubPortalConfiguration _datahubPortalConfiguration;
    private readonly IDbContextFactory<DatahubProjectDBContext> _datahubProjectDbFactory;
    private readonly IDatahubAuditingService _auditingService;
    private readonly ILogger<AnnouncementService> _logger;

    public AnnouncementService(DatahubPortalConfiguration datahubPortalConfiguration, IDbContextFactory<DatahubProjectDBContext> datahubProjectDbFactory,
        IDatahubAuditingService auditingService, ILogger<AnnouncementService> logger)
    {
        _datahubPortalConfiguration = datahubPortalConfiguration;
        _datahubProjectDbFactory = datahubProjectDbFactory;
        _logger = logger;
        _auditingService = auditingService;
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
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving announcement");
            return false;
        }
    }

    public Task<bool> DeleteAnnouncementAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<List<AnnouncementPreview>> GetActivePreviews(bool isFrench)
    {
        await using var ctx = await _datahubProjectDbFactory.CreateDbContextAsync();

        var today = DateTime.Now.Date;

        var articles = await ctx.Announcements
            .Where(e => !e.ForceHidden && today > e.StartDateTime && (!e.EndDateTime.HasValue || today < e.EndDateTime.Value))
            .OrderByDescending(e => e.StartDateTime)
            .Select(e => new AnnouncementPreview(e.Id, isFrench ? e.PreviewFr : e.PreviewEn))
            .ToListAsync();

        return articles;
    }
}