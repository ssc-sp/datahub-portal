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
    public static bool CreatedOrInProcessOf(string status)
    {
        return new List<string>
        {
            CreateRequested, InProgress, Completed
        }.Contains(status);
    }

    public static bool RequestedOrInProcessOf(string status)
    {
        return new List<string>
        {
            CreateRequested, InProgress
        }.Contains(status);
    }
    public static bool DeletedOrInProcessOf(string status)
    {
        return new List<string>
        {
            DeleteInProgress, Deleted, DeleteRequested
        }.Contains(status);
    }
    public static bool ExistsOrInAnyProcess(string status)
    {
        return new List<string>()
        {
            CreateRequested, InProgress, Completed, DeleteRequested, DeleteInProgress, Deleted
        }.Contains(status);
    }
}