namespace Datahub.Application.Services;

public interface IProjectStorageConfigurationService
{
    string GetProjectStorageAccountName(string projectAcronym);
}
