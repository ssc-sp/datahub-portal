using Datahub.Core.Model.Announcements;

namespace Datahub.Application.Services.Announcements;

public interface IAnnouncementService
{
    public Task<List<Announcement>> GetAnnouncementsAsync();
    public Task<Announcement> GetAnnouncementAsync(int id);
    public Task<Announcement> CreateAnnouncementAsync(Announcement announcement);
    public Task<Announcement> UpdateAnnouncementAsync(Announcement announcement);
    public Task<bool> DeleteAnnouncementAsync(int id);
}