Install-Module -Name Az -AllowClobber -Scope CurrentUser
#Install-Module -Name Az.Accounts -AllowClobber -Scope CurrentUser
Import-Module Az.KeyVault
$domain = "163oxygen.onmicrosoft.com"
$context = Get-AzContext
Write-Host "Checking if user is signed in on Azure"
if ($null -eq $context -or $context.Account.Tenants[0] -ne $domain) {
    connect-azaccount -Domain $domain -AuthScope AzureKeyVaultServiceEndpointResourceId
} else {
    Write-Output "User $($context.Account.Id) is signed in."
}

$env:AzureClientId = Get-AzKeyVaultSecret -VaultName "fsdh-key-dev" -Name "devops-client-id" -AsPlainText
$env:AzureClientSecret = Get-AzKeyVaultSecret -VaultName "fsdh-key-dev" -Name "devops-client-secret" -AsPlainText
$env:DataHub_ENVNAME = "dev"
$env:AZURE_SUBSCRIPTION_ID = "bc4bcb08-d617-49f4-b6af-69d6f10c240b"
$env:AzureSubscriptionId = $env:AZURE_SUBSCRIPTION_ID
$env:AzureTenantId = $context.Tenant.Id

