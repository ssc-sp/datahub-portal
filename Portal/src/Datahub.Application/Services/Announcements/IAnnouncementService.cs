using Datahub.Core.Model.Announcements;

namespace Datahub.Application.Services.Announcements;

public interface IAnnouncementService
{
    /// <summary>
    /// Gets a list of all announcements.
    /// </summary>
    /// <returns></returns>
    public Task<List<Announcement>> GetAnnouncementsAsync();
    
    /// <summary>
    /// Returns an announcement with the given id. If no announcement is found, returns an empty announcement.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<Announcement?> GetAnnouncementAsync(int id);
    public Task<bool> SaveAnnouncementAsync(Announcement announcement);
    public Task<bool> DeleteAnnouncementAsync(int id);
    Task<List<AnnouncementPreview>> GetActivePreviews(bool isFrench);
}

public record AnnouncementPreview(int Id, string Preview);