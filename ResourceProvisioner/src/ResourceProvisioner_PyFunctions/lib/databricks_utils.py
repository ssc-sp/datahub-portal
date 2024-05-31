from databricks.sdk import WorkspaceClient
from databricks.sdk.service.iam import ComplexValue
from databricks.sdk.service.workspace import AzureKeyVaultSecretScopeMetadata
from databricks.sdk.service.workspace import ScopeBackendType
import lib.azkeyvault_utils as azkv_utils
import lib.constants as constants
import os
import logging

WORKSPACE_KV_SCOPE_NAME = "dh-workspace"

def get_definition_role_lookup():
    """
    Returns a dictionary that maps the definition role to the workspace group display name.

    Returns:
        dict: A dictionary that maps the definition role to the workspace group display name.
    """
    definition_role_lookup = {
        'Owner': 'project_lead',
        'Admin': 'admins',
        'User': 'project_users',
        'Guest': 'project_users'
    }
    return definition_role_lookup

def get_workspace_client(databricksHost):
    """
    Returns a WorkspaceClient object for the given databricks host and token.

    Args:
        databricksHost (str): The databricks host url.

    Returns:
        WorkspaceClient: The databricks workspace client.
    """

    w = WorkspaceClient(
        host=databricksHost,
        azure_client_secret = os.environ["AzureClientSecret"],
        azure_client_id = os.environ["AzureClientId"],
        azure_tenant_id = os.environ["AzureTenantId"],
        auth_type='azure-client-secret'
    )
    return w

def get_workspace_groups(workspace_client):
    """
    Returns a dictionary of workspace groups keyed off their display name.

    Args:
        workspace_client (WorkspaceClient): The databricks workspace client.

    Returns:
        dict: A dictionary of workspace groups keyed off their display name.
    """
    workspace_groups = {g.display_name: g for g in workspace_client.groups.list()}
    return workspace_groups

def remove_deleted_users_in_workspace(definition_json, workspace_client):
    """
    Removes all users from the workspace that do not have an external ID.

    Args:
        workspace_client (WorkspaceClient): The databricks workspace client.

    Returns:
        None
    """
    # remove users with removed role
    removedIds = list(user['ObjectId'] for user in definition_json['Workspace']['Users'] if (user['Role'] == 'Removed'))
    logging.info(f'Users to remove: {removedIds}')
    toRemove = []
    for user in workspace_client.users.list():
        if user.external_id is None:
            logging.info(f'User {user.user_name} does not have an external ID, removing from workspace')
            workspace_client.users.delete(user.id)
        else:
            if user.external_id in removedIds:
                toRemove.append(user)
            logging.info(f'User {user.user_name} with external ID {user.external_id} exists')
    for user in toRemove:
        logging.info(f'User {user.user_name} with external ID {user.external_id} is marked for removal')
        workspace_client.users.delete(user.id)

def synchronize_workspace_secrets(environment_name, subscription_id, definition_json, workspace_client):
    azure_tenant_id = os.environ["AzureTenantId"]    
    kv_client = azkv_utils.get_keyvault_client(subscription_id, azure_tenant_id)
    secret_list = azkv_utils.list_secrets(kv_client, environment_name, definition_json)
    for secret in secret_list:
        logging.info(f"adding secret: {secret.name} to workspace")
        workspace_client.secrets.put_secret(scope=WORKSPACE_KV_SCOPE_NAME, key=secret.name)

