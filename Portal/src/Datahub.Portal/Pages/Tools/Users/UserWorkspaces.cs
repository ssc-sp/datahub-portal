using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;

namespace Datahub.Portal.Pages.Tools.Users
{
    public record UserWorkspaces
    {
        public PortalUser User { get; init; } = null!;
        public List<Datahub_Project> Workspaces { get; init; } = null!;
    }
}