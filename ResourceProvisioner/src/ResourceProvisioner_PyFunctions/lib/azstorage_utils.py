from azure.identity import ClientSecretCredential
from azure.mgmt.authorization import AuthorizationManagementClient
from azure.mgmt.authorization.models import RoleAssignmentCreateParameters
import logging
import os
import uuid
import lib.constants as constants

logger = logging.getLogger(__name__)

CONTRIBUTOR="ba92f5b4-2d11-453d-a403-e96b0029c9fe"
READER="acdd72a7-3385-48ef-bd42-f606fba81ae7"

def _role_definition_guid(role_definition_id):
    """Return the GUID portion of a role definition resource identifier."""
    return role_definition_id.split("/")[-1].lower()

def _is_managed_blob_role(role_definition_id):
    """Return whether the supplied role definition is one of the managed blob roles."""
    role_id = _role_definition_guid(role_definition_id)
    return role_id == READER or role_id == CONTRIBUTOR

def assign_blob_role(auth_client, subscription_id, scope, user_object_id, read_only):
    """Assign the appropriate blob reader/contributor role to a user for the supplied scope."""
    if (read_only):
        roleId = READER
    else:
        roleId = CONTRIBUTOR
    # Define the role assignment parameters
    # see https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles for details
    role_assignment_parameters = RoleAssignmentCreateParameters(
        role_definition_id="/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{roleId}".format(subscriptionId=subscription_id,roleId=roleId),
        principal_id=user_object_id
    )

    # Assign the role
    auth_client.role_assignments.create(
        scope=scope,
        role_assignment_name=uuid.uuid4(),
        parameters=role_assignment_parameters
    )

def remove_existing_role(client:AuthorizationManagementClient, scope, user_object_id):
    """Remove any managed blob role assignments for the specified user at the given scope."""
    role_assignments = client.role_assignments.list_for_scope(scope)
    for role in role_assignments:
        if role.principal_id == user_object_id and _is_managed_blob_role(role.role_definition_id):
            client.role_assignments.delete_at_scope(scope, role.name)

def get_storage_reference(environment_name, definition_json):
    """Build the resource group and storage account names for the workspace environment."""
    rg_name = f"{constants.RESOURCE_PREFIX}_proj_{definition_json['Workspace']['Acronym']}_{environment_name}_rg"
    sg_name = f"{constants.RESOURCE_PREFIX}proj{definition_json['Workspace']['Acronym']}{environment_name}"
    return rg_name,sg_name

def get_scope(subscription_id, resource_group_name, storage_account_name):
    """Build the Azure resource scope for a storage account."""
    return f"/subscriptions/{subscription_id}/resourceGroups/{resource_group_name}/providers/Microsoft.Storage/storageAccounts/{storage_account_name}"

def get_blob_container_scope(subscription_id, resource_group_name, storage_account_name, container_name):
    """Build the Azure resource scope for a blob container."""
    return f"/subscriptions/{subscription_id}/resourceGroups/{resource_group_name}/providers/Microsoft.Storage/storageAccounts/{storage_account_name}/blobServices/default/containers/{container_name}"

def get_blob_container_names(definition_json):
    """Extract blob container names from the workspace definition payload."""
    names = set()
    templates = definition_json.get('Templates') or []
    for template in templates:
        if not isinstance(template, dict):
            continue
        if template.get('Name') not in ('azure-storage-blob', 'terraform:azure-storage-blob'):
            continue
        _collect_container_names(template, names)
        for key in ['Config', 'Configuration', 'Parameters', 'Properties', 'Values', 'Input', 'Inputs']:
            _collect_container_names(template.get(key), names)

    app_data = definition_json.get('AppData')
    if isinstance(app_data, dict):
        _collect_container_names(app_data, names)

    return sorted(names)

