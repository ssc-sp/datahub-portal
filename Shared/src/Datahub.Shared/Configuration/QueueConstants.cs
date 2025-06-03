namespace Datahub.Shared.Configuration;

public static class QueueConstants
{
    public const string PongQueueName = "pong-queue";

    // Serverless Operations Queues
    public const string BugReportQueueName = "bug-report";
    public const string EmailNotificationQueueName = "email-notification";
    public const string InfrastructureHealthCheckQueueName = "infrastructure-health-check";
    public const string InfrastructureHealthCheckResultsQueueName = "infrastructure-health-check-results";
    public const string ProjectCapacityUpdateQueueName = "project-capacity-update";
    public const string ProjectInactiveQueueName = "project-inactive";
    public const string ProjectInactivityNotificationQueueName = "project-inactivity-notification";
    public const string ProjectUsageNotificationQueueName = "project-usage-notification";
    public const string ProjectUsageUpdateQueueName = "project-usage-update";
    public const string UserInactivityNotification = "user-inactivity-notification";
    public const string TerraformOutputHandlerQueueName = "terraform-output-handler";
    public const string WorkspaceAppServiceConfigurationQueueName = "workspace-app-service-configuration";

    // Synchronization output queues
    public const string DatabricksSyncOutputQueueName = "databricks-sync-output";
    public const string KeyvaultSyncOutputQueueName = "keyvault-sync-output";
    public const string StorageSyncOutputQueueName = "storage-sync-output";

    // Resource Provisioner Queues
    public const string ResourceRunRequestQueueName = "resource-run-request";
    public const string UserRunRequestQueueName = "user-run-request";
}