def synchronize_workspace_secret_scopes(environment_name, subscription_id, definition_json, workspace_client):
    """
    Synchronizes the workspace secret scopes with the secret scopes defined in the definition file.

    Will iterate over each secret scope in the definition file and check if they exist in the workspace.
        - If the secret scope exists in the workspace, it will be checked to see if it has the correct ACLs.
        - If the secret scope does not exist in the workspace, it will be created.

    Args:
        definition_json (dict): The workspace definition file as a dictionary (json).
        workspace_client (WorkspaceClient): The databricks workspace client.

    Returns:
        None
    """
    # see https://databricks-sdk-py.readthedocs.io/en/latest/autogen/workspace.html#databricks.sdk.service.workspace.SecretsAPI    
    rg_name, vault_name = azkv_utils.get_kv_reference(environment_name, definition_json)    

    logging.info(f"using vault: [{rg_name}].[{vault_name}]")
    kv_uri = azkv_utils.get_keyvault_uri(vault_name.lower())
    resource_id = f"/subscriptions/{subscription_id}/resourcegroups/{rg_name.lower()}/providers/Microsoft.KeyVault/vaults/{vault_name.lower()}"
    workspace_secret_scopes = workspace_client.secrets.list_scopes()
    # for workspace_secret_scope in workspace_secret_scopes:
    #     logging.info(f"Deleting secret scope {workspace_secret_scope.name}")
    #     workspace_client.secrets.delete_scope(scope=workspace_secret_scope.name)
    # Check if WORKSPACE_KV_SCOPE_NAME exists   
    workspace_kv_scope_found = False
    for workspace_secret_scope in workspace_secret_scopes:
        if workspace_secret_scope.name == WORKSPACE_KV_SCOPE_NAME:
            workspace_kv_scope_found = True
            break
    if not workspace_kv_scope_found:
        logging.info(f"Secret scope {WORKSPACE_KV_SCOPE_NAME} does not exist in workspace")
        azKeyVault = AzureKeyVaultSecretScopeMetadata(resource_id=resource_id,dns_name=kv_uri)
        workspace_client.secrets.create_scope(scope=WORKSPACE_KV_SCOPE_NAME, initial_manage_principal='users', 
                                              scope_backend_type = ScopeBackendType.AZURE_KEYVAULT,
                                              backend_azure_keyvault=azKeyVault)
        logging.info(f"Secret scope {WORKSPACE_KV_SCOPE_NAME} created in workspace")
    else:
        logging.info(f"Secret scope {WORKSPACE_KV_SCOPE_NAME} already exists in workspace")    

def synchronize_workspace_users(definition_json, workspace_client, retries = 0):
    """
    Synchronizes the workspace users with the users defined in the definition file.

    Will iterate over each user in the definition file and check if they exist in the workspace.
        - If the user exists in the workspace, they will be checked to see if they are in the correct group.
        - If the user does not exist in the workspace, they will be created.

    Args:
        definition_json (dict): The workspace definition file as a dictionary (json).
        workspace_client (WorkspaceClient): The databricks workspace client.

    Returns:
        None
    """
    workspace_users = workspace_client.users.list()

    # Iterate over each user in the definition file
    # exclude users that have been removed
    for user in (user for user in definition_json['Workspace']['Users'] if (user['Role'] != 'Removed')):
        try:
            # Find the user matching user in the workspace
            user_found = False
            for workspace_user in workspace_users:
                # compare case insensitive

                if user['ObjectId'].lower() == workspace_user.external_id.lower():
                    user_found = True

                    # Check if the user is in the correct group
                    logging.info(f"Checking {user['Email']}'s has group {user['Role']} in workspace")
                    set_user_group_in_workspace(workspace_client, workspace_user, user)
                    
                    # Since we found the user, break out of the loop
                    break

            # If we didn't find the user, create them
            if not user_found:
                logging.info(f"User {user['Email']} does not exist in workspace")
                create_new_user_in_workspace(workspace_client, user)
        except Exception:
            logging.exception(f"Error synchronizing user {user['Email']} in workspace.")
            if (retries < 1):
                logging.info(f"Retrying to synchronize user {user['Email']} in workspace.")
                # search for user id by email
                for workspace_user in workspace_users:
                    logging.info(f"Checking {workspace_user} in workspace")
                    if user['Email'].lower() == workspace_user.user_name.lower():
                        logging.info(f"Deleting user {user['Email']} in workspace")
                        workspace_client.users.delete(workspace_user.id)
                        break
                synchronize_workspace_users(definition_json, workspace_client, retries + 1)

