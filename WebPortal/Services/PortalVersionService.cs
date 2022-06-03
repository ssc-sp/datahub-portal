using Datahub.Core.Services;
using System.Reflection;

namespace Datahub.Portal.Services
{
    public class PortalVersionService : IPortalVersionService
    {
        public PortalVersionService()
        {
        }

        public string ReleaseTag => PortalVersion.ReleaseTag;
        public string ReleaseUrl => PortalVersion.ReleaseUrl;
        public string CommitSha => PortalVersion.CommitSha;
        public string CommitUrl => PortalVersion.CommitUrl;
        public string ReleaseVersion => Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;
    }
}
