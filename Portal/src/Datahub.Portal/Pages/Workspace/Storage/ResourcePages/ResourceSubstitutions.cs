namespace Datahub.Portal.Pages.Workspace.Storage.ResourcePages;

public static class ResourceSubstitutions 
{
    public const string SasUri = "<sas_uri>";
    public const string DesktopCode = "<desktop_code>";
    public const string UploadCode = "<upload_code>";
    public const string StorageAccount = "<storage_account>";
    public const string ProjectAcronym = "<project_acronym>";
    public const string ContainerName = "<container_name>";

    public static string GetStorageAccountNameFromProjectAcronym(string projectAcronym)
    {
        var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return $"dh{projectAcronym.ToLower()}{envName}";
    }
}