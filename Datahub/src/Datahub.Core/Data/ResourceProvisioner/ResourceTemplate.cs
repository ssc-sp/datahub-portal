namespace Datahub.Core.Data.ResourceProvisioner;

// ReSharper disable UnusedAutoPropertyAccessor.Global
public record ResourceTemplate
{
    // ReSharper disable MemberCanBePrivate.Global
    public string Name { get; init;}
    public string Version { get; init;}

    public static ResourceTemplate Default => new()
    {
        Name = "new-project-template",
        Version = "latest",
    };
}