def _collect_container_names(source, names, include_name_field=False):
    """Recursively collect container names from nested definition data structures."""
    if source is None:
        return
    if isinstance(source, str):
        source_name = source.strip()
        if source_name:
            names.add(source_name)
        return
    if isinstance(source, list):
        for item in source:
            _collect_container_names(item, names, include_name_field)
        return
    if not isinstance(source, dict):
        return

    for key in ["Containers", "ContainerNames", "BlobContainers", "StorageContainers"]:
        if key in source:
            _collect_container_names(source.get(key), names, True)

    for key in ["Container", "ContainerName"]:
        value = source.get(key)
        if isinstance(value, str):
            value = value.strip()
            if value:
                names.add(value)

    if include_name_field:
        value = source.get("Name")
        if isinstance(value, str):
            value = value.strip()
            if value:
                names.add(value)

def get_blob_container_scopes(subscription_id, resource_group_name, storage_account_name, blob_containers):
    """Build the Azure scopes for each blob container to synchronize."""
    if not blob_containers:
        raise ValueError("No blob containers found in workspace definition for storage role synchronization")

    return [
        get_blob_container_scope(subscription_id, resource_group_name, storage_account_name, container_name)
        for container_name in blob_containers
    ]

def check_blob_reader_role(client:AuthorizationManagementClient, scope, user_object_id):
    """Return whether the user has the managed blob reader role for the supplied scope."""
    role_assignments = client.role_assignments.list_for_scope(scope)
    for role in role_assignments:
        if role.principal_id == user_object_id:
            return _role_definition_guid(role.role_definition_id) == READER
    return None

def get_authorization_client(subscription_id, tenant_id) -> AuthorizationManagementClient:
    """
    Retrieves an Authorization Management client for the specified environment and workspace definition.

    Args:
        subscription_id (str): The subscription id.
        tenant_id (str): The tenant id.

    Returns:
        auth_client: The Authorization Management client object.

    """
    credential = ClientSecretCredential(
        tenant_id=tenant_id,
        client_id=os.environ["AzureClientId"],
        client_secret=os.environ["AzureClientSecret"]
    )

    auth_client = AuthorizationManagementClient(
        credential=credential,
        subscription_id=subscription_id
    )
    
    return auth_client    

def synchronize_access_policies(client:AuthorizationManagementClient, subscription_id, environment_name, definition_json, blob_containers):
    """Synchronize blob access policies for workspace users based on their role assignments."""
    (rg_name,sg_account) = get_storage_reference(environment_name, definition_json)
    scopes = get_blob_container_scopes(subscription_id, rg_name, sg_account, blob_containers)
    # iterate through definition_json['Workspace']['Acronym']
    for user in (user for user in definition_json['Workspace']['Users'] if user['Role'] != 'Removed'):
        user_id = user['ObjectId']
        logger.info("Processing user %s access policies", user_id)
        try:
            for scope in scopes:
                reader_role = check_blob_reader_role(client, scope, user_id)
                if user['Role'] == 'Guest':
                    if reader_role is None:
                        logger.info("Assigning user %s to access policies - read-only", user_id)
                        assign_blob_role(client, subscription_id, scope, user_id, True)
                    elif not reader_role:
                        logger.info("Assigning user %s to access policies - read-only", user_id)
                        remove_existing_role(client, scope, user_id)
                        assign_blob_role(client, subscription_id, scope, user_id, True)
                    else:
                        logger.info("User %s already has read-only access policies", user_id)
                else:
                    if reader_role is None:
                        logger.info("Assigning user %s to access policies - read-write", user_id)
                        assign_blob_role(client, subscription_id, scope, user_id, False)
                    elif reader_role:
                        logger.info("Assigning user %s to access policies - read-write", user_id)
                        remove_existing_role(client, scope, user_id)
                        assign_blob_role(client, subscription_id, scope, user_id, False)
                    else:
                        logger.info("User %s already has read-write access policies", user_id)
        except Exception:
            logger.exception("Error processing user %s access policies", user_id)
    for user in (user for user in definition_json['Workspace']['Users'] if user['Role'] == 'Removed'):
        try:
            logger.info("Removing user %s from access policies", user['ObjectId'])
            for scope in scopes:
                remove_existing_role(client, scope, user['ObjectId'])
        except Exception:
            logger.exception("Error processing user %s access policies", user['ObjectId'])
