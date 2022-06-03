namespace Datahub.Core.Services
{
    public interface IPortalVersionService
    {
        string CommitSha { get; }
        string CommitUrl { get; }
        string ReleaseTag { get; }
        string ReleaseUrl { get; }
        string ReleaseVersion { get; }
    }
}