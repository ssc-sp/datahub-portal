from databricks.sdk import WorkspaceClient
from databricks.sdk.service.iam import ComplexValue
from databricks.sdk.service.workspace import AzureKeyVaultSecretScopeMetadata
from databricks.sdk.service.workspace import ScopeBackendType
import lib.azkeyvault_utils as azkv_utils
import lib.constants as constants
from lib.unity_catalog_constants import UNITY_CATALOG_PRESET_PRIVILEGES, UNITY_CATALOG_ROLE_PRESET
import os
import logging

WORKSPACE_KV_SCOPE_NAME = "dh-workspace"
logger = logging.getLogger(__name__)

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


def get_unity_catalog_role_lookup():
    """
    Returns a role-to-principal mapping for Unity Catalog grants.

    Uses the definition role-to-group mapping by default.

    Returns:
        dict: A dictionary that maps definition roles to UC principals.
    """
    return get_definition_role_lookup().copy()


def _find_workspace_user_for_definition_user(workspace_users, definition_user):
    """Find the workspace user by external_id first, then email."""
    object_id = (definition_user or {}).get("ObjectId")
    if object_id:
        for workspace_user in workspace_users:
            external_id = getattr(workspace_user, "external_id", None)
            if external_id and object_id.lower() == external_id.lower():
                return workspace_user

    return _find_existing_workspace_user_by_email(workspace_users, (definition_user or {}).get("Email"))


def _resolve_unity_catalog_principal_for_user(definition_user, workspace_users):
    """Resolve UC principal as the user's workspace identity (email)."""
    workspace_user = _find_workspace_user_for_definition_user(workspace_users, definition_user)
    if workspace_user is not None:
        workspace_user_name = getattr(workspace_user, "user_name", None)
        if workspace_user_name:
            return workspace_user_name

        emails = getattr(workspace_user, "emails", None) or []
        for email_entry in emails:
            email_value = getattr(email_entry, "value", None)
            if email_value:
                return email_value

    email = (definition_user or {}).get("Email")
    if email:
        return email

    return None

def get_workspace_client(databricksHost):
    """
    Returns a WorkspaceClient object for the given Databricks host and token.

    Args:
        databricksHost (str): The Databricks host url.

    Returns:
        WorkspaceClient: The Databricks workspace client.
    """

    normalized_host = (databricksHost or "").strip().lower()
    if not normalized_host or not any(
        marker in normalized_host
        for marker in (".azuredatabricks.net", ".databricks.azure.us", ".databricks.azure.cn")
    ):
        raise ValueError(
            "Databricks host must be an Azure Databricks workspace host for azure-client-secret auth. "
            f"Received: {databricksHost}"
        )

    return WorkspaceClient(
        host=databricksHost,
        azure_client_secret=os.environ["AzureClientSecret"],
        azure_client_id=os.environ["AzureClientId"],
        azure_tenant_id=os.environ["AzureTenantId"],
        auth_type='azure-client-secret',
    )

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
    logger.info(f'Users to remove: {removedIds}')
    toRemove = []
    for user in workspace_client.users.list():
        if user.external_id is None:
            logger.info(f'User {user.user_name} does not have an external ID, removing from workspace')
            workspace_client.users.delete(user.id)
        else:
            if user.external_id in removedIds:
                toRemove.append(user)
            logger.info(f'User {user.user_name} with external ID {user.external_id} exists')
    for user in toRemove:
        logger.info(f'User {user.user_name} with external ID {user.external_id} is marked for removal')
        workspace_client.users.delete(user.id)

