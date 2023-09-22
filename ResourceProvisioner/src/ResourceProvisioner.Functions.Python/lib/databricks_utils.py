from databricks.sdk import WorkspaceClient

def get_workspace_client(databricksHost, databricksToken):

    # Create the workspace client
    w = WorkspaceClient(
        host = databricksHost,
        token= databricksToken
    )
    return w