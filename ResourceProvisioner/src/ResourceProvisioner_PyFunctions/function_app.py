import azure.functions as func
import logging
import lib.databricks_utils as dtb_utils
import lib.azkeyvault_utils as azkv_utils
import os

#from lib.databricks_utils import get_workspace_client, remove_deleted_users_in_workspace, synchronize_workspace_users

app = func.FunctionApp()

@app.function_name(name="SynchronizeWorkspaceUsersHttpTrigger")
@app.route(route="sync-workspace-users") # HTTP Trigger
def http_sync_workspace_users_function(req: func.HttpRequest) -> func.HttpResponse:
    """
    Synchronizes the users in the Databricks workspace with the users in the definition file.

    Args:
        req (HttpRequest): The HTTP request object.

    Returns:
        HttpResponse: The HTTP response object.

    """
    workspace_definition = req.get_json()
    logging.info("Synchronizing workspace users.")
    logging.info("Synchronizing databricks users.")
    sync_databricks_workspace_users_function(workspace_definition)
    logging.info("Synchronizing keyvault users.")
    sync_keyvault_workspace_users_function(workspace_definition)
    logging.info("Successfully synchronized workspace users.")

    return func.HttpResponse("Successfully synchronized workspace users.")

@app.function_name(name="SynchronizeWorkspaceUsersQueueTrigger")
@app.queue_trigger(arg_name="msg", queue_name="user-run-request", 
                   connection="AzureStorageQueueConnectionString") # Queue Trigger

def queue_sync_workspace_users_function(msg: func.QueueMessage) -> None:
    """
    Synchronizes the users in the Databricks workspace with the users in the definition file.

    Args:
        workspace_definition (QueueMessage): The workspace definition file.

    Returns:
        None

    """
    workspace_definition = msg.get_json()
    logging.info("Synchronizing workspace users.")
    logging.info("Synchronizing databricks users.")
    sync_databricks_workspace_users_function(workspace_definition)
    logging.info("Synchronizing keyvault users.")
    sync_keyvault_workspace_users_function(workspace_definition)
    logging.info("Successfully synchronized workspace users.")
    return None


def sync_databricks_workspace_users_function(workspace_definition):
    """
    Synchronizes the users in the Databricks workspace with the users in the definition file.

    Args:
        workspace_definition (dict): The workspace definition file.

    Returns:
        None

    """
    databricksHost = workspace_definition['AppData']['DatabricksHostUrl']
    environment_name = os.environ["DataHub_ENVNAME"]   
    subscription_id = os.environ["AZURE_SUBSCRIPTION_ID"]

    workspace_client = dtb_utils.get_workspace_client(databricksHost)

    # Cleanup users in workspace that aren't in AAD Graph
    dtb_utils.remove_deleted_users_in_workspace(workspace_definition, workspace_client)
    dtb_utils.synchronize_workspace_users(workspace_definition, workspace_client)

    dtb_utils.synchronize_workspace_secret_scopes(environment_name, subscription_id, workspace_definition, workspace_client)    
    #dtb_utils.synchronize_workspace_secrets(environment_name, subscription_id, workspace_definition, workspace_client)  

def sync_keyvault_workspace_users_function(workspace_definition):
    """
    Synchronizes the users in the Databricks workspace with the users in the definition file.

    Args:
        workspace_definition (dict): The workspace definition file.

    Returns:
        None

    """
    # get environment name from environment variables
    environment_name = os.environ["DataHub_ENVNAME"]   
    subscription_id = os.environ["AZURE_SUBSCRIPTION_ID"]
    tenantId = os.environ["AzureTenantId"]

    kv_client = azkv_utils.get_keyvault_client(subscription_id, tenantId)
    azkv_utils.synchronize_access_policies(kv_client,environment_name, workspace_definition, tenantId)

    # Cleanup users in workspace that aren't in AAD Graph
    #remove_deleted_users_in_workspace(workspace_client)
    #synchronize_workspace_users(workspace_definition, workspace_client)



# ####################################################################################
# # Temporary function to run the sync function in INT and POC environments 
# ####################################################################################



# @app.function_name(name="TempIntSynchronizeWorkspaceUsersQueueTrigger")
# @app.queue_trigger(arg_name="msg", queue_name="user-run-request", 
#                    connection="TempIntConnectionString") # Queue Trigger

# def queue_sync_workspace_users_function(msg: func.QueueMessage) -> None:
#     """
#     Synchronizes the users in the Databricks workspace with the users in the definition file.

#     Args:
#         workspace_definition (QueueMessage): The workspace definition file.

#     Returns:
#         None

#     """
#     workspace_definition = msg.get_json()
#     logging.info("Synchronizing workspace users.")
    
#     sync_workspace_users_function(workspace_definition)

#     logging.info("Successfully synchronized workspace users.")
#     return None

# @app.function_name(name="TempPocSynchronizeWorkspaceUsersQueueTrigger")
# @app.queue_trigger(arg_name="msg", queue_name="user-run-request", 
#                    connection="TempPocConnectionString") # Queue Trigger

# def queue_sync_workspace_users_function(msg: func.QueueMessage) -> None:
#     """
#     Synchronizes the users in the Databricks workspace with the users in the definition file.

#     Args:
#         workspace_definition (QueueMessage): The workspace definition file.

#     Returns:
#         None

#     """
#     workspace_definition = msg.get_json()
#     logging.info("Synchronizing workspace users.")
    
#     sync_workspace_users_function(workspace_definition)

#     logging.info("Successfully synchronized workspace users.")
#     return None