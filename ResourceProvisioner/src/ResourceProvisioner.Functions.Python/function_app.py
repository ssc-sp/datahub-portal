import azure.functions as func
import os
from databricks.sdk import WorkspaceClient

from lib.databricks_utils import get_workspace_client, remove_deleted_users_in_workspace, synchronize_workspace_users

app = func.FunctionApp()

@app.function_name(name="SynchronizeWorkspaceUsersHttpTrigger")
@app.route(route="sync-workspace-users") # HTTP Trigger
def sync_workspace_users_function(req: func.HttpRequest) -> func.HttpResponse:
    """
    Synchronizes the users in the Databricks workspace with the users in the definition file.

    Args:
        req (HttpRequest): The HTTP request object.

    Returns:
        HttpResponse: The HTTP response object.

    """

    workspace_definition = req.get_json()
    databricksHost = workspace_definition['Apps']['azure-databricks']['HostUrl']

    workspace_client = get_workspace_client(databricksHost)

    # Cleanup users in workspace that aren't in AAD Graph
    remove_deleted_users_in_workspace(workspace_client)
    synchronize_workspace_users(workspace_definition, workspace_client)

    return func.HttpResponse("Successfully synchronized workspace users.")