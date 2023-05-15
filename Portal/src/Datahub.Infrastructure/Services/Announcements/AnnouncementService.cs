using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Announcements;
using Datahub.Core.Model.Announcements;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services;
using Datahub.Core.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services.Announcements;

public class AnnouncementService : IAnnouncementService
{
    private readonly DatahubPortalConfiguration _datahubPortalConfiguration;
    private readonly IDbContextFactory<DatahubProjectDBContext> _datahubProjectDbFactory;
    private readonly ILogger<AnnouncementService> _logger;


    public AnnouncementService(DatahubPortalConfiguration datahubPortalConfiguration, IDbContextFactory<DatahubProjectDBContext> datahubProjectDbFactory,
        ILogger<AnnouncementService> logger)
    {
        _datahubPortalConfiguration = datahubPortalConfiguration;
        _datahubProjectDbFactory = datahubProjectDbFactory;
        _logger = logger;
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

    public Task<Announcement> CreateAnnouncementAsync(Announcement announcement)
    {
        throw new NotImplementedException();
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
            await context.SaveChangesAsync();
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
}