def synchronize_workspace_secrets(environment_name, subscription_id, definition_json, workspace_client):
    azure_tenant_id = os.environ["AzureTenantId"]    
    kv_client = azkv_utils.get_keyvault_client(subscription_id, azure_tenant_id)
    secret_list = azkv_utils.list_secrets(kv_client, environment_name, definition_json)
    for secret in secret_list:
        logger.info(f"adding secret: {secret.name} to workspace")
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

    logger.info("using vault: [%s].[%s]", rg_name, vault_name)
    kv_uri = azkv_utils.get_keyvault_uri(vault_name.lower())
    resource_id = f"/subscriptions/{subscription_id}/resourcegroups/{rg_name.lower()}/providers/Microsoft.KeyVault/vaults/{vault_name.lower()}"
    workspace_secret_scopes = workspace_client.secrets.list_scopes()
    # for workspace_secret_scope in workspace_secret_scopes:
    #     logger.info(f"Deleting secret scope {workspace_secret_scope.name}")
    #     workspace_client.secrets.delete_scope(scope=workspace_secret_scope.name)
    # Check if WORKSPACE_KV_SCOPE_NAME exists   
    workspace_kv_scope_found = False
    for workspace_secret_scope in workspace_secret_scopes:
        if workspace_secret_scope.name == WORKSPACE_KV_SCOPE_NAME:
            workspace_kv_scope_found = True
            break
    if not workspace_kv_scope_found:
        logger.info("Secret scope %s does not exist in workspace", WORKSPACE_KV_SCOPE_NAME)
        azKeyVault = AzureKeyVaultSecretScopeMetadata(resource_id=resource_id,dns_name=kv_uri)
        workspace_client.secrets.create_scope(scope=WORKSPACE_KV_SCOPE_NAME, initial_manage_principal='users', 
                                              scope_backend_type = ScopeBackendType.AZURE_KEYVAULT,
                                              backend_azure_keyvault=azKeyVault)
        logger.info("Secret scope %s created in workspace", WORKSPACE_KV_SCOPE_NAME)
    else:
        logger.info("Secret scope %s already exists in workspace", WORKSPACE_KV_SCOPE_NAME)

def _find_existing_workspace_user_by_email(workspace_users, email):
    """Return an existing Databricks user by case-insensitive email match."""
    normalized_email = (email or "").strip().lower()
    if not normalized_email:
        return None

    for workspace_user in workspace_users:
        user_name = getattr(workspace_user, "user_name", None)
        if user_name and user_name.lower() == normalized_email:
            return workspace_user

        emails = getattr(workspace_user, "emails", None) or []
        for email_entry in emails:
            email_value = getattr(email_entry, "value", None)
            if email_value and email_value.lower() == normalized_email:
                return workspace_user

    return None


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
    workspace_users = list(workspace_client.users.list())

    # Iterate over each user in the definition file
    # exclude users that have been removed
    for user in (user for user in definition_json['Workspace']['Users'] if (user['Role'] != 'Removed')):
        try:
            # Find the user matching user in the workspace by external ID or case-insensitive email.
            user_found = False
            existing_user = None
            for workspace_user in workspace_users:
                external_id = getattr(workspace_user, "external_id", None)
                if external_id and user.get('ObjectId') and user['ObjectId'].lower() == external_id.lower():
                    existing_user = workspace_user
                    user_found = True
                    break

            if not user_found:
                existing_user = _find_existing_workspace_user_by_email(workspace_users, user.get('Email'))
                user_found = existing_user is not None

            if user_found and existing_user is not None:
                logger.info(f"User {user['Email']} already exists in workspace; reconciling group membership")
                set_user_group_in_workspace(workspace_client, existing_user, user)
                continue

            logger.info(f"User {user['Email']} does not exist in workspace")
            create_new_user_in_workspace(workspace_client, user)
        except Exception:
            logger.exception(f"Error synchronizing user {user['Email']} in workspace.")
            if (retries < 1):
                logger.info(f"Retrying to synchronize user {user['Email']} in workspace.")
                refreshed_users = list(workspace_client.users.list())
                existing_user = _find_existing_workspace_user_by_email(refreshed_users, user.get('Email'))
                if existing_user is not None:
                    logger.info(f"Found existing user {user['Email']} in workspace during retry; reconciling group membership")
                    set_user_group_in_workspace(workspace_client, existing_user, user)
                    return

                for workspace_user in refreshed_users:
                    logger.info(f"Checking {workspace_user} in workspace")
                    if user['Email'].lower() == getattr(workspace_user, 'user_name', '').lower():
                        logger.info(f"Deleting user {user['Email']} in workspace")
                        workspace_client.users.delete(workspace_user.id)
                        break
                synchronize_workspace_users(definition_json, workspace_client, retries + 1)

