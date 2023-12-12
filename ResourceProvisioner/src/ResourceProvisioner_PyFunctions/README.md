# Resource Provisioner Python Functions


## Overview

This project contains the Python functions for the Resource Provisioner. The functions are deployed to Azure Functions and are triggered by events from the Storage Queue.

In order for the functions to run, the following configuration variables must be set:

```json
{
    ..
    "AzureClientId": "<client_id>",
    "AzureClientSecret": "<client_secret>",
    "AzureTenantId": "<tenant_id>",
    ..
}
```

## SynchronizeWorkspaceUsersHttpTrigger

This function is triggered by a message in the Storage Queue. The message contains the workspace definition file with the list of users and their roles. The function will synchronize the users and their roles with the workspace.


```mermaid
flowchart
    Start --> Clean["`
    **remove_deleted_users_in_workspace**
    *Clean the users in the workspace. Any users without an external ID in the databricks workspace will be removed as they no longer exist in MS Graph.*
    `"]

    Clean --> Sync["`
    **synchronize_workspace_users**
    *Synchronizes the workspace users with the users defined in the definition file.*
    `"]
    Sync -->|for each user in definition| Exists["`
    Check user is in workspace.
    *if user['ObjectId'] == workspace_user.external_id:*
    `"]
    
    Exists-->|user exists| SetGroup["`
    **set_user_group_in_workspace**
    *Set the user's group in the workspace based on the role.*
    `"]
    Exists-->|user doesn't exist| Add["`
    **create_new_user_in_workspace**
    *Create a new user in the workspace with the correct group assignment.*
    `"]

    SetGroup --> |user has no groups| AddGroup["`
    **add_user_to_group_in_workspace**
    *Add the user to the group in the workspace based on the role.*
    `"]
    SetGroup --> |user has groups| UpdateGroups["`
    **update_user_group_in_workspace**
    *Set the user's group in the workspace based on the role.*
    `"]

    AddGroup --> Empty["`*Synchronize the Databricks workspace for removed users.*`"]
    UpdateGroups --> Empty
    Add --> Empty

    Empty --> |for each user in workspace| CheckDef["`
    Check user is in definition file.
    *if workspace_user.external_id not in definition_users_object_ids:*
    `"]

    CheckDef -->|user not in definition| Remove["`
    **remove_user_from_workspace**
    *Remove the user from the workspace as they are not in the definition file.*
    `"]

    CheckDef -->|user in definition| End
    Remove --> End
```