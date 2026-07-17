from azure.identity import ClientSecretCredential
from azure.mgmt.authorization import AuthorizationManagementClient
from azure.mgmt.authorization.models import RoleAssignmentCreateParameters
import os
import uuid
import lib.constants as constants

CONTRIBUTOR="ba92f5b4-2d11-453d-a403-e96b0029c9fe"
READER="acdd72a7-3385-48ef-bd42-f606fba81ae7"

def _role_definition_guid(role_definition_id):
    return role_definition_id.split("/")[-1].lower()

def _is_managed_blob_role(role_definition_id):
    role_id = _role_definition_guid(role_definition_id)
    return role_id == READER or role_id == CONTRIBUTOR

def assign_blob_reader_role(auth_client, subscription_id, scope, user_object_id, read_only):
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
    role_assignments = client.role_assignments.list_for_scope(scope)
    for role in role_assignments:
        if role.principal_id == user_object_id and _is_managed_blob_role(role.role_definition_id):
            client.role_assignments.delete_at_scope(scope, role.name)

def get_storage_reference(environment_name, definition_json):
    rg_name = f"{constants.RESOURCE_PREFIX}_proj_{definition_json['Workspace']['Acronym']}_{environment_name}_rg"
    sg_name = f"{constants.RESOURCE_PREFIX}proj{definition_json['Workspace']['Acronym']}{environment_name}"
    return rg_name,sg_name

def get_scope(subscription_id, resource_group_name, storage_account_name):
    return "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Storage/storageAccounts/{storageAccountName}".format(subscriptionId=subscription_id, resourceGroupName=resource_group_name, storageAccountName=storage_account_name)

def get_blob_container_scope(subscription_id, resource_group_name, storage_account_name, container_name):
    return "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Storage/storageAccounts/{storageAccountName}/blobServices/default/containers/{containerName}".format(
        subscriptionId=subscription_id,
        resourceGroupName=resource_group_name,
        storageAccountName=storage_account_name,
        containerName=container_name
    )

def get_blob_container_names(definition_json):
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
    if not blob_containers:
        raise ValueError("No blob containers found in workspace definition for storage role synchronization")

    return [
        get_blob_container_scope(subscription_id, resource_group_name, storage_account_name, container_name)
        for container_name in blob_containers
    ]

def check_blob_reader_role(client:AuthorizationManagementClient, scope, user_object_id):
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
    (rg_name,sg_account) = get_storage_reference(environment_name, definition_json)
    scopes = get_blob_container_scopes(subscription_id, rg_name, sg_account, blob_containers)
    # iterate through definition_json['Workspace']['Acronym']
    for user in (user for user in definition_json['Workspace']['Users'] if user['Role'] != 'Removed'):
        user_id = user['ObjectId']
        print(f"processing user {user_id} access policies")
        try:
            for scope in scopes:
                reader_role = check_blob_reader_role(client, scope, user_id)
                if user['Role'] == 'Guest':
                    if reader_role is None:
                        print(f"assigning user {user_id} to access policies - read-only")
                        assign_blob_reader_role(client, subscription_id, scope, user_id, True)
                    elif not reader_role:
                        print(f"assigning user {user_id} to access policies - read-only")
                        remove_existing_role(client, scope, user_id)
                        assign_blob_reader_role(client, subscription_id, scope, user_id, True)
                    else:
                        print(f"user {user_id} already has read-only access policies")
                else:
                    if reader_role is None:
                        print(f"assigning user {user_id} to access policies - read-write")
                        assign_blob_reader_role(client, subscription_id, scope, user_id, False)
                    elif reader_role:
                        print(f"assigning user {user_id} to access policies - read-write")
                        remove_existing_role(client, scope, user_id)
                        assign_blob_reader_role(client, subscription_id, scope, user_id, False)
                    else:
                        print(f"user {user_id} already has read-write access policies")
        except Exception as e:
            print(f"error processing user {user_id} access policies: {e}")
    for user in (user for user in definition_json['Workspace']['Users'] if user['Role'] == 'Removed'):
        try:
            print(f"removing user {user['ObjectId']} from access policies")
            for scope in scopes:
                remove_existing_role(client, scope, user['ObjectId'])
        except Exception as e:
            print(f"error processing user {user['ObjectId']} access policies: {e}")
