from azure.mgmt.keyvault import KeyVaultManagementClient
from azure.identity import ClientSecretCredential
from azure.mgmt.keyvault import KeyVaultManagementClient
from azure.mgmt.keyvault.models import AccessPolicyEntry, VaultAccessPolicyParameters, SecretPermissions
import os
import lib.constants as constants

def get_keyvault_client(subscription_id, tenant_id) -> KeyVaultManagementClient:
    """
    Retrieves a Key Vault client for the specified environment and workspace definition.

    Args:
        environment_name (str): The name of the environment.
        definition_json (dict): The definition of the workspace.

    Returns:
        keyvault_client: The Key Vault client object.

    """
    credential = ClientSecretCredential(
        tenant_id=tenant_id,
        client_id=os.environ["AzureClientId"],
        client_secret=os.environ["AzureClientSecret"])    

    kv_client = KeyVaultManagementClient(
        credential=credential,
        subscription_id=subscription_id
    )
    
    return kv_client


def get_keyvault_uri(keyvault_name):
    # Replace these values with your Azure Key Vault details
    return f"https://{keyvault_name}.vault.azure.net/"

def list_secrets(client:KeyVaultManagementClient, environment_name, definition_json):
    rg_name, vault_name = get_kv_reference(environment_name, definition_json)
    print(f"using vault: [{rg_name}].[{vault_name}]")
    
    vault = client.vaults.get(rg_name, vault_name)
    # Get a list of secrets    
    secrets = client.secrets.list(rg_name,vault_name)
    return secrets

def synchronize_access_policies(client:KeyVaultManagementClient, environment_name, definition_json, tenant_id):
    rg_name, vault_name = get_kv_reference(environment_name, definition_json)
    print(f"using vault: [{rg_name}].[{vault_name}]")
    # Replace these values with your Azure Key Vault details

    # Create a SecretClient using the default Azure credential from Azure Identity

    # Get access policies
    vault = client.vaults.get(rg_name, vault_name)

    current_policies = vault.properties.access_policies
    # iterate through definition_json['Workspace']['Acronym']
    for user in (user for user in definition_json['Workspace']['Users'] if user['Role'] != 'Removed'):
        user_id = user['ObjectId']
        # Define the access policy
        permissions = ["list","get"]
        if (user['Role'] == 'Admin' or user['Role'] == 'Owner'):
            permissions = ["list","get","delete","set"]        
        # check if user exists in access policies
        user_exists = False
        valid_permissions = False
        existing_policy = None
        access_policy = AccessPolicyEntry(tenant_id=tenant_id, object_id=user_id, 
                                        permissions={'secrets': permissions})
                
        for policy in vault.properties.access_policies:
            if policy.object_id == user_id:
                user_exists = True
                existing_policy = policy
                if (set(policy.permissions.secrets) == set(permissions)):
                    valid_permissions = True
                break
        # if user does not exist, add user to access policies
        if not user_exists:
            print(f"adding user {user_id} to access policies")
            # add user to access policies                        
            #vault = clients.kv_client.vaults.get(rg_name, vault_name)
            #print(f"User {user} has permissions: {policy.permissions}")
            #print(policy)        
            # enable secret list,get,delete,set,update permissions for user
            current_policies.append(access_policy)
        elif not valid_permissions:
            print(f"updating permissions for user {user_id} in access policies")
            # update permissions for user
            current_policies.remove(existing_policy)
            current_policies.append(access_policy)
                            
        else:
            print(f"user {user['ObjectId']} already exists in access policies - permissions are valid: {valid_permissions}")
    # collect all the object ids
    #object_ids = [policy.object_id for policy in vault.properties.access_policies]
    #output = asyncio.run(collect_ms_graph_properties(clients,object_ids))
    # Print access policies
    removed_users = [user for user in definition_json['Workspace']['Users'] if user['Role'] == 'Removed']    
    for policy in vault.properties.access_policies:
        if policy.object_id in (user['ObjectId'] for user in removed_users):
            print(f"removing user {policy.object_id} from access policies")
            current_policies.remove(policy)
            #vault = clients.kv_client.vaults.get(rg_name, vault_name)
            #        
        # print(f"User {user} has permissions: {policy.permissions}")
        # print(policy)        
    # Update the vault with the new policies
    vault.properties.access_policies = current_policies
    keyvault_poller = client.vaults.begin_create_or_update(
        rg_name, vault_name, vault
    )

    return keyvault_poller.result() 

def get_kv_reference(environment_name, definition_json):
    rg_name = f"{constants.RESOURCE_PREFIX}_proj_{definition_json['Workspace']['Acronym']}_{environment_name}_rg"
    vault_name = f"{constants.RESOURCE_PREFIX}-proj-{definition_json['Workspace']['Acronym']}-{environment_name}-kv"
    return rg_name,vault_name   



    

