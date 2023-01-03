namespace Datahub.Shared;

public static class TerraformVariables
{
    
    public static readonly string OutputProjectAcronym = "project_cd";
    public static readonly string OutputNewProjectTemplate = "new_project_template";
    
    public static readonly string AzureStorageBlobTemplateName = "azure-storage-blob";
    public static readonly string AzureStorageType = "blob";
    public static readonly string OutputAzureStorageAccountName = "azure_storage_account_name";
    public static readonly string OutputAzureStorageContainerName = "azure_storage_container_name";
    public static readonly string OutputAzureStorageBlobStatus = "azure_storage_blob_status";
    
    public static readonly string AzureDatabricksTemplateName = "azure-databricks";
    public static readonly string OutputAzureDatabricksUrl = "azure_databricks_workspace_url";
    public static readonly string OutputAzureDatabricksStatus = "azure_databricks_workspace_status";
    public static readonly string OutputAzureDatabricksWorkspaceId = "azure_databricks_workspace_id";
    public static readonly string OutputAzureDatabricksWorkspaceName = "azure_databricks_workspace_name";
    
}