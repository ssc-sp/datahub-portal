namespace Datahub.Shared.Configuration;

public static class QueueConstants
{
    /// <summary>
    /// Queue: <c>pong-queue</c><br/>
    /// Used for ping/pong health probes between services.
    /// </summary>
    public const string PongQueueName = "pong-queue";

    // -------------------------------------------------------------------------
    // Serverless Operations Queues  (consumed by Datahub.Functions)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Queue: <c>bug-report</c><br/>
    /// Message: <c>BugReportMessage</c><br/>
    /// Publishers:
    /// <list type="bullet">
    ///   <item><description>Portal – user-submitted bug reports</description></item>
    ///   <item><description>ResourceProvisioner_PyFunctions – workspace sync errors (<c>send_exception_to_service_bus</c>)</description></item>
    /// </list>
    /// Consumers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.BugReport</c> – posts to Microsoft Teams, creates Azure DevOps work item, and sends GC Notify email</description></item>
    /// </list>
    /// </summary>
    public const string BugReportQueueName = "bug-report";

    /// <summary>
    /// Queue: <c>email-notification</c><br/>
    /// Message: <c>EmailRequestMessage</c><br/>
    /// Publishers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.ProjectUsageNotifier</c></description></item>
    ///   <item><description><c>Datahub.Functions.ProjectInactivityNotifier</c></description></item>
    ///   <item><description>Various other functions and portal services that need to send email</description></item>
    /// </list>
    /// Consumers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.EmailNotificationHandler</c> – sends email via SMTP (MailKit)</description></item>
    /// </list>
    /// </summary>
    public const string EmailNotificationQueueName = "email-notification";

    /// <summary>
    /// Queue: <c>infrastructure-health-check</c><br/>
    /// Message: <c>InfrastructureHealthCheckMessage</c><br/>
    /// Publishers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.CheckInfrastructureScheduled</c> (timer) – enqueues an "all" group check</description></item>
    ///   <item><description>External callers via <c>CheckInfrastructureStatusHttp</c> HTTP trigger</description></item>
    /// </list>
    /// Consumers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.CheckInfrastructureStatusQueue</c> – runs health checks and publishes results to <c>infrastructure-health-check-results</c></description></item>
    /// </list>
    /// </summary>
    public const string InfrastructureHealthCheckQueueName = "infrastructure-health-check";

    /// <summary>
    /// Queue: <c>infrastructure-health-check-results</c><br/>
    /// Message: <c>InfrastructureHealthCheckResultMessage</c><br/>
    /// Publishers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.CheckInfrastructureStatusQueue</c></description></item>
    ///   <item><description>ResourceProvisioner_PyFunctions – workspace sync health status (<c>send_healthcheck_to_service_bus</c>)</description></item>
    /// </list>
    /// Consumers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.RecordInfrastructureStatusQueue</c> – persists results to the database</description></item>
    /// </list>
    /// </summary>
    public const string InfrastructureHealthCheckResultsQueueName = "infrastructure-health-check-results";

    /// <summary>
    /// Queue: <c>project-capacity-update</c><br/>
    /// Message: <c>ProjectCapacityUpdateMessage</c><br/>
    /// Publishers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.TerraformOutputHandler</c> – after processing Terraform outputs</description></item>
    /// </list>
    /// Consumers:
    /// <list type="bullet">
    ///   <item><description>Portal infrastructure services – updates workspace storage/budget capacity records</description></item>
    /// </list>
    /// </summary>
    public const string ProjectCapacityUpdateQueueName = "project-capacity-update";

    /// <summary>
    /// Queue: <c>project-inactive</c><br/>
    /// Message: <c>ProjectInactiveMessage</c><br/>
    /// Publishers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.ProjectInactivityNotifier</c> – when a project is deemed inactive beyond the deletion threshold</description></item>
    /// </list>
    /// Consumers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.ProjectInactiveHandler</c> – placeholder for workspace deletion logic (not yet implemented)</description></item>
    /// </list>
    /// </summary>
    public const string ProjectInactiveQueueName = "project-inactive";

