# Datahub.Functions

Azure Functions project (isolated worker, .NET 10) containing all serverless operations for the Datahub platform. Handles email notifications, workspace usage tracking, inactivity management, infrastructure health checks, virus scan processing, Terraform output processing, and more.

---

## Quick start (local)

**Prerequisites**
- .NET SDK 10.0 or later
- Azure Functions Core Tools v4
- Azurite (local Storage emulator) or an Azure Storage account connection string
- A `local.settings.json` populated from the template (see [Configuration](#configuration))

```powershell
# 1. Move to the project folder
cd ServerlessOperations/src/Datahub.Functions

# 2. Start Azurite in background (if using local storage)
azurite --silent &

# 3. Start the Functions host
func host start
```

---

## Functions

### Timer-triggered

| Function | Class | Schedule setting | Description |
|---|---|---|---|
| `ProjectUsageScheduler` | `ProjectUsageScheduler` | `%ProjectUsageCRON%` | Queries workspace cost/storage and enqueues `project-usage-update` messages for each workspace |
| `InactivityScheduler` | `InactivityScheduler` | `%InactivityCRON%` | Queries inactive projects and users; enqueues `project-inactivity-notification` and `user-inactivity-notification` messages |
| `CheckInfrastructureScheduled` | `CheckInfrastructureStatus` | `%ProjectUsageCRON%` | Runs all infrastructure health checks on a schedule |
| `DocumentationRankUpdate` | `DocumentationRankUpdate` | `%DocumentationRankUpdateCRON%` | Recalculates documentation page hit rankings in the database |

### Service Bus-triggered

| Function | Class | Queue consumed | Message type |
|---|---|---|---|
| `EmailNotificationHandler` | `EmailNotificationHandler` | `email-notification` | `EmailRequestMessage` |
| `BugReport` | `BugReport` | `bug-report` | `BugReportMessage` |
| `TerraformOutputHandler` | `TerraformOutputHandler` | `terraform-output-handler` | Key/value JSON map of Terraform outputs |
| `ProjectUsageUpdater` | `ProjectUsageUpdater` | `project-usage-update` | `ProjectUsageUpdateMessage` |
| `ProjectUsageNotifier` | `ProjectUsageNotifier` | `project-usage-notification` | `ProjectUsageNotificationMessage` |
| `ProjectInactivityNotifier` | `ProjectInactivityNotifier` | `project-inactivity-notification` | `ProjectInactivityNotificationMessage` |
| `ProjectInactiveHandler` | `ProjectInactiveHandler` | `project-inactive` | `ProjectInactiveMessage` |
| `UserInactivityNotifier` | `UserInactivityNotifier` | `user-inactivity-notification` | `UserInactivityNotificationMessage` |
| `ConfigureWorkspaceAppService` | `ConfigureWorkspaceAppService` | `workspace-app-service-configuration` | `WorkspaceAppServiceConfigurationMessage` |
| `CheckInfrastructureStatusQueue` | `CheckInfrastructureStatus` | `infrastructure-health-check` | `InfrastructureHealthCheckMessage` |
| `RecordInfrastructureStatusQueue` | `RecordInfrastructureStatus` | `infrastructure-health-check-results` | `InfrastructureHealthCheckResultMessage` |
| `VirusScanNotificationHandler` | `VirusScanNotificationHandler` | `virus-scan-notification` | `VirusScanNotificationMessage` |
| `VirusScanUserStatusHandler` | `VirusScanUserStatusHandler` | `virus-scan-user-status` | `VirusScanUserStatusMessage` |

### HTTP-triggered

| Function | Class | Method | Auth level | Description |
|---|---|---|---|---|
| `ProjectUsageSchedulerHttp` | `ProjectUsageScheduler` | GET / POST | Function | Manually trigger usage update for specific workspace acronyms |
| `InactivitySchedulerHttp` *(DEBUG only)* | `InactivityScheduler` | GET / POST | Function | Manually trigger inactivity scheduling |
| `CheckInfrastructureStatusHttp` | `CheckInfrastructureStatus` | GET / POST | Function | Run a health check for a specific infrastructure resource |
| `RecordInfrastructureStatusHttp` | `RecordInfrastructureStatus` | GET / POST | Function | Manually record a health check result to the database |
| `GetUsersStatus` | `GetUsersStatus` | GET | Function | Returns locked and service-principal AAD users via Microsoft Graph |
| `CreateGraphUser` | `CreateGraphUser` | GET / POST | Function | Invites a new user to the AAD tenant and sends an invitation email |
| `FunctionsHealthCheck` | `FunctionsHealthCheck` | GET / POST | Function | Checks GC Notify connectivity; result is cached for 5 minutes |

---

## Service Bus

| Setting | Value |
|---|---|
| **Connection string key** | `DatahubServiceBus:ConnectionString` |

All Service Bus-triggered functions use the same connection string key. MassTransit (`AddMassTransit`) is also registered for publishing outbound messages to queues.

### Queues consumed

| Queue name | Constant (`QueueConstants`) |
|---|---|
| `email-notification` | `EmailNotificationQueueName` |
| `bug-report` | `BugReportQueueName` |
| `terraform-output-handler` | `TerraformOutputHandlerQueueName` |
| `project-usage-update` | `ProjectUsageUpdateQueueName` |
| `project-usage-notification` | `ProjectUsageNotificationQueueName` |
| `project-inactivity-notification` | `ProjectInactivityNotificationQueueName` |
| `project-inactive` | `ProjectInactiveQueueName` |
| `user-inactivity-notification` | `UserInactivityNotification` |
| `workspace-app-service-configuration` | `WorkspaceAppServiceConfigurationQueueName` |
| `infrastructure-health-check` | `InfrastructureHealthCheckQueueName` |
| `infrastructure-health-check-results` | `InfrastructureHealthCheckResultsQueueName` |
| `virus-scan-notification` | `VirusScanNotificationQueueName` |
| `virus-scan-user-status` | `VirusScanUserStatusQueueName` |

### Queues published

| Queue name | Published by |
|---|---|
| `project-usage-update` | `ProjectUsageScheduler` |
| `project-usage-notification` | `ProjectUsageUpdater` |
| `project-inactivity-notification` | `InactivityScheduler` |
| `user-inactivity-notification` | `InactivityScheduler` |
| `email-notification` | Various functions (e.g. `ProjectUsageNotifier`, `ProjectInactivityNotifier`) |

---

## Message format

All inbound Service Bus messages use the same JSON envelope:

```json
{
  "message": { /* typed message payload */ }
}
```

The `DeserializeAndUnwrapMessageAsync<T>` extension method handles unwrapping automatically.

### Key message types

#### `EmailRequestMessage` (`email-notification`)
```json
{
  "message": {
    "to": ["recipient@example.com"],
    "subject": "...",
    "body": "..."
  }
}
```

#### `ProjectUsageUpdateMessage` (`project-usage-update`)
```json
{
  "message": {
    "projectAcronym": "ABC1",
    "numberOfDays": 30
  }
}
```

#### `ProjectUsageNotificationMessage` (`project-usage-notification`)
```json
{
  "message": {
    "projectAcronym": "ABC1"
  }
}
```

#### `ProjectInactivityNotificationMessage` (`project-inactivity-notification`)
```json
{
  "message": {
    "projectId": 42
  }
}
```

#### `UserInactivityNotificationMessage` (`user-inactivity-notification`)
```json
{
  "message": {
    "userId": "<aad-object-id>"
  }
}
```

#### `BugReportMessage` (`bug-report`)
```json
{
  "message": {
    "bugReportType": "InfrastructureError",
    "workspaceAcronym": "ABC1",
    "description": "...",
    "url": "..."
  }
}
```

#### `InfrastructureHealthCheckMessage` (`infrastructure-health-check`)
```json
{
  "message": {
    "group": "all",
    "resourceType": "AzureStorageAccount",
    "workspaceAcronym": "ABC1"
  }
}
```

#### `VirusScanNotificationMessage` (`virus-scan-notification`)
```json
{
  "message": {
    "workspaceAcronym": "ABC1",
    "fileName": "report.pdf",
    "scanStatus": "Clean",
    "userObjectId": "<aad-oid>",
    "scanCompletedOn": "2024-01-01T12:00:00Z"
  }
}
```

#### `VirusScanUserStatusMessage` (`virus-scan-user-status`)
```json
{
  "message": {
    "workspaceAcronym": "ABC1",
    "fileName": "report.pdf",
    "blobPath": "container/path/report.pdf",
    "scanStatus": "Clean",
    "uploaderEmail": "user@example.com",
    "uploaderObjectId": "<aad-oid>",
    "aclsApplied": true,
    "fileSizeBytes": 102400,
    "scanCompletedOn": "2024-01-01T12:00:00Z"
  }
}
```

#### `WorkspaceAppServiceConfigurationMessage` (`workspace-app-service-configuration`)
```json
{
  "message": {
    "projectAcronym": "ABC1",
    "configuration": { /* AppServiceConfiguration */ }
  }
}
```

---

## Configuration

Copy `local.settings.json-tmpl` to `local.settings.json` and fill in the values.

### Required settings

| Key | Description |
|---|---|
| `AzureWebJobsStorage` | Azure Storage connection string; use `UseDevelopmentStorage=true` locally |
| `FUNCTIONS_WORKER_RUNTIME` | Must be `dotnet-isolated` |
| `DatahubServiceBus:ConnectionString` | Azure Service Bus namespace connection string |
| `datahub_mssql_project` | SQL Server connection string for the Datahub project database |

### Azure AD / Service Principal

| Key | Description |
|---|---|
| `TENANT_ID` | AAD tenant ID |
| `FUNC_SP_CLIENT_ID` | Service principal client ID used by functions |
| `FUNC_SP_CLIENT_SECRET` | Service principal client secret |
| `SUBSCRIPTION_ID` | Azure subscription ID |

### Timer schedules (CRON expressions)

| Key | Used by |
|---|---|
| `ProjectUsageCRON` | `ProjectUsageScheduler`, `CheckInfrastructureScheduled` |
| `InactivityCRON` | `InactivityScheduler` |
| `DocumentationRankUpdateCRON` | `DocumentationRankUpdate` |

### Optional / feature settings

| Key | Description |
|---|---|
| `Email__*` | SMTP/email configuration (host, port, credentials); validated before sending |
| `Email__DumpMessages` | Set `true` in dev to log emails instead of sending them |
| `NotificationPercents` | Comma-separated budget threshold percentages for usage notifications (default: `25,50,80,100`) |
| `AzureDevOpsConfiguration__*` | ADO organisation, project, PAT – used by `ConfigureWorkspaceAppService` |
| `AzureDevOpsConfiguration__AppServiceConfigPipeline` | Pipeline name to trigger for App Service configuration |
| `GCNotify__*` | GC Notify API key and template IDs for notification emails |


