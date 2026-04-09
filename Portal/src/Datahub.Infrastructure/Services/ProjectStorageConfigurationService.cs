using Datahub.Application.Configuration;
using Datahub.Application.Services;

namespace Datahub.Infrastructure.Services;

public class ProjectStorageConfigurationService : IProjectStorageConfigurationService
{
    private readonly DatahubPortalConfiguration _portalConfiguration;

    public ProjectStorageConfigurationService(DatahubPortalConfiguration portalConfiguration)
    {
        _portalConfiguration = portalConfiguration;
    }

    public string GetProjectStorageAccountName(string projectAcronym)
    {
        var envName = GetEnvironmentName();
        return $"{_portalConfiguration.ResourcePrefix}proj{projectAcronym.ToLower()}{envName}";
    }

    private static string GetEnvironmentName()
    {
        var envName = (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "dev").ToLower();

        // map developemnt or sandbox to dev
        if (envName.Equals("development") || envName.Equals("sand"))
            return "dev";

        return envName;
    }

}
