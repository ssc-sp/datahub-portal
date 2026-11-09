using Datahub.Portal.Layout;
using Xunit;

namespace Datahub.Tests.Portal;

public class DeletedWorkspaceSectionTests
{
    [Theory]
    [InlineData(WorkspaceSidebar.SectionViews.Dashboard)]
    [InlineData(WorkspaceSidebar.SectionViews.Dashboard_FR)]
    [InlineData(WorkspaceSidebar.SectionViews.Sudo)]
    [InlineData(null)]
    [InlineData("")]
    public void IsSectionAvailableForDeletedWorkspace_AllowsDashboardAndSudo(string? section)
    {
        Assert.True(WorkspaceSidebar.IsSectionAvailableForDeletedWorkspace(section));
    }

    [Theory]
    [InlineData(WorkspaceSidebar.SectionViews.Toolbox)]
    [InlineData(WorkspaceSidebar.SectionViews.Toolbox_FR)]
    [InlineData(WorkspaceSidebar.SectionViews.Users)]
    [InlineData(WorkspaceSidebar.SectionViews.Users_FR)]
    [InlineData(WorkspaceSidebar.SectionViews.ExternalUsers)]
    [InlineData(WorkspaceSidebar.SectionViews.Settings)]
    [InlineData(WorkspaceSidebar.SectionViews.Storage)]
    [InlineData(WorkspaceSidebar.SectionViews.Delete)]
    [InlineData(WorkspaceSidebar.SectionViews.DeleteWorkspace)]
    [InlineData(WorkspaceSidebar.SectionViews.Healthcheck)]
    [InlineData(WorkspaceSidebar.SectionViews.Publishing)]
    public void IsSectionAvailableForDeletedWorkspace_BlocksOtherSections(string section)
    {
        Assert.False(WorkspaceSidebar.IsSectionAvailableForDeletedWorkspace(section));
    }
}
