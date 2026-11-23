from azure.identity import ClientSecretCredential
from azure.mgmt.authorization import AuthorizationManagementClient
from azure.mgmt.authorization.models import RoleAssignmentCreateParameters
import logging
import os
import uuid
import lib.constants as constants
logger = logging.getLogger(__name__)

BLOB_DATA_CONTRIBUTOR = "ba92f5b4-2d11-453d-a403-e96b0029c9fe"
BLOB_DATA_READER = "2a2b9908-6ea1-4ae2-8e65-a410df84e7d1"
STORAGE_ACCOUNT_READER = "acdd72a7-3385-48ef-bd42-f606fba81ae7"

def _role_definition_guid(role_definition_id):
    """Return the GUID portion of a role definition resource identifier."""
    return role_definition_id.split("/")[-1].lower()

def _is_managed_blob_role(role_definition_id):
    """Return whether the supplied role definition is one of the managed blob roles."""
    role_id = _role_definition_guid(role_definition_id)
    return role_id == BLOB_DATA_READER or role_id == BLOB_DATA_CONTRIBUTOR

def _is_managed_storage_role(role_definition_id):
    """Return whether the supplied role definition is managed by storage access synchronization."""
    role_id = _role_definition_guid(role_definition_id)
    return role_id == STORAGE_ACCOUNT_READER or _is_managed_blob_role(role_definition_id)

def assign_role(auth_client, subscription_id, scope, user_object_id, role_id):
    """Assign a built-in Azure role to a user for the supplied scope."""
    role_assignment_parameters = RoleAssignmentCreateParameters(
        role_definition_id="/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{roleId}".format(subscriptionId=subscription_id, roleId=role_id),
        principal_id=user_object_id
    )

    auth_client.role_assignments.create(
        scope=scope,
        role_assignment_name=uuid.uuid4(),
        parameters=role_assignment_parameters
    )

def assign_blob_role(auth_client, subscription_id, scope, user_object_id, read_only):
    """Assign the appropriate blob reader/BLOB_DATA_CONTRIBUTOR role to a user for the supplied scope."""
    if (read_only):
        roleId = BLOB_DATA_READER
    else:
        roleId = BLOB_DATA_CONTRIBUTOR
    assign_role(auth_client, subscription_id, scope, user_object_id, roleId)

def assign_storage_account_reader(auth_client, subscription_id, scope, user_object_id):
    """Assign Storage Account Reader at the storage account scope."""
    assign_role(auth_client, subscription_id, scope, user_object_id, STORAGE_ACCOUNT_READER)

def remove_existing_role(client:AuthorizationManagementClient, scope, user_object_id):
    """Remove any managed storage role assignments for the specified user at the given scope."""
    role_assignments = client.role_assignments.list_for_scope(scope)
    for role in role_assignments:
        if role.principal_id == user_object_id and _is_managed_storage_role(role.role_definition_id):
            if hasattr(client.role_assignments, "delete"):
                client.role_assignments.delete(scope, role.name)
            else:
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

def has_blob_data_role(client:AuthorizationManagementClient, scope, user_object_id, role_id):
    """Check whether the user has the specified blob data role at the supplied scope.

    Returns:
        True: The user has an assignment for the specified role.
        False: The user has one or more assignments, but none is for the specified role.
        None: The user has no assignments at this scope.

    The distinction between False and None lets synchronization replace an
    incorrect role assignment instead of treating it as a missing assignment.
    """
    user_assignments = list(filter(
        lambda role: role.principal_id == user_object_id,
        client.role_assignments.list_for_scope(scope),
    ))
    return (
        any(map(
            lambda role: _role_definition_guid(role.role_definition_id) == role_id,
            user_assignments,
        ))
        if user_assignments
        else None
    )

def check_role_assignment(client:AuthorizationManagementClient, scope, user_object_id, role_id):
    """Return whether the user has the specified role assignment at the supplied scope."""
    role_assignments = client.role_assignments.list_for_scope(scope)
    return any(
        role.principal_id == user_object_id
        and _role_definition_guid(role.role_definition_id) == role_id
        for role in role_assignments
    )

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

def synchronize_blob_access_policies(client:AuthorizationManagementClient, subscription_id, environment_name, definition_json, blob_containers):
    """Synchronize blob access policies for workspace users based on their role assignments."""
    (rg_name,sg_account) = get_storage_reference(environment_name, definition_json)
    storage_account_scope = get_scope(subscription_id, rg_name, sg_account)
    scopes = get_blob_container_scopes(subscription_id, rg_name, sg_account, blob_containers)
    # iterate through definition_json['Workspace']['Acronym']
    for user in (user for user in definition_json['Workspace']['Users'] if user['Role'] != 'Removed'):
        user_id = user['ObjectId']
        logger.info("Processing user %s access policies", user_id)
        try:
            if not check_role_assignment(client, storage_account_scope, user_id, STORAGE_ACCOUNT_READER):
                logger.info("Assigning user %s Storage Account Reader at account scope", user_id)
                assign_storage_account_reader(client, subscription_id, storage_account_scope, user_id)

            # Container listing is a data-plane operation and requires the Blob Data Reader
            # role at the storage-account scope, in addition to the management-plane Reader role.
            if not check_role_assignment(client, storage_account_scope, user_id, BLOB_DATA_READER):
                logger.info("Assigning user %s Blob Data Reader at account scope", user_id)
                assign_blob_role(client, subscription_id, storage_account_scope, user_id, True)

            for scope in scopes:
                if user['Role'] == 'Guest':
                    reader_role = has_blob_data_role(client, scope, user_id, BLOB_DATA_READER)
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
                    # contributor, lead, admin, owner
                    contributor_role = has_blob_data_role(
                        client, scope, user_id, BLOB_DATA_CONTRIBUTOR
                    )
                    if contributor_role is None:
                        logger.info("Assigning user %s to access policies - read-write", user_id)
                        assign_blob_role(client, subscription_id, scope, user_id, False)
                    elif not contributor_role:
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
            remove_existing_role(client, storage_account_scope, user['ObjectId'])
            for scope in scopes:
                remove_existing_role(client, scope, user['ObjectId'])
        except Exception:
            logger.exception("Error processing user %s access policies", user['ObjectId'])
