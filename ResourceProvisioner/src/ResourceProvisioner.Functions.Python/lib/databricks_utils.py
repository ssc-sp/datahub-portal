from databricks.sdk import WorkspaceClient
from databricks.sdk.service.iam import ComplexValue

import os


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

def remove_deleted_users_in_workspace(workspace_client):
    """
    Removes all users from the workspace that do not have an external ID.

    Args:
        workspace_client (WorkspaceClient): The databricks workspace client.

    Returns:
        None
    """
    for user in workspace_client.users.list():
        if user.external_id is None:
            print(f'User {user.user_name} does not have an external ID, removing from workspace')
            workspace_client.users.delete(user.id)
        else:
            print(f'User {user.user_name} with external ID {user.external_id} exists')

def synchronize_workspace_users(definition_json, workspace_client):
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
    for user in definition_json['Workspace']['Users']:

        # Find the user matching user in the workspace
        user_found = False
        for workspace_user in workspace_users:
            if user['ObjectId'] == workspace_user.external_id:
                user_found = True

                # Check if the user is in the correct group
                print(f"Checking {user['Email']}'s has group {user['Role']} in workspace")
                set_user_group_in_workspace(workspace_client, workspace_user, user)
                
                # Since we found the user, break out of the loop
                break

        # If we didn't find the user, create them
        if not user_found:
            print(f"User {user['Email']} does not exist in workspace")
            create_new_user_in_workspace(workspace_client, user)

def create_new_user_in_workspace(workspace_client, user):
    """
    Creates a new user in the workspace and adds them to the correct group based on their role.

    Args:
        workspace_client (WorkspaceClient): The databricks workspace client.
        user (dict): The definition user to create in the workspace.

    Returns:
        workspace_user (User): The workspace user that was created.

    """
    print(f"\tCreating user {user['Email']} in workspace")

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
    print(f"\tUser {user['Email']} created in workspace")

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
            print(f"\tUser {definition_user['Email']} has correct group {group} in workspace")
            break
    if not group_found:
        print(f"\tUser {definition_user['Email']} does not have correct groups {definition_user['Role']} in workspace, updating...")

        role_display_name = definition_role_lookup[definition_user['Role']]
        group_to_add = ComplexValue(value=workspace_groups[role_display_name].id, display=role_display_name, primary=None, type=None)

        print(f"\tAdding missing group {group_to_add} to user {definition_user['Email']}")

        workspace_client.users.update(id=workspace_user.id, user_name=definition_user['Email'], groups=[group_to_add])
        print(f"\tUser {definition_user['Email']} now has role {definition_user['Role']} in workspace (definition role: {definition_role_lookup[definition_user['Role']]}))")

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

    print(f"\tUser {definition_user['Email']} has no groups in workspace, updating...")

    role_display_name = definition_role_lookup[definition_user['Role']]
    group_to_add = ComplexValue(value=workspace_groups[role_display_name].id, display=role_display_name, primary=None, type=None)

    print(f"\tAdding new group {group_to_add} to user {definition_user['Email']}")

    workspace_client.users.update(id=workspace_user.id, user_name=definition_user['Email'], groups=[group_to_add])
    print(f"\tUser {definition_user['Email']} is now in the {role_display_name} group in workspace")