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
            .ToListAsync();
        
        return announcements;
    }

    public Task<Announcement> GetAnnouncementAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Announcement> CreateAnnouncementAsync(Announcement announcement)
    {
        throw new NotImplementedException();
    }

    public Task<Announcement> UpdateAnnouncementAsync(Announcement announcement)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAnnouncementAsync(int id)
    {
        throw new NotImplementedException();
    }
}