from azure.identity import DefaultAzureCredential
from azure.mgmt.keyvault import KeyVaultManagementClient
import os

def get_keyvault_roles(vault_name, user_id):
    try:
        subscription_id = os.getenv("AZURE_SUBSCRIPTION_ID")
        # Authenticate using DefaultAzureCredential
        credential = DefaultAzureCredential()
        
        # Initialize KeyVaultManagementClient
        client = KeyVaultManagementClient(credential, subscription_id)
        
        # Get the Key Vault resource group name
        vault = client.vaults.get('rg', vault_name)
        resource_group_name = vault.id.split('/')[4]
        
        # List role assignments for the Key Vault
        role_assignments = client.role_assignments.list_for_scope(
            scope=f"/subscriptions/{subscription_id}/resourceGroups/{resource_group_name}/providers/Microsoft.KeyVault/vaults/{vault_name}"
        )
        
        # Filter role assignments for the specified user
        user_roles = [role for role in role_assignments if role.principal_id == user_id]
        
        return user_roles
    except Exception as e:
        print(f"An error occurred: {e}")
        return None