def _get_current_metastore_catalog_name(workspace_client):
    """Return the catalog name derived from the workspace's current metastore."""
    def _normalize_catalog_name(value):
        if value is None:
            return None
        normalized = str(value).strip()
        if not normalized or normalized.lower() in {"none", "null"}:
            return None
        return normalized

    if not workspace_client or not hasattr(workspace_client, "metastores"):
        logger.warning("Workspace client or its metastores attribute is not available.")
        return None

    metastores_api = workspace_client.metastores
    if not hasattr(metastores_api, "current"):
        logger.warning("Workspace client metastores API does not expose current().")
        return None

    try:
        current_assignment = metastores_api.current()
        logger.info(
            "_get_current_metastore_catalog_name: loaded current metastore assignment for metastore_id=%s",
            getattr(current_assignment, "metastore_id", None),
        )
    except Exception:
        logger.exception("Failed to retrieve current metastore from workspace")
        return None

    catalog_name = _normalize_catalog_name(getattr(current_assignment, "default_catalog_name", None))
    if catalog_name:
        logger.info(
            "Resolved Unity Catalog name '%s' from current metastore assignment field 'default_catalog_name'.",
            catalog_name,
        )
        return catalog_name

    logger.error(
        "Unable to resolve Unity Catalog name from current metastore assignment. "
        "Assignment fields: metastore_id=%s default_catalog_name=%s",
        getattr(current_assignment, "metastore_id", None),
        getattr(current_assignment, "default_catalog_name", None),
    )

    return None


def get_unity_catalog_targets(definition_json, workspace_client=None):
    """
    Returns the catalog name to use for Unity Catalog grants.

    Args:
        definition_json (dict): The workspace definition file as a dictionary (json).

    Returns:
        str: The catalog name.
    """
    app_data = definition_json.get("AppData", {}) or {}
    metastore_catalog_name = _get_current_metastore_catalog_name(workspace_client)
    catalog_name = None
    catalog_source = None

    explicit_catalog_name = app_data.get("DatabricksCatalogName")
    if explicit_catalog_name is not None and str(explicit_catalog_name).strip():
        catalog_name = str(explicit_catalog_name).strip()
        catalog_source = "AppData.DatabricksCatalogName"
        logger.debug("get_unity_catalog_targets: using catalog from AppData.DatabricksCatalogName")
    elif app_data.get("UnityCatalogName") is not None and str(app_data.get("UnityCatalogName")).strip():
        catalog_name = str(app_data.get("UnityCatalogName")).strip()
        catalog_source = "AppData.UnityCatalogName"
        logger.debug("get_unity_catalog_targets: using catalog from AppData.UnityCatalogName")
    elif metastore_catalog_name is not None and str(metastore_catalog_name).strip():
        catalog_name = str(metastore_catalog_name).strip()
        catalog_source = "workspace_client.metastores.current()"
        logger.debug("get_unity_catalog_targets: using catalog from workspace_client.metastores.current()")

    if not catalog_name:
        workspace_name = definition_json.get("Workspace", {}).get("Acronym", "unknown")
        logger.error(
            "get_unity_catalog_targets: unable to resolve catalog for workspace '%s'. "
            "DatabricksCatalogName=%s UnityCatalogName=%s AppDataKeys=%s",
            workspace_name,
            app_data.get("DatabricksCatalogName"),
            app_data.get("UnityCatalogName"),
            sorted(app_data.keys()),
        )
        raise ValueError(
            "Unable to resolve Unity Catalog name. Set AppData.DatabricksCatalogName/AppData.UnityCatalogName "
            "or ensure workspace_client.metastores.current() returns a catalog/default_catalog_name."
        )

    logger.debug("Using Unity Catalog name '%s' from %s", catalog_name, catalog_source)
    return catalog_name


class _CatalogGrantPayload:
    def __init__(self, principal, privileges):
        self.principal = principal
        self.privileges = privileges

    def as_dict(self):
        return {"principal": self.principal, "privileges": self.privileges}


class _CatalogPermissionsChangePayload:
    def __init__(self, principal, add=None, remove=None):
        self.principal = principal
        self.add = add
        self.remove = remove

    def as_dict(self):
        body = {"principal": self.principal}
        if self.add:
            body["add"] = [getattr(privilege, "value", privilege) for privilege in self.add]
        if self.remove:
            body["remove"] = [getattr(privilege, "value", privilege) for privilege in self.remove]
        return body


def _resolve_unity_catalog_privileges_for_role(role_name):
    """Return role preset label and documented API privileges to apply."""
    preset = UNITY_CATALOG_ROLE_PRESET.get(role_name)
    if preset is None:
        return None, None

    privileges = UNITY_CATALOG_PRESET_PRIVILEGES.get(preset)
    if privileges is None:
        raise ValueError(f"Unity Catalog preset '{preset}' is not mapped to API privileges")

    return preset, privileges


