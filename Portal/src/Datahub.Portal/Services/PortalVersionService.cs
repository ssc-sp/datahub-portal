using Datahub.Core.Services;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Datahub.Portal.Services;

public class PortalVersionService : IPortalVersionService
{
    private readonly Dictionary<string, string> _attributes;
    private readonly string _release;
    private readonly PortalVersion _portalVersion;

    public PortalVersionService(IOptions<PortalVersion> config)
    {
        _attributes = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>().ToDictionary(a => a.Key, a => a.Value);
        _release = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        _portalVersion = config.Value;
    }

    public string ReleaseVersion => _release;
    public string BuildId => _portalVersion.BuildId;
    public string Release => _portalVersion.Release;
    public string Commit => _portalVersion.Commit;
    public string GetCustomValue(string key) => _attributes.ContainsKey(key) ? _attributes[key] : string.Empty;
}

public class PortalVersion
{
    public string Release { get; set; }
    public string Commit { get; set; }
    public string BuildId { get; set; }
}