namespace Datahub.Portal.Pages.Project.FileExplorer.ResourcePages;

public static class ResourceSubstitutions 
{
    public const string Token = "<token>";
    public const string StorageAccount = "<storage_account>";
    public const string ProjectAcronym = "<project_acronym>";
    public const string ContainerName = "<container_name>";

    public static string GetStorageAccountNameFromProjectAcronym(string projectAcronym)
    {
        var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return $"dh{projectAcronym}{envName}";
    }
}