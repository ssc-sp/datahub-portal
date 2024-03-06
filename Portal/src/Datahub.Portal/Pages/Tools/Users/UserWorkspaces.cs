using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Projects;

namespace Datahub.Portal.Pages.Tools.Users
{
    public record UserWorkspaces
    {
        public PortalUser User { get; init; } = null!;
        public List<DatahubProject> Workspaces { get; init; } = null!;
    }
}