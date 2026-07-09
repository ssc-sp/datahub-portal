# ResourceProvisioner.Functions

Azure Functions project (isolated worker, .NET 10) that acts as the Service Bus-triggered entry point for the Resource Provisioner pipeline. It receives workspace provisioning requests, validates them, and delegates to the application layer which creates or updates Terraform infrastructure via pull requests on Azure DevOps.

---

## Functions

### `ResourceRunRequest`

| Property | Value |
|---|---|
| **Class** | `ResourceProvisioner.Functions.ResourceRunRequest` |
| **Trigger** | Azure Service Bus – Queue trigger |
| **Queue** | `resource-run-request` |
| **Connection setting** | `DatahubServiceBus:ConnectionString` |

**What it does:**

1. Receives a JSON envelope from the `resource-run-request` Service Bus queue.
2. Deserializes the `message` property of the envelope into a `WorkspaceDefinition`.
3. Validates the command with `WorkspaceDefinitionValidator` (FluentValidation).
4. Calls `IRepositoryService.HandleResourcing(command)`, which clones/updates the infrastructure Git repository, renders Terraform templates, commits the changes, and opens a pull request on Azure DevOps.
5. Returns (logs) a `PullRequestUpdateMessage` on success.

---

## Service Bus

| Setting | Description |
|---|---|
| **Connection string key** | `DatahubServiceBus:ConnectionString` |
| **Transport** | AMQP over Web Sockets (`host.json` → `extensions.serviceBus.transportType: amqpWebSockets`) |

The project also registers **MassTransit** for Azure Functions (`AddMassTransitForAzureFunctions`) using the same connection string, allowing consumers discovered in the `ResourceProvisioner.Functions` namespace to be wired up automatically.

### Queues consumed

| Queue name | Constant | Consumer |
|---|---|---|
| `resource-run-request` | `QueueConstants.ResourceRunRequestQueueName` | `ResourceRunRequest` function |

> Other queue name constants defined in `QueueConstants` (e.g. `user-run-request`, `terraform-output-handler`) are owned by other services and are listed here for reference only.

---

## Message format

### Inbound – `resource-run-request`

The Service Bus message body is a JSON envelope:

```json
{
  "message": { /* WorkspaceDefinition */ }
}
```

#### `WorkspaceDefinition`

```json
{
  "message": {
    "templates": [
      {
        "name": "new-project-template",
        "status": "create-requested"
      }
    ],
    "workspace": {
      "name": "My Workspace",
      "acronym": "MWS",
      "budgetAmount": 100,
      "subscriptionId": "<azure-subscription-guid>",
      "version": "latest",
      "storageSizeLimitInTB": 5,
      "terraformOrganization": { /* TerraformOrganization */ },
      "users": [
        { "email": "user@example.com", "objectId": "<aad-oid>", "role": "Admin" }
      ],
      "SSCCBRID": "1234"
    },
    "appData": {
      "databricksHostUrl": "",
      "appServiceConfiguration": null,
      "postgresConfiguration": null,
      "databricksConfiguration": null
    },
    "requestingUserEmail": "requester@example.com",
    "resourceGroupName": "fsdh-dev-rg",
    "updateWorkspaceVersion": false
  }
}
```

**Validation rules** (applied before processing):

- `workspace` must not be null and must pass `WorkspaceValidator`.
- `templates` must not be empty; each entry must pass `TerraformTemplateValidator`.
- `requestingUserEmail` must be a valid email address.

#### `TerraformTemplate` – known `name` values

| Constant | String value |
|---|---|
| `TerraformTemplate.NewProjectTemplate` | `new-project-template` |
| `TerraformTemplate.VariableUpdate` | `variable-update` |
| `TerraformTemplate.AzureStorageBlob` | `azure-storage-blob` |
| `TerraformTemplate.AzureDatabricks` | `azure-databricks` |
| `TerraformTemplate.AzureVirtualMachine` | `azure-virtual-machine` |
| `TerraformTemplate.AzureAppService` | `azure-app-service` |
| `TerraformTemplate.AzurePostgres` | `azure-postgres` |

#### Template dependencies

Some templates require others to be provisioned first:

| Template | Depends on |
|---|---|
| `new-project-template` | *(none)* |
| `variable-update` | *(none)* |
| `azure-storage-blob` | `new-project-template` |
| `azure-databricks` | `new-project-template`, `azure-storage-blob` |
| `azure-app-service` | `new-project-template`, `azure-storage-blob` |
| `azure-postgres` | `new-project-template` |

### Outbound – result

The function does not publish to another queue directly. `HandleResourcing` returns a `PullRequestUpdateMessage` (logged only):

