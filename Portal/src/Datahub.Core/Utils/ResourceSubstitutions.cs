namespace Datahub.Portal.Pages.Workspace.Storage.ResourcePages;

public static class ResourceSubstitutions
{
    public const string SasUri = "<sas_uri>";
    public const string DesktopCode = "<desktop_code>";
    public const string UploadCode = "<upload_code>";
    public const string StorageAccount = "<storage_account>";
    public const string ProjectAcronym = "<project_acronym>";
    public const string ContainerName = "<container_name>";

    public const string AZAccountKey = "<az_key>";
    public const string AZAccountName = "<az_name>";

    public const string AWSAccessKey = "<aws_access_key>";
    public const string AWSAccessKeySecret = "<aws_access_key_secret>";
    public const string AWSRegion = "<aws_region>";
    public const string AWSS3Bucket = "<aws_s3_bucket>";

    public const string GCPProjectId = "<gcp_project_id>";
    public const string GCPAccountKey = "<gcp_account_key>";

    public static string GetStorageAccountNameFromProjectAcronym(string projectAcronym)
    {
        var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return $"dh{projectAcronym.ToLower()}{envName}";
    }
}