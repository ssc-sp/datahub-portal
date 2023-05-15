using System.Collections.Generic;
using System.Threading.Tasks;

namespace Datahub.Core.Services.Announcements;

public interface IAnnouncementService
{
    Task<List<AnnouncementPreview>> GetActivePreviews();
}

public record AnnouncementPreview(int Id, string Title, string Body);