def _catalog_privilege_value(privilege_name):
    if privilege_name is None:
        return None

    return getattr(privilege_name, "value", privilege_name)


def _normalize_catalog_permissions(privileges):
    if privileges is None:
        return None
    return [_catalog_privilege_value(privilege) for privilege in privileges]


def _build_catalog_permissions_change(principal, add=None, remove=None):
    normalized_add = _normalize_catalog_permissions(add)
    normalized_remove = _normalize_catalog_permissions(remove)

    return _CatalogPermissionsChangePayload(
        principal=principal,
        add=normalized_add,
        remove=normalized_remove,
    )


def _revoke_unity_catalog_privileges(workspace_client, securable_type, full_name, principal):
    if not hasattr(workspace_client, "grants") or not hasattr(workspace_client.grants, "list"):
        logger.info("Workspace client does not expose a grants list API; skipping Unity Catalog privilege revocation")
        return False

    try:
        current_assignments = list(workspace_client.grants.list(securable_type, full_name, principal=principal))
    except Exception:
        logger.exception(
            "Failed to list current Unity Catalog privileges for principal '%s' on '%s'",
            principal,
            full_name,
        )
        return False

    changes = []
    for assignment in current_assignments:
        privileges = getattr(assignment, "privileges", None) or []
        if not privileges:
            continue

        assignment_principal = getattr(assignment, "principal", None) or principal
        remove_privileges = [
            _catalog_privilege_value(getattr(privilege, "value", privilege))
            for privilege in privileges
        ]
        changes.append(
            _build_catalog_permissions_change(
                principal=assignment_principal,
                remove=remove_privileges,
            )
        )

    if not changes:
        return False

    logger.info(
        "Removing existing Unity Catalog privileges for principal '%s' on '%s' before reapplying role presets",
        principal,
        full_name,
    )
    workspace_client.grants.update(
        securable_type=securable_type,
        full_name=full_name,
        changes=changes,
    )
    return True


def apply_unity_catalog_grant(workspace_client, securable_type, full_name, principal, privileges):
    """
    Applies a Unity Catalog grant using the workspace client's grants API when available.

    Args:
        workspace_client (WorkspaceClient): The databricks workspace client.
        securable_type (str): The Unity Catalog securable type.
        full_name (str): The fully qualified name of the securable.
        principal (str): The workspace group or user principal to grant access to.
        privileges (list[str]): The privileges to apply.

    Returns:
        None        
    """    
    if not hasattr(workspace_client, "grants") or not hasattr(workspace_client.grants, "update"):
        logger.info("Workspace client does not expose a grants API; skipping Unity Catalog permissions")
        return

    logger.info("Applying Unity Catalog grant for principal '%s' on '%s'", principal, full_name)
    try:
        grant = _build_catalog_permissions_change(
            principal=principal,
            add=_normalize_catalog_permissions(privileges),
        )

        workspace_client.grants.update(
            securable_type=securable_type,
            full_name=full_name,
            changes=[grant]
        )

        logger.info(f"Applied Unity Catalog privileges {privileges} to {principal} on {full_name}")
    except Exception:
        logger.exception(f"Failed to apply Unity Catalog grant for {principal} on {full_name}")
        raise


def synchronize_unity_catalog_permissions(definition_json, workspace_client):
    """
    Synchronizes Unity Catalog permissions for the workspace groups derived from the definition roles.

    Args:
        definition_json (dict): The workspace definition file as a dictionary (json).
        workspace_client (WorkspaceClient): The databricks workspace client.

    Returns:
        None
    """
    catalog_name = get_unity_catalog_targets(definition_json, workspace_client)
    workspace_users = list(workspace_client.users.list()) if hasattr(workspace_client, "users") and hasattr(workspace_client.users, "list") else []
    for user in (user for user in definition_json.get("Workspace", {}).get("Users", []) if user.get("Role") != "Removed"):
        principal = _resolve_unity_catalog_principal_for_user(user, workspace_users)
        if not principal:
            logger.info(f"Skipping Unity Catalog permission sync for user {user.get('Email')} because no principal email could be resolved")
            continue

        role_name = user.get("Role", "User")
        preset, role_privileges = _resolve_unity_catalog_privileges_for_role(role_name)
        _revoke_unity_catalog_privileges(
            workspace_client,
            "catalog",
            catalog_name,
            principal,
        )

        if preset is None or role_privileges is None:
            logger.info(
                "Skipping Unity Catalog privilege assignment for %s because role '%s' is not mapped to a preset",
                principal,
                role_name,
            )
            continue

        preset_display = preset.replace("_", " ")
        logger.info(
            "Synchronizing Unity Catalog permissions for %s (%s) using API preset '%s' on catalog '%s'",
            principal,
            role_name,
            preset_display,
            catalog_name,
        )

        apply_unity_catalog_grant(
            workspace_client,
            "catalog",
            catalog_name,
            principal,
            role_privileges,
        )


