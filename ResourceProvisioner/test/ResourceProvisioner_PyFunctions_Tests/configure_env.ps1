# Check if Az module is installed
if (-not (Get-Module -ListAvailable -Name Az)) {
    Write-Host "Az module not found. Installing..."
    Install-Module -Name Az -AllowClobber -Scope CurrentUser
} else {
    Write-Host "Az module is already installed."
}

# Check if Az.KeyVault module is installed
if (-not (Get-Module -ListAvailable -Name Az.KeyVault)) {
    Write-Host "Az.KeyVault module not found. Installing..."
    Install-Module -Name Az.KeyVault -AllowClobber -Scope CurrentUser
} else {
    Write-Host "Az.KeyVault module is already installed."
}
$domain = "163oxygen.onmicrosoft.com"
$env:AZURE_SUBSCRIPTION_ID = "bc4bcb08-d617-49f4-b6af-69d6f10c240b"
$context = Get-AzContext
Write-Host "Checking if user is signed in on Azure"
if ($null -eq $context -or $context.Account.Tenants[0] -ne $domain) {
    #-AuthScope AzureKeyVaultServiceEndpointResourceId 
    connect-azaccount -Subscription $env:AZURE_SUBSCRIPTION_ID -Domain $domain
} else {
    Write-Output "User $($context.Account.Id) is signed in."
}

$env:AzureClientId = Get-AzKeyVaultSecret -VaultName "fsdh-key-dev" -Name "devops-client-id" -AsPlainText
$env:AzureClientSecret = Get-AzKeyVaultSecret -VaultName "fsdh-key-dev" -Name "devops-client-secret" -AsPlainText
$env:DataHub_ENVNAME = "dev"
$env:AzureSubscriptionId = $env:AZURE_SUBSCRIPTION_ID
$env:AzureTenantId = $context.Account.Tenants[0]

