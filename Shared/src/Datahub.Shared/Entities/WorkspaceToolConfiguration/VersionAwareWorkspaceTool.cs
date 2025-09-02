using System;
using System.Collections.Generic;
using System.Text;

namespace Datahub.Shared.Entities.WorkspaceToolConfiguration;

public class VersionAwareWorkspaceToolInfo
{
    public static readonly Version FAR_FUTURE = new(9999, 12, 31);
    public static readonly Version ALWAYS = new(1, 0, 0);

    public string ToolName { get; set; }
    public string ToolLabel { get; set; }
    public string ToolCategory { get; set; }
    public string ToolDescription { get; set; }
    public string ToolIcon { get; set; }
    public IEnumerable<string> ToolDependencies { get; set; } = Array.Empty<string>();
    public Version MinAvailableVersion { get; set; }
    public IEnumerable<VersionAwareWorkspaceToolConfigInfo> ConfigurationVersions { get; set; } = Array.Empty<VersionAwareWorkspaceToolConfigInfo>();

    public bool IsAvailable(Version workspaceVersion) => MinAvailableVersion <= workspaceVersion;

    public bool IsConfigurable(Version workspaceVersion) => ConfigurationVersions != null && ConfigurationVersions.Any(c => c.MinVersion <= workspaceVersion);

#nullable enable
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
    public Version MinVersion { get; set; }
    // TODO config methods
}
