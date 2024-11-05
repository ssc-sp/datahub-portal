namespace Datahub.Shared;

public static class TerraformStatus
{
    public const string CreateRequested = "CreateRequested";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    public const string DeleteRequested = "DeleteRequested";
    public const string DeleteInProgress = "DeleteInProgress";
    public const string Deleted = "Deleted";

    public const string Unknown = "Unknown";
    public const string Failed = "Failed";
    public const string Missing = "Missing";
}