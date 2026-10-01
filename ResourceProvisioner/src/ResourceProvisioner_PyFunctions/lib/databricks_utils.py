from databricks.sdk import WorkspaceClient
from databricks.sdk.service.iam import ComplexValue
from databricks.sdk.service.workspace import AzureKeyVaultSecretScopeMetadata
from databricks.sdk.service.workspace import ScopeBackendType
import lib.azkeyvault_utils as azkv_utils
import lib.constants as constants
import os
import logging

WORKSPACE_KV_SCOPE_NAME = "dh-workspace"
DEFAULT_SCHEMA_FSDH = "bronze"
logger = logging.getLogger(__name__)

# Role-level privilege presets requested by product semantics.
UNITY_CATALOG_ROLE_PRESET = {
    "Owner": "ALL_PRIVILEGES",
    "Guest": "DATA_READER",
    "Admin": "DATA_EDITOR",
    "User": "DATA_EDITOR",
}

# API-level privilege values expected to be supported directly by the Grants API.
UNITY_CATALOG_API_PRESET_PRIVILEGES = {
    "ALL_PRIVILEGES": {
        "catalog": ["ALL_PRIVILEGES"],
        "schema": ["ALL_PRIVILEGES"],
    },
    "DATA_EDITOR": {
        "catalog": ["DATA_EDITOR"],
        "schema": ["DATA_EDITOR"],
    },
    "DATA_READER": {
        "catalog": ["DATA_READER"],
        "schema": ["DATA_READER"],
    },
}

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

    Optional environment variable overrides:
    - DATABRICKS_UC_OWNER_PRINCIPAL
    - DATABRICKS_UC_ADMIN_PRINCIPAL
    - DATABRICKS_UC_USER_PRINCIPAL
    - DATABRICKS_UC_GUEST_PRINCIPAL

    Returns:
        dict: A dictionary that maps definition roles to UC principals.
    """
    role_lookup = get_definition_role_lookup().copy()

    owner_principal_override = os.environ.get("DATABRICKS_UC_OWNER_PRINCIPAL", "").strip()
    admin_principal_override = os.environ.get("DATABRICKS_UC_ADMIN_PRINCIPAL", "").strip()
    user_principal_override = os.environ.get("DATABRICKS_UC_USER_PRINCIPAL", "").strip()
    guest_principal_override = os.environ.get("DATABRICKS_UC_GUEST_PRINCIPAL", "").strip()

    if owner_principal_override:
        role_lookup["Owner"] = owner_principal_override
    if user_principal_override:
        role_lookup["User"] = user_principal_override
    if guest_principal_override:
        role_lookup["Guest"] = guest_principal_override

    if admin_principal_override:
        role_lookup["Admin"] = admin_principal_override

    return role_lookup


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

    logger.info("using vault: [%s].[%s]", rg_name, vault_name)
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
                logging.info(f"User {user['Email']} already exists in workspace; reconciling group membership")
                set_user_group_in_workspace(workspace_client, existing_user, user)
                continue

            logging.info(f"User {user['Email']} does not exist in workspace")
            create_new_user_in_workspace(workspace_client, user)
        except Exception:
            logging.exception(f"Error synchronizing user {user['Email']} in workspace.")
            if (retries < 1):
                logging.info(f"Retrying to synchronize user {user['Email']} in workspace.")
                refreshed_users = list(workspace_client.users.list())
                existing_user = _find_existing_workspace_user_by_email(refreshed_users, user.get('Email'))
                if existing_user is not None:
                    logging.info(f"Found existing user {user['Email']} in workspace during retry; reconciling group membership")
                    set_user_group_in_workspace(workspace_client, existing_user, user)
                    return

                for workspace_user in refreshed_users:
                    logging.info(f"Checking {workspace_user} in workspace")
                    if user['Email'].lower() == getattr(workspace_user, 'user_name', '').lower():
                        logging.info(f"Deleting user {user['Email']} in workspace")
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
        logging.warning("Workspace client or its metastores attribute is not available.")
        return None

    metastores_api = workspace_client.metastores
    if not hasattr(metastores_api, "current"):
        logging.warning("Workspace client metastores API does not expose current().")
        return None

    try:
        current_assignment = metastores_api.current()
        logging.info(
            "_get_current_metastore_catalog_name: loaded current metastore assignment for metastore_id=%s",
            getattr(current_assignment, "metastore_id", None),
        )
    except Exception:
        logging.exception("Failed to retrieve current metastore from workspace")
        return None

    catalog_name = _normalize_catalog_name(getattr(current_assignment, "default_catalog_name", None))
    if catalog_name:
        logging.info(
            "Resolved Unity Catalog name '%s' from current metastore assignment field 'default_catalog_name'.",
            catalog_name,
        )
        return catalog_name

    logging.error(
        "Unable to resolve Unity Catalog name from current metastore assignment. "
        "Assignment fields: metastore_id=%s default_catalog_name=%s",
        getattr(current_assignment, "metastore_id", None),
        getattr(current_assignment, "default_catalog_name", None),
    )

    return None


def get_unity_catalog_targets(definition_json, workspace_client=None):
    """
    Returns the catalog and schema names to use for Unity Catalog grants.

    Args:
        definition_json (dict): The workspace definition file as a dictionary (json).

    Returns:
        tuple[str, str]: The catalog and schema names.
    """
    app_data = definition_json.get("AppData", {}) or {}
    metastore_catalog_name = _get_current_metastore_catalog_name(workspace_client)
    catalog_name = None
    catalog_source = None

    explicit_catalog_name = app_data.get("DatabricksCatalogName")
    if explicit_catalog_name is not None and str(explicit_catalog_name).strip():
        catalog_name = str(explicit_catalog_name).strip()
        catalog_source = "AppData.DatabricksCatalogName"
        logging.debug("get_unity_catalog_targets: using catalog from AppData.DatabricksCatalogName")
    elif app_data.get("UnityCatalogName") is not None and str(app_data.get("UnityCatalogName")).strip():
        catalog_name = str(app_data.get("UnityCatalogName")).strip()
        catalog_source = "AppData.UnityCatalogName"
        logging.debug("get_unity_catalog_targets: using catalog from AppData.UnityCatalogName")
    elif metastore_catalog_name is not None and str(metastore_catalog_name).strip():
        catalog_name = str(metastore_catalog_name).strip()
        catalog_source = "workspace_client.metastores.current()"
        logging.debug("get_unity_catalog_targets: using catalog from workspace_client.metastores.current()")

    if not catalog_name:
        workspace_name = definition_json.get("Workspace", {}).get("Acronym", "unknown")
        logging.error(
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

    logging.debug("Using Unity Catalog name '%s' from %s", catalog_name, catalog_source)

    schema_name = (
        app_data.get("DatabricksSchemaName")
        or app_data.get("UnitySchemaName")
        or DEFAULT_SCHEMA_FSDH
    )
    return catalog_name, schema_name


class _CatalogGrantPayload:
    def __init__(self, principal, privileges):
        self.principal = principal
        self.privileges = privileges

    def as_dict(self):
        return {"principal": self.principal, "privileges": self.privileges}


def _resolve_unity_catalog_privileges_for_role(role_name):
    """Return preset label and API-native privileges to apply for a definition role."""
    preset = UNITY_CATALOG_ROLE_PRESET.get(role_name, "DATA_EDITOR")
    api_native_privileges = UNITY_CATALOG_API_PRESET_PRIVILEGES[preset]
    return preset, api_native_privileges


def _get_unity_catalog_api_supported_privileges():
    """Return SDK-exposed Unity Catalog privilege names, or None if unavailable."""
    try:
        from databricks.sdk.service.catalog import Privilege as CatalogPrivilege
    except Exception:
        return None

    supported = set()

    enum_members = getattr(CatalogPrivilege, "__members__", None)
    if isinstance(enum_members, dict):
        supported.update(enum_members.keys())

    for attr_name in dir(CatalogPrivilege):
        if attr_name.startswith("_"):
            continue
        attr_value = getattr(CatalogPrivilege, attr_name, None)
        if isinstance(attr_value, str):
            supported.add(attr_name)
            supported.add(attr_value)

    return supported


def _validate_unity_catalog_role_presets_against_api():
    """Validate configured role presets are represented by the current SDK/API surface."""
    required_presets = {preset for preset in UNITY_CATALOG_ROLE_PRESET.values()}
    supported_privileges = _get_unity_catalog_api_supported_privileges()
    if supported_privileges is None:
        raise ValueError(
            "Unable to validate Unity Catalog role presets against SDK API because catalog.Privilege is not available"
        )

    missing_presets = sorted(preset for preset in required_presets if preset not in supported_privileges)
    if missing_presets:
        raise ValueError(
            "Configured Unity Catalog role presets are not available in the current Databricks API/SDK: "
            f"{', '.join(missing_presets)}"
        )


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
        logging.info("Workspace client does not expose a grants API; skipping Unity Catalog permissions")
        return

    logging.info("Applying Unity Catalog grant for principal '%s' on '%s'", principal, full_name)
    try:
        try:
            from databricks.sdk.service.catalog import Grant as CatalogGrant
        except Exception:
            CatalogGrant = None

        if CatalogGrant is not None:
            try:
                grant = CatalogGrant(principal=principal, privileges=privileges)
                if not hasattr(grant, "as_dict"):
                    raise AttributeError("Grant object does not expose as_dict")
            except Exception:
                grant = _CatalogGrantPayload(principal=principal, privileges=privileges)
        else:
            grant = _CatalogGrantPayload(principal=principal, privileges=privileges)

        workspace_client.grants.update(
            securable_type=securable_type,
            full_name=full_name,
            changes=[grant]
        )

        logging.info(f"Applied Unity Catalog privileges {privileges} to {principal} on {full_name}")
    except Exception:
        logging.exception(f"Failed to apply Unity Catalog grant for {principal} on {full_name}")


def synchronize_unity_catalog_permissions(definition_json, workspace_client):
    """
    Synchronizes Unity Catalog permissions for the workspace groups derived from the definition roles.

    Args:
        definition_json (dict): The workspace definition file as a dictionary (json).
        workspace_client (WorkspaceClient): The databricks workspace client.

    Returns:
        None
    """
    catalog_name, schema_name = get_unity_catalog_targets(definition_json, workspace_client)
    _validate_unity_catalog_role_presets_against_api()
    schema_full_name = f"{catalog_name}.{schema_name}"

    workspace_users = list(workspace_client.users.list()) if hasattr(workspace_client, "users") and hasattr(workspace_client.users, "list") else []
    for user in (user for user in definition_json.get("Workspace", {}).get("Users", []) if user.get("Role") != "Removed"):
        principal = _resolve_unity_catalog_principal_for_user(user, workspace_users)
        if not principal:
            logging.info(f"Skipping Unity Catalog permission sync for user {user.get('Email')} because no principal email could be resolved")
            continue

        role_name = user.get("Role", "User")
        preset, role_privileges = _resolve_unity_catalog_privileges_for_role(role_name)
        preset_display = preset.replace("_", " ")
        logging.info(
            "Synchronizing Unity Catalog permissions for %s (%s) using API preset '%s' on catalog '%s' (schema '%s' relies on inherited permissions)",
            principal,
            role_name,
            preset_display,
            catalog_name,
            schema_full_name,
        )

        apply_unity_catalog_grant(
            workspace_client,
            "catalog",
            catalog_name,
            principal,
            role_privileges["catalog"],
        )


def _get_workspace_group_for_role(workspace_groups, role_name):
    """Return the Databricks group for the given definition role when it exists."""
    definition_role_lookup = get_definition_role_lookup()
    group_name = definition_role_lookup.get(role_name)
    if not group_name:
        logging.warning("No Databricks group mapping for role '%s'", role_name)
        return None

    if group_name not in workspace_groups:
        logging.warning(
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
    logging.info(f"\tCreating user {user['Email']} in workspace")

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
        if role_display_name not in workspace_groups:
            logging.warning(
                "Skipping group update for user %s because Databricks group '%s' does not exist in this workspace",
                definition_user['Email'],
                role_display_name,
            )
            return
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
    if role_display_name not in workspace_groups:
        logging.warning(
            "Skipping group assignment for user %s because Databricks group '%s' does not exist in this workspace",
            definition_user['Email'],
            role_display_name,
        )
        return

    group_to_add = ComplexValue(value=workspace_groups[role_display_name].id, display=role_display_name, primary=None, type=None)

    logging.info(f"\tAdding new group {group_to_add} to user {definition_user['Email']}")

    workspace_client.users.update(id=workspace_user.id, user_name=definition_user['Email'], groups=[group_to_add])
    logging.info(f"\tUser {definition_user['Email']} is now in the {role_display_name} group in workspace")