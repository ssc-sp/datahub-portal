using Datahub.Core.Services;
using System.Reflection;

namespace Datahub.Portal.Services
{
    public class PortalVersionService : IPortalVersionService
    {
        private readonly Dictionary<string, string> _attributes;
        private readonly string _release;

        public PortalVersionService()
        {
            _attributes = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>().ToDictionary(a => a.Key, a => a.Value);
            _release = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }

        public string ReleaseVersion => _release;
        public string GetCustomValue(string key) => _attributes.ContainsKey(key) ? _attributes[key] : string.Empty;
    }
}
