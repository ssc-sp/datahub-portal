using System;
using System.Collections.Generic;
using System.Text;

namespace Datahub.Shared.Entities.WorkspaceToolConfiguration;

#nullable enable
public class VersionAwareWorkspaceToolInfo
{
    public static readonly Version UNDER_DEVELOPMENT = new(9999, 12, 31);
    public static readonly Version ALWAYS = new(0, 0, 0);

    public string ToolName { get; set; } = string.Empty;
    public string ToolLabel { get; set; } = string.Empty;
    public string ToolCategory { get; set; } = string.Empty;
    public string ToolDescription { get; set; } = string.Empty;
    public string ToolIcon { get; set; } = string.Empty;
    public IEnumerable<string> ToolDependencies => TerraformTemplate.GetDependencyNames(ToolName) ?? [];
    public Version MinAvailableVersion { get; set; } = ALWAYS;
    public bool CanBeDeleted { get; set; } = true;
    public bool IsDisabled { get; set; } = false;
    public (string Name, object[] Parameters) ToolCostInformation { get; set; } = ("No cost information available for this resource.", []);
    // Unfortunately, we can't use the IWorkspaceToolConfiguration interface here due to language constraints on static abstract members (error CS8920)
    public Func<object, (string Name, object[] Parameters)>? ToolCostSummaryFunction { get; set; } = (config) => (string.Empty, []);
    public (string Text, string URL)[] AdditionalLinks { get; set; } = [];
    public IEnumerable<VersionAwareWorkspaceToolConfigInfo> ConfigurationVersions { get; set; } = Array.Empty<VersionAwareWorkspaceToolConfigInfo>();

    public bool IsAvailable(Version workspaceVersion) => MinAvailableVersion <= workspaceVersion;

    public bool IsConfigurable(Version workspaceVersion) => ConfigurationVersions != null && ConfigurationVersions.Any(c => c.MinVersion <= workspaceVersion && c.HasConfigurationDialog);

    public bool IsConfigurableInFutureVersion(Version workspaceVersion) => ConfigurationVersions != null && ConfigurationVersions.Any(c => c.MinVersion > workspaceVersion && c.HasConfigurationDialog);

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
    public Type ConfigClass { get; set; } = typeof(IWorkspaceToolConfiguration);
    public Type? ConfigDialogClass { get; set; } = null;
    public bool HasConfigurationDialog => ConfigDialogClass != null;
    public IWorkspaceToolConfiguration GetConfigurationFromWorkspaceDefinition(WorkspaceDefinition workspaceDefinition)
    {
        var methodName = nameof(IWorkspaceToolConfiguration.ReadFromWorkspaceDefinition);
        var method = ConfigClass.GetMethod(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static) ??
            throw new InvalidOperationException($"The class {ConfigClass.FullName} does not implement the required static method {methodName}.");
        if (method.Invoke(null, [workspaceDefinition]) is not IWorkspaceToolConfiguration config)
        {
            throw new InvalidOperationException($"The method {methodName} in class {ConfigClass.FullName} did not return a valid IWorkspaceToolConfiguration instance.");
        }
        return config;
    }
    public string GetPropertyLabel(string propertyName)
    {
        var methodName = nameof(IWorkspaceToolConfiguration.GetPropertyLabel);
        var method = ConfigClass.GetMethod(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static) ??
            throw new InvalidOperationException($"The class {ConfigClass.FullName} does not implement the required static method {methodName}.");
        if (method.Invoke(null, [propertyName]) is not string label)
        {
            throw new InvalidOperationException($"The method {methodName} in class {ConfigClass.FullName} did not return a valid string label.");
        }
        return label;
    }
}
