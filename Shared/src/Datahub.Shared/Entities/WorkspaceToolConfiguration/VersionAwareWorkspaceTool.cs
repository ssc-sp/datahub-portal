using System;
using System.Collections.Generic;
using System.Text;

namespace Datahub.Shared.Entities.WorkspaceToolConfiguration;

#nullable enable
public class VersionAwareWorkspaceToolInfo
{
    public static readonly Version UNDER_DEVELOPMENT = new(9999, 12, 31);
    public static readonly Version ALWAYS = new(1, 0, 0);

    public string ToolName { get; set; } = string.Empty;
    public string ToolLabel { get; set; } = string.Empty;
    public string ToolCategory { get; set; } = string.Empty;
    public string ToolDescription { get; set; } = string.Empty;
    public string ToolIcon { get; set; } = string.Empty;
    public IEnumerable<string> ToolDependencies => TerraformTemplate.GetDependencyNames(ToolName);
    public Version MinAvailableVersion { get; set; } = ALWAYS;
    public bool CanBeDeleted { get; set; } = true;
    public bool IsDisabled { get; set; } = false;
    public (string Name, object[] Parameters) ToolCostInformation { get; set; } = ("No cost information available for this resource.", []);
    public (string Text, string URL)[] AdditionalLinks { get; set; } = [];
    public IEnumerable<VersionAwareWorkspaceToolConfigInfo> ConfigurationVersions { get; set; } = Array.Empty<VersionAwareWorkspaceToolConfigInfo>();

    public bool IsAvailable(Version workspaceVersion) => MinAvailableVersion <= workspaceVersion;

    public bool IsConfigurable(Version workspaceVersion) => ConfigurationVersions != null && ConfigurationVersions.Any(c => c.MinVersion <= workspaceVersion);
    public bool IsConfigurableInFutureVersion(Version workspaceVersion) => ConfigurationVersions != null && ConfigurationVersions.Any(c => c.MinVersion > workspaceVersion);

    public VersionAwareWorkspaceToolConfigInfo? GetApplicableConfigInfo(Version workspaceVersion)
    {
        if (ConfigurationVersions == null || !ConfigurationVersions.Any())
        {
            return null;
        }
        // Get the config with the highest MinVersion that is less than or equal to the workspaceVersion
        return ConfigurationVersions
            .Where(c => c.MinVersion <= workspaceVersion)
            .OrderByDescending(c => c.MinVersion)
            .FirstOrDefault();
    }
}
public class VersionAwareWorkspaceToolConfigInfo
{
    public Version MinVersion { get; set; } = VersionAwareWorkspaceToolInfo.ALWAYS;
    // TODO config methods
}
