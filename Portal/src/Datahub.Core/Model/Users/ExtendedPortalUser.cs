using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Users;

namespace Datahub.Core.Model.Users;

public class ExtendedPortalUser
{
    public int Id { get; set; }
    public required string GraphGuid { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public DateTime? FirstLoginDateTime { get; set; }
    public DateTime? LastLoginDateTime { get; set; }
    public string? BannerPictureUrl { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsLocked { get; set; }

    public ExtendedPortalUser()
    {
    }

    public ExtendedPortalUser(PortalUser portalUser)
    {
        GraphGuid = portalUser.GraphGuid;
        Id = portalUser.Id;
        Email = portalUser.Email;
        DisplayName = portalUser.DisplayName;
        FirstLoginDateTime = portalUser.FirstLoginDateTime;
        LastLoginDateTime = portalUser.LastLoginDateTime;
        BannerPictureUrl = portalUser.BannerPictureUrl;
        ProfilePictureUrl = portalUser.ProfilePictureUrl;
    }
}
