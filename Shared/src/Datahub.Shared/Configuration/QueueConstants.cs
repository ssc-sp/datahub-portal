namespace Datahub.Shared.Configuration;

public static class QueueConstants
{
    public const string BugReportQueueName = "bug-report";
    public const string EmailNotificationQueueName = "email-notification";
    public const string InfrastructureHealthCheckQueueName = "infrastructure-health-check";
    public const string WorkspaceAppServiceConfigurationQueueName = "workspace-app-service-configuration";
    public const string ProjectUsageNotificationQueueName = "project-usage-notification";
    public const string ProjectInactivityNotificationQueueName = "project-inactivity-notification";
    public const string PongQueueName = "pong-queue";
}