```json
{
  "events": [ /* List<RepositoryUpdateEvent> */ ],
  "terraformWorkspace": { /* TerraformWorkspace */ },
  "pullRequestValueObject": { /* PR number, URL, etc. */ }
}
```

---

## Configuration

All settings live in `local.settings.json` (local dev) or Azure Function App settings (cloud). Secrets for the dev environment are fetched from Key Vault `fsdh-key-dev` via `create-dev-appsettings.ps1`.

### Required settings

| Key | Description |
|---|---|
| `DatahubServiceBus:ConnectionString` | Azure Service Bus namespace connection string |
| `AzureWebJobsStorage` | Azure Storage connection string (use `UseDevelopmentStorage=true` locally) |
| `FUNCTIONS_WORKER_RUNTIME` | Must be `dotnet-isolated` |

### Module repository (`ModuleRepository`)

| Key | Description |
|---|---|
| `ModuleRepository__Url` | Git URL of the Terraform module repository (e.g. `https://github.com/ssc-sp/datahub-resource-modules.git`) |
| `ModuleRepository__LocalPath` | Local clone path (e.g. `/tmp/`) |
| `ModuleRepository__TemplatePathPrefix` | Path prefix for templates inside the repo (e.g. `templates/`) |
| `ModuleRepository__ModulePathPrefix` | Path prefix for modules inside the repo (e.g. `modules/`) |
| `ModuleRepository__Name` | Human-readable name (e.g. `datahub-resource-modules`) |

### Infrastructure repository (`InfrastructureRepository`)

| Key | Description |
|---|---|
| `InfrastructureRepository__Url` | Azure DevOps Git URL for the per-environment infrastructure repo |
| `InfrastructureRepository__LocalPath` | Local clone path |
| `InfrastructureRepository__Name` | Repository name (e.g. `datahub-project-infrastructure-dev`) |
| `InfrastructureRepository__MainBranch` | Branch to target for PRs (e.g. `main`) |
| `InfrastructureRepository__ProjectPathPrefix` | Folder prefix for workspace Terraform projects (e.g. `terraform/projects`) |
| `InfrastructureRepository__PullRequestUrl` | Azure DevOps REST API URL for creating pull requests |
| `InfrastructureRepository__PullRequestBrowserUrl` | Browser URL prefix for pull requests |
| `InfrastructureRepository__ApiVersion` | Azure DevOps API version (e.g. `7.1-preview.1`) |
| `InfrastructureRepository__AzureDevOpsConfiguration__TenantId` | AAD tenant ID for DevOps auth |
| `InfrastructureRepository__AzureDevOpsConfiguration__ClientId` | Service principal client ID *(secret – use Key Vault)* |
| `InfrastructureRepository__AzureDevOpsConfiguration__ClientSecret` | Service principal client secret *(secret – use Key Vault)* |

### Terraform variables (`Terraform`)

| Key | Description |
|---|---|
| `Terraform__Backend__ResourceGroupName` | Azure resource group for the Terraform state backend |
| `Terraform__Variables__az_subscription_id` | Target Azure subscription ID |
| `Terraform__Variables__az_tenant_id` | Target AAD tenant ID |
| `Terraform__Variables__az_location` | Azure region (e.g. `canadacentral`) |
| `Terraform__Variables__environment_name` | Environment tag (e.g. `dev`) |
| `Terraform__Variables__environment_classification` | Classification tag (e.g. `U`) |
| `Terraform__Variables__resource_prefix` | Short resource prefix (e.g. `fsdh`) |
| `Terraform__Variables__resource_prefix_alphanumeric` | Alphanumeric resource prefix |
| `Terraform__Variables__budget_amount` | Default workspace budget in CAD |
| `Terraform__Variables__storage_size_limit_tb` | Default storage quota in TB |
| `Terraform__Variables__ssc_cbrid` | SSC CBRID identifier |
| `Terraform__Variables__common_tags__Sector` | Common tag: Sector |
| `Terraform__Variables__common_tags__Environment` | Common tag: Environment |
| `Terraform__Variables__common_tags__ClientOrganization` | Common tag: ClientOrganization |

---

## Local development

1. Install [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local).
2. Start Azurite for local storage: `azurite --silent &`
3. Run `create-dev-appsettings.ps1` to populate secrets from Key Vault into `local.settings.json`:
   ```powershell
   .\create-dev-appsettings.ps1
   ```
4. Start the function host:
   ```powershell
   func start
   ```

To send a test message, publish a JSON envelope (see [Message format](#message-format) above) to the `resource-run-request` queue on your Service Bus namespace.
