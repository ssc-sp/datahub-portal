Install-Module -Name Az -AllowClobber -Scope CurrentUser
#Install-Module -Name Az.Accounts -AllowClobber -Scope CurrentUser
Import-Module Az
$env:AzureTenantId = "8c1a4d93-d828-4d0e-9303-fd3bd611c822"
Connect-AzAccount -Tenant $env:AzureTenantId
$env:AzureClientId = Get-AzKeyVaultSecret -VaultName "fsdh-key-dev" -Name "devops-client-id" -AsPlainText
$env:AzureClientSecret = Get-AzKeyVaultSecret -VaultName "fsdh-key-dev" -Name "devops-client-secret" -AsPlainText
$env:DataHub_ENVNAME = "dev"
$env:AZURE_SUBSCRIPTION_ID = "bc4bcb08-d617-49f4-b6af-69d6f10c240b"
