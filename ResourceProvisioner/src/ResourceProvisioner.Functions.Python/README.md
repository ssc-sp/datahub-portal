# Resource Provisioner Python Functions


## Overview

This project contains the Python functions for the Resource Provisioner. The functions are deployed to Azure Functions and are triggered by events from the Storage Queue.

## SynchronizeWorkspaceUsers

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
    Sync -->|for each user| Exists["`
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

    AddGroup --> End
    UpdateGroups --> End

    Add --> End
```