def _get_workspace_group_for_role(workspace_groups, role_name):
    """Return the Databricks group for the given definition role when it exists."""
    definition_role_lookup = get_definition_role_lookup()
    group_name = definition_role_lookup.get(role_name)
    if not group_name:
        logger.warning("No Databricks group mapping for role '%s'", role_name)
        return None

    if group_name not in workspace_groups:
        logger.warning(
            "Role '%s' maps to Databricks group '%s', but that group does not exist in this workspace; skipping group assignment",
            role_name,
            group_name,
        )
        return None

    return workspace_groups[group_name]


def create_new_user_in_workspace(workspace_client, user):
    """
    Creates a new user in the workspace and adds them to the correct group based on their role.

    Args:
        workspace_client (WorkspaceClient): The databricks workspace client.
        user (dict): The definition user to create in the workspace.

    Returns:
        workspace_user (User): The workspace user that was created.

    """
    logger.info(f"\tCreating user {user['Email']} in workspace")

    workspace_groups = get_workspace_groups(workspace_client)
    workspace_email = ComplexValue(value=user['Email'], 
                                display=None, 
                                primary=None, 
                                type='work')
    create_kwargs = {
        "display_name": user['Email'],
        "user_name": user['Email'],
        "emails": [workspace_email],
        "external_id": user['ObjectId'],
    }

    group = _get_workspace_group_for_role(workspace_groups, user['Role'])
    if group is not None:
        workspace_group = ComplexValue(value=group.id, display=group.display_name, primary=None, type=None)
        create_kwargs["groups"] = [workspace_group]

    workspace_user = workspace_client.users.create(**create_kwargs)
    logger.info(f"\tUser {user['Email']} created in workspace")

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
            logger.info(f"\tUser {definition_user['Email']} has correct group {group} in workspace")
            break
    if not group_found:
        logger.info(f"\tUser {definition_user['Email']} does not have correct groups {definition_user['Role']} in workspace, updating...")

        role_display_name = definition_role_lookup[definition_user['Role']]
        if role_display_name not in workspace_groups:
            logger.warning(
                "Skipping group update for user %s because Databricks group '%s' does not exist in this workspace",
                definition_user['Email'],
                role_display_name,
            )
            return
        group_to_add = ComplexValue(value=workspace_groups[role_display_name].id, display=role_display_name, primary=None, type=None)

        logger.info(f"\tAdding missing group {group_to_add} to user {definition_user['Email']}")

        workspace_client.users.update(id=workspace_user.id, user_name=definition_user['Email'], groups=[group_to_add])
        logger.info(f"\tUser {definition_user['Email']} now has role {definition_user['Role']} in workspace (definition role: {definition_role_lookup[definition_user['Role']]}))")

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

    logger.info(f"\tUser {definition_user['Email']} has no groups in workspace, updating...")

    role_display_name = definition_role_lookup[definition_user['Role']]
    if role_display_name not in workspace_groups:
        logger.warning(
            "Skipping group assignment for user %s because Databricks group '%s' does not exist in this workspace",
            definition_user['Email'],
            role_display_name,
        )
        return

    group_to_add = ComplexValue(value=workspace_groups[role_display_name].id, display=role_display_name, primary=None, type=None)

    logger.info(f"\tAdding new group {group_to_add} to user {definition_user['Email']}")

    workspace_client.users.update(id=workspace_user.id, user_name=definition_user['Email'], groups=[group_to_add])
    logger.info(f"\tUser {definition_user['Email']} is now in the {role_display_name} group in workspace")