    /// <summary>
    /// Queue: <c>project-inactivity-notification</c><br/>
    /// Message: <c>ProjectInactivityNotificationMessage</c><br/>
    /// Publishers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.InactivityScheduler</c> (timer) – enqueues one message per inactive project</description></item>
    /// </list>
    /// Consumers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.ProjectInactivityNotifier</c> – sends inactivity notification emails to project leads</description></item>
    /// </list>
    /// </summary>
    public const string ProjectInactivityNotificationQueueName = "project-inactivity-notification";

    /// <summary>
    /// Queue: <c>project-usage-notification</c><br/>
    /// Message: <c>ProjectUsageNotificationMessage</c><br/>
    /// Publishers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.ProjectUsageUpdater</c> – after updating cost/storage, enqueues a notification if budget thresholds are crossed</description></item>
    /// </list>
    /// Consumers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.ProjectUsageNotifier</c> – sends budget/usage alert emails via GC Notify</description></item>
    /// </list>
    /// </summary>
    public const string ProjectUsageNotificationQueueName = "project-usage-notification";

    /// <summary>
    /// Queue: <c>project-usage-update</c><br/>
    /// Message: <c>ProjectUsageUpdateMessage</c><br/>
    /// Publishers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.ProjectUsageScheduler</c> (timer) – enqueues one message per workspace</description></item>
    /// </list>
    /// Consumers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.ProjectUsageUpdater</c> – fetches Azure cost and storage data and updates workspace records</description></item>
    /// </list>
    /// </summary>
    public const string ProjectUsageUpdateQueueName = "project-usage-update";

    /// <summary>
    /// Queue: <c>user-inactivity-notification</c><br/>
    /// Message: <c>UserInactivityNotificationMessage</c><br/>
    /// Publishers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.InactivityScheduler</c> (timer) – enqueues one message per inactive user</description></item>
    /// </list>
    /// Consumers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.UserInactivityNotifier</c> – sends user inactivity notification emails and may trigger account disabling</description></item>
    /// </list>
    /// </summary>
    public const string UserInactivityNotification = "user-inactivity-notification";

    /// <summary>
    /// Queue: <c>terraform-output-handler</c><br/>
    /// Message: Key/value JSON map of Terraform pipeline outputs<br/>
    /// Publishers:
    /// <list type="bullet">
    ///   <item><description>Azure DevOps Terraform pipelines – post outputs after a successful run</description></item>
    /// </list>
    /// Consumers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.TerraformOutputHandler</c> – parses outputs, updates workspace resource records in the database, and enqueues capacity/sync messages</description></item>
    /// </list>
    /// </summary>
    public const string TerraformOutputHandlerQueueName = "terraform-output-handler";

    /// <summary>
    /// Queue: <c>workspace-app-service-configuration</c><br/>
    /// Message: <c>WorkspaceAppServiceConfigurationMessage</c><br/>
    /// Publishers:
    /// <list type="bullet">
    ///   <item><description>Portal / resource provisioning flow – when App Service configuration needs to be applied to a workspace</description></item>
    /// </list>
    /// Consumers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.ConfigureWorkspaceAppService</c> – triggers the ADO App Service configuration pipeline</description></item>
    /// </list>
    /// </summary>
    public const string WorkspaceAppServiceConfigurationQueueName = "workspace-app-service-configuration";

    /// <summary>
    /// Queue: <c>virus-scan-notification</c><br/>
    /// Message: <c>VirusScanNotificationMessage</c><br/>
    /// Publishers:
    /// <list type="bullet">
    ///   <item><description>Virus scan pipeline / storage event handler – after a scan completes</description></item>
    /// </list>
    /// Consumers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.VirusScanNotificationHandler</c> – creates an in-app system notification for the file owner</description></item>
    /// </list>
    /// </summary>
    public const string VirusScanNotificationQueueName = "virus-scan-notification";

    /// <summary>
    /// Queue: <c>virus-scan-user-status</c><br/>
    /// Message: <c>VirusScanUserStatusMessage</c><br/>
    /// Publishers:
    /// <list type="bullet">
    ///   <item><description>Virus scan pipeline / storage event handler – after ACLs are applied to a scanned file</description></item>
    /// </list>
    /// Consumers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.VirusScanUserStatusHandler</c> – placeholder for user status updates, audit logging, and workspace metrics (not yet fully implemented)</description></item>
    /// </list>
    /// </summary>
    public const string VirusScanUserStatusQueueName = "virus-scan-user-status";

