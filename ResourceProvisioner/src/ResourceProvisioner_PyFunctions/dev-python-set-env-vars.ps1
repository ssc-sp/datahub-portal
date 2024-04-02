echo "Setting environment variables from Azure Key Vault"

$env:AzureClientId = (az keyvault secret show --name "devops-client-id" --vault-name "fsdh-key-dev" --query value -o tsv)
$env:AzureClientSecret = (az keyvault secret show --name "devops-client-secret" --vault-name "fsdh-key-dev" --query value -o tsv)
$env:AzureTenantId = "8c1a4d93-d828-4d0e-9303-fd3bd611c822"

echo "Environment variables set"