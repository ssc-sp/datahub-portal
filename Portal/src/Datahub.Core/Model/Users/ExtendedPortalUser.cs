using System.Diagnostics.CodeAnalysis;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Users;

namespace Datahub.Core.Model.Users;

public class ExtendedPortalUser
{
    // Reference to the underlying PortalUser
    public required PortalUser PortalUser { get; init; }

    // Delegated properties (read-only, reflect current PortalUser values)
    public int Id => PortalUser.Id;
    public string? Email => PortalUser.Email;
    public string? DisplayName => PortalUser.DisplayName;
    public DateTime? FirstLoginDateTime => PortalUser.FirstLoginDateTime;
    public DateTime? LastLoginDateTime => PortalUser.LastLoginDateTime;
    public string? BannerPictureUrl => PortalUser.BannerPictureUrl;
    public string? ProfilePictureUrl => PortalUser.ProfilePictureUrl;

    // Extended flags maintained separately
    public bool IsDeleted { get; set; }
    public bool IsLocked { get; set; }

    public ExtendedPortalUser()
    {
    }

    [SetsRequiredMembers]
    public ExtendedPortalUser(PortalUser portalUser)
    {
        PortalUser = portalUser;
    }
}
