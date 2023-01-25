namespace Datahub.Core.Enums
{
    public static class TerraformOutputStatus
    {
        public static string PendingApproval => "Pending Approval";
        public static string InProgress => "In Progress";
        public static string Completed => "Completed";
        public static string Missing => "Missing";
        public static string Failed => "Failed";
    }
}