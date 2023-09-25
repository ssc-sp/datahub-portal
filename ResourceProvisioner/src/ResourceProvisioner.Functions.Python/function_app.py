import azure.functions as func
import os
import pprint as pprint

from lib.databricks_utils import get_workspace_client, remove_deleted_users_in_workspace, synchronize_workspace_users

app = func.FunctionApp()

@app.function_name(name="SynchronizeWorkspaceUsers")
@app.route(route="sync-workspace-users") # HTTP Trigger
def test_function(req: func.HttpRequest) -> func.HttpResponse:

    workspace_definition = req.get_json()

    databricksHost = workspace_definition['Apps']['azure-databricks']['HostUrl']
    databricksToken = os.environ['DATABRICKS_TOKEN']

    workspace_client = get_workspace_client(databricksHost, databricksToken)

    # Cleanup users in workspace that aren't in AAD Graph
    remove_deleted_users_in_workspace(workspace_client)
    synchronize_workspace_users(workspace_definition, workspace_client)


    return func.HttpResponse("Successfully synchronized workspace users.")