namespace Datahub.Core.Services
{
    public interface IPortalVersionService
    {
        string ReleaseVersion { get; }
        string GetCustomValue(string key);
    }
}