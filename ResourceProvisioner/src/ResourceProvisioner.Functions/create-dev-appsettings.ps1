Write-Output "Fetching secrets from keyvault"

$environmentName = "dev"
$resourcePrefix = "fsdh"
$azureDevOpsOrganization = "DataSolutionsDonnees"
$azureDevOpsProject = "FSDH%20SSC"
$vaultName = "fsdh-key-dev"
$azureDevopsRepository = "datahub-project-infrastructure-$environmentName"

$tenantId = (az keyvault secret show --name "datahub-portal-tenant-id" --vault-name $vaultName --query value -o tsv)
$subscriptionId = (az keyvault secret show --name "datahub-portal-subscription-id" --vault-name $vaultName --query value -o tsv)
$repositoryId = (az keyvault secret show --name "datahub-infrastructure-repo-id" --vault-name $vaultName --query value -o tsv)

$hashTable = @{
    "ModuleRepository" = @{
        "Url" = "https://github.com/ssc-sp/datahub-resource-modules.git"
        "LocalPath" = "/tmp/datahub-resource-modules"
        "TemplatePathPrefix" = "templates/"
        "ModulePathPrefix" = "modules/"
    }
    "InfrastructureRepository" = @{
        "Url" = "https://$azureDevOpsOrganization@dev.azure.com/$azureDevOpsOrganization/$azureDevOpsProject/_git/$azureDevopsRepository"
        "LocalPath" = "/tmp/$azureDevopsRepository"
        "ProjectPathPrefix" = "terraform/projects"
        "PullRequestUrl" = "https://dev.azure.com/$azureDevOpsOrganization/$azureDevOpsProject/_apis/git/repositories/$repositoryId/pullrequests"
        "PullRequestBrowserUrl" = "https://dev.azure.com/$azureDevOpsOrganization/$azureDevOpsProject/_git/$azureDevopsRepository/pullrequest"
        "ApiVersion" = "7.1-preview.1"
        "MainBranch" = "main"
        "AzureDevOpsConfiguration" = @{
            "TenantId" = "$tenantId"
            "ClientId" = (az keyvault secret show --name "devops-client-id" --vault-name $vaultName --query value -o tsv)
            "ClientSecret" = (az keyvault secret show --name "devops-client-secret" --vault-name $vaultName --query value -o tsv)
        }
    }
    "Terraform" = @{
        "Backend" = @{
            "ResourceGroupName" = "$resourcePrefix-core-$environmentName-rg"
        }        
        "Variables" = @{
            "az_subscription_id" = $subscriptionId
            "az_tenant_id" = "$tenantId"
            "datahub_app_sp_oid" = (az keyvault secret show --name "datahubportal-client-oid" --vault-name $vaultName --query value -o tsv)
            "azure_databricks_enterprise_oid" = (az keyvault secret show --name "datahub-databricks-sp" --vault-name $vaultName --query value -o tsv)
            "environment_classification" = "U"
            "environment_name" = "$environmentName"
            "budget_amount" = "400"
            "aad_admin_group_oid" = (az keyvault secret show --name "aad-admin-group-oid" --vault-name $vaultName --query value -o tsv)
            "storage_size_limit_tb" = "5"
            "az_location" = "canadacentral"
            "resource_prefix" = "$resourcePrefix"
            "common_tags" = @{
                "Sector" = "Science Program"
                "Environment" = "$environmentName"
                "ClientOrganization" = "TEST-TEST"
            }
        }
    }
}

Write-Output "Converting secrets to json object"
$jsonObject = $hashTable | ConvertTo-Json -Depth 100

Write-Output "Writing json object to appsettings.test.json"
$jsonObject | Out-File -FilePath "appsettings.test.json"

Write-Output "Done"