    // -------------------------------------------------------------------------
    // Synchronization output queues
    // (published by ResourceProvisioner_PyFunctions after syncing workspace users)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Queue: <c>databricks-sync-output</c><br/>
    /// Message: Databricks workspace user synchronization result<br/>
    /// Publishers:
    /// <list type="bullet">
    ///   <item><description>ResourceProvisioner_PyFunctions (<c>sync_databricks_workspace_users_function</c>) – after syncing Databricks workspace users</description></item>
    /// </list>
    /// Consumers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.TerraformOutputHandler</c> – processes sync output and updates workspace state</description></item>
    /// </list>
    /// </summary>
    public const string DatabricksSyncOutputQueueName = "databricks-sync-output";

    /// <summary>
    /// Queue: <c>keyvault-sync-output</c><br/>
    /// Message: Key Vault user synchronization result<br/>
    /// Publishers:
    /// <list type="bullet">
    ///   <item><description>ResourceProvisioner_PyFunctions (<c>sync_keyvault_workspace_users_function</c>) – after syncing Key Vault access policies</description></item>
    ///   <item><description>Portal – <c>KeyVaultUserService</c> / <c>KeyVaultCoreService</c></description></item>
    /// </list>
    /// Consumers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.TerraformOutputHandler</c> – processes sync output and updates workspace state</description></item>
    /// </list>
    /// </summary>
    public const string KeyvaultSyncOutputQueueName = "keyvault-sync-output";

    /// <summary>
    /// Queue: <c>storage-sync-output</c><br/>
    /// Message: Storage account user synchronization result<br/>
    /// Publishers:
    /// <list type="bullet">
    ///   <item><description>ResourceProvisioner_PyFunctions (<c>sync_storage_workspace_users_function</c>) – after syncing storage account policies</description></item>
    ///   <item><description>Portal – <c>ProjectUsageUpdater</c> storage sync path</description></item>
    /// </list>
    /// Consumers:
    /// <list type="bullet">
    ///   <item><description><c>Datahub.Functions.TerraformOutputHandler</c> – processes sync output and updates workspace state</description></item>
    /// </list>
    /// </summary>
    public const string StorageSyncOutputQueueName = "storage-sync-output";

    // -------------------------------------------------------------------------
    // Resource Provisioner Queues
    // -------------------------------------------------------------------------

    /// <summary>
    /// Queue: <c>resource-run-request</c><br/>
    /// Message: <c>WorkspaceDefinition</c><br/>
    /// Publishers:
    /// <list type="bullet">
    ///   <item><description>Portal – <c>ResourceMessagingService.SendToTerraformQueue</c> – triggered when a workspace resource is created or updated</description></item>
    /// </list>
    /// Consumers:
    /// <list type="bullet">
    ///   <item><description><c>ResourceProvisioner.Functions.ResourceRunRequest</c> – deserializes the message into <c>WorkspaceDefinition</c>, validates it, clones the infrastructure Git repo, renders Terraform templates, and opens a pull request on Azure DevOps</description></item>
    /// </list>
    /// </summary>
    public const string ResourceRunRequestQueueName = "resource-run-request";

    /// <summary>
    /// Queue: <c>user-run-request</c><br/>
    /// Message: <c>WorkspaceDefinition</c> (same envelope as <c>resource-run-request</c>)<br/>
    /// Publishers:
    /// <list type="bullet">
    ///   <item><description>Portal – <c>ResourceMessagingService.SendToUserQueue</c> – triggered when workspace membership changes</description></item>
    /// </list>
    /// Consumers:
    /// <list type="bullet">
    ///   <item><description>ResourceProvisioner_PyFunctions (<c>SynchronizeWorkspaceUsersQueueTrigger</c>) – syncs Databricks, Key Vault, and Storage users, then publishes a health check result to <c>infrastructure-health-check-results</c></description></item>
    /// </list>
    /// </summary>
    public const string UserRunRequestQueueName = "user-run-request";
}