def create_new_user_in_workspace(workspace_client, user):
    """
    Creates a new user in the workspace and adds them to the correct group based on their role.

    Args:
        workspace_client (WorkspaceClient): The databricks workspace client.
        user (dict): The definition user to create in the workspace.

    Returns:
        workspace_user (User): The workspace user that was created.

    """
    logging.info(f"\tCreating user {user['Email']} in workspace")

    workspace_groups = get_workspace_groups(workspace_client)
    definition_role_lookup = get_definition_role_lookup()

    workspace_email = ComplexValue(value=user['Email'], 
                                display=None, 
                                primary=None, 
                                type='work')
    workspace_group = ComplexValue(value=workspace_groups[definition_role_lookup[user['Role']]].id, 
                                display=definition_role_lookup[user['Role']], 
                                primary=None, 
                                type=None)
    
    workspace_user = workspace_client.users.create(
        display_name=user['Email'], 
        user_name=user['Email'],
        emails=[workspace_email], 
        groups=[workspace_group],
        external_id=user['ObjectId'])
    logging.info(f"\tUser {user['Email']} created in workspace")

    return workspace_user

def set_user_group_in_workspace(workspace_client, workspace_user, definition_user):
    """
    Sets the user's group in the workspace based on their role.

    Args:
        workspace_client (WorkspaceClient): The databricks workspace client.
        workspace_user (User): The workspace user to update.
        definition_user (dict): The definition user to update.

    Returns:
        None
    """
    # If the user has no groups, update them to have the correct group based on their role
    if workspace_user.groups is None:
        add_user_to_group_in_workspace(workspace_client, workspace_user, definition_user)
    
    # If the user has groups, check if they have the correct groups
    else:
        update_user_group_in_workspace(workspace_client, workspace_user, definition_user)
        

def update_user_group_in_workspace(workspace_client, workspace_user, definition_user):
    """
    Updates the user's group in the workspace based on their role.

    Args:
        workspace_client (WorkspaceClient): The databricks workspace client.
        workspace_user (User): The workspace user to update.
        definition_user (dict): The definition user to update.

    Returns:
        None
    """
    workspace_groups = get_workspace_groups(workspace_client)
    definition_role_lookup = get_definition_role_lookup()

    group_found = False
    for group in workspace_user.groups:
        if group.display == definition_role_lookup[definition_user['Role']]:
            group_found = True
            logging.info(f"\tUser {definition_user['Email']} has correct group {group} in workspace")
            break
    if not group_found:
        logging.info(f"\tUser {definition_user['Email']} does not have correct groups {definition_user['Role']} in workspace, updating...")

        role_display_name = definition_role_lookup[definition_user['Role']]
        # throw exception if workspace_groups does not contain role_display_name
        if role_display_name not in workspace_groups:
            raise Exception(f"Role '{role_display_name}' not found in workspace groups")
        group_to_add = ComplexValue(value=workspace_groups[role_display_name].id, display=role_display_name, primary=None, type=None)

        logging.info(f"\tAdding missing group {group_to_add} to user {definition_user['Email']}")

        workspace_client.users.update(id=workspace_user.id, user_name=definition_user['Email'], groups=[group_to_add])
        logging.info(f"\tUser {definition_user['Email']} now has role {definition_user['Role']} in workspace (definition role: {definition_role_lookup[definition_user['Role']]}))")

def add_user_to_group_in_workspace(workspace_client, workspace_user, definition_user):
    """
    Adds the user to the correct group in the workspace based on their role.

    Args:
        workspace_client (WorkspaceClient): The databricks workspace client.
        workspace_user (User): The workspace user to update.
        definition_user (dict): The definition user to update.

    Returns:
        None
    """
    workspace_groups = get_workspace_groups(workspace_client)
    definition_role_lookup = get_definition_role_lookup()

    logging.info(f"\tUser {definition_user['Email']} has no groups in workspace, updating...")

    role_display_name = definition_role_lookup[definition_user['Role']]
    group_to_add = ComplexValue(value=workspace_groups[role_display_name].id, display=role_display_name, primary=None, type=None)

    logging.info(f"\tAdding new group {group_to_add} to user {definition_user['Email']}")

    workspace_client.users.update(id=workspace_user.id, user_name=definition_user['Email'], groups=[group_to_add])
    logging.info(f"\tUser {definition_user['Email']} is now in the {role_display_name} group in workspace")