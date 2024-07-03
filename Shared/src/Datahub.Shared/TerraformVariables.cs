namespace Datahub.Shared;

public static class TerraformVariables
{
    // Azure resource group output variables
    public const string OutputProjectAcronym = "project_cd";
    public const string OutputNewProjectTemplate = "new_project_template";
    public const string OutputWorkspaceVersion = "workspace_version";
    public const string OutputAzureResourceGroupName = "azure_resource_group_name";

    // Azure Storage output variables
    public const string AzureStorageType = "blob";
    public const string OutputAzureStorageAccountName = "azure_storage_account_name";
    public const string OutputAzureStorageContainerName = "azure_storage_container_name";
    public const string OutputAzureStorageBlobStatus = "azure_storage_blob_status";

    // Azure Databricks output variables
    public const string OutputAzureDatabricksWorkspaceUrl = "azure_databricks_workspace_url";
    public const string OutputAzureDatabricksStatus = "azure_databricks_module_status";
    public const string OutputAzureDatabricksWorkspaceId = "azure_databricks_workspace_id";
    public const string OutputAzureDatabricksWorkspaceName = "azure_databricks_workspace_name";

    // Azure App Service output variables
    public const string OutputAzureAppServiceStatus = "azure_app_service_module_status";
    public const string OutputAzureAppServiceId = "azure_app_service_id";
    public const string OutputAzureAppServiceHostName = "azure_app_service_hostname";

    // Azure Postgres output variables
    public const string OutputAzurePostgresDatabaseName = "azure_postgresql_db_name";
    public const string OutputAzurePostgresDns = "azure_postgresql_dns";
    public const string OutputAzurePostgresId = "azure_postgresql_id";
    public const string OutputAzurePostgresSecretNameAdmin = "azure_postgresql_secret_name_admin";
    public const string OutputAzurePostgresSecretNamePassword = "azure_postgresql_secret_name_password";
    public const string OutputAzurePostgresServerName = "azure_postgresql_server_name";
    public const string OutputAzurePostgresStatus = "azure_psql_module_status";

    // Workspace role assignments
    public const string DatabricksProjectLeadUsers = "databricks_project_lead_users";
    public const string DatabricksAdminUsers = "databricks_admin_users";
    public const string DatabricksProjectUsers = "databricks_project_users";
    public const string DatabricksProjectGuests = "databricks_project_guests";
    public const string StorageContributorUsers = "storage_contributor_users";
    public const string StorageGuestUsers = "storage_reader_users";

    // Terraform backend variables
    public const string BackendResourceGroupName = "resource_group_name";
    public const string BackendStorageAccountName = "storage_account_name";
    public const string BackendContainerName = "container_name";
    public const string BackendKeyName = "key";
    public const string BackendSubscriptionIdName = "subscription_id";

    // Workspace related variables
    public const string ProjectAcronym = "project_cd";
    public const string BudgetAmount = "budget_amount";
    public const string StorageSizeLimitInTb = "storage_size_limit_tb";
    public const string AzureDatabricksEnterpriseOid = "azure_databricks_enterprise_oid";
    public const string AzureLogWorkspaceId = "log_workspace_id";

    /// <summary>
    /// Default outbound IP address for the Datahub Portal
    /// </summary>
    public const string AllowSourceIp = "allow_source_ip";

    public const string MapType = "map";
    public const string ListAnyType = "list(any)";
}

public static class TerraformAzureAppServiceDefaults
{
    public const string AppServiceSku = "app_service_sku";
    public const string WorkerCountInitial = "worker_count_init";
}