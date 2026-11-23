from azure.mgmt.keyvault import KeyVaultManagementClient
from azure.identity import ClientSecretCredential
from azure.mgmt.keyvault import KeyVaultManagementClient
from azure.mgmt.keyvault.models import AccessPolicyEntry, VaultAccessPolicyParameters, SecretPermissions
import os
import threading
import lib.constants as constants
import logging

logger = logging.getLogger(__name__)

_keyvault_operation_lock = threading.Semaphore(1)

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
    logger.info("using vault: [%s].[%s]", rg_name, vault_name)
    
    vault = client.vaults.get(rg_name, vault_name)
    # Get a list of secrets    
    secrets = client.secrets.list(rg_name,vault_name)
    return secrets

def synchronize_access_policies(client:KeyVaultManagementClient, environment_name, definition_json, tenant_id):
    rg_name, vault_name = get_kv_reference(environment_name, definition_json)
    logger.info("using vault: [%s].[%s]", rg_name, vault_name)

    _keyvault_operation_lock.acquire()
    try:
        vault = client.vaults.get(rg_name, vault_name)
        current_policies = list(vault.properties.access_policies or [])

        for user in (user for user in definition_json['Workspace']['Users'] if user['Role'] != 'Removed'):
            user_id = user['ObjectId']
            permissions = ["list", "get"]
            if user['Role'] in {'Admin', 'Owner'}:
                permissions = ["list", "get", "delete", "set"]

            user_exists = False
            valid_permissions = False
            existing_policy = None
            access_policy = AccessPolicyEntry(
                tenant_id=tenant_id,
                object_id=user_id,
                permissions={'secrets': permissions},
            )

            for policy in current_policies:
                if policy.object_id == user_id:
                    user_exists = True
                    existing_policy = policy
                    if set(policy.permissions.secrets) == set(permissions):
                        valid_permissions = True
                    break

            if not user_exists:
                logger.info("adding user %s to access policies", user_id)
                current_policies.append(access_policy)
            elif not valid_permissions:
                logger.info("updating permissions for user %s to access policies", user_id)
                current_policies.remove(existing_policy)
                current_policies.append(access_policy)
            else:
                logger.info("user %s already exists in access policies - permissions are valid: %s", user['ObjectId'], valid_permissions)

        removed_users = [user for user in definition_json['Workspace']['Users'] if user['Role'] == 'Removed']
        for policy in list(current_policies):
            if policy.object_id in (user['ObjectId'] for user in removed_users):
                logger.info("removing user %s from access policies", policy.object_id)
                current_policies.remove(policy)

        vault.properties.access_policies = current_policies
        keyvault_poller = client.vaults.begin_create_or_update(
            rg_name, vault_name, vault
        )
        return keyvault_poller.result()
    finally:
        _keyvault_operation_lock.release()

def get_kv_reference(environment_name, definition_json):
    rg_name = f"{constants.RESOURCE_PREFIX}_proj_{definition_json['Workspace']['Acronym']}_{environment_name}_rg"
    vault_name = f"{constants.RESOURCE_PREFIX}-proj-{definition_json['Workspace']['Acronym']}-{environment_name}-kv"
    return rg_name,vault_name   



    

