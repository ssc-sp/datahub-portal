namespace Datahub.Core.Services;

public interface IPortalVersionService
{
    string ReleaseVersion { get; }
    string BuildId { get; }
    string Release { get; }
    string Commit { get; }
    string GetCustomValue(string key);
}