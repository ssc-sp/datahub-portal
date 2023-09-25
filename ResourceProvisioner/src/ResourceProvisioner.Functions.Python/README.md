# Resource Provisioner Python Functions


## Overview

This project contains the Python functions for the Resource Provisioner. The functions are deployed to Azure Functions and are triggered by events from the Storage Queue.

## SynchronizeWorkspaceUsers

This function is triggered by a message in the Storage Queue. The message contains the workspace definition file with the list of users and their roles. The function will synchronize the users and their roles with the workspace.


```mermaid
flowchart
    Start --> Clean[Clean the users in the workspace. Any users without an external ID in the databricks workspace will be removed as they no longer exist in MS Graph.]
    Clean --> Verify[Verify that the users in the definition file exist within the workspace.]
    Verify -->|for each user| Exists[Check if the ObjectId matches the external ID.]
    Exists-->|user exists| ExistsT[Verify that the group assignment is correct.]
    Exists-->|user doesn't exist| Add[Add the user to the workspace.]

    ExistsT--> GroupCheck[Check the existing group assignments.]
    GroupCheck -->|group assignment is correct| CorrectGroup[Do nothing.]
    CorrectGroup --> End
    GroupCheck -->|no groups assigned| AddGroup[Add to group based on the role.]
    AddGroup --> End

    Add --> AddComplete[Create a new user in the workspace with the correct group assignment.]
    AddComplete --> End

```