namespace Datahub.Application.Services;

public interface IProjectStorageConfigurationService
{
    Task<string> GetProjectStorageAccountKey(string projectAcronym);
    string GetProjectStorageAccountName(string projectAcronym);
}
