import azure.functions as func
import logging
import lib.databricks_utils as dtb_utils
import lib.azkeyvault_utils as azkv_utils
import lib.azstorage_utils as azsg_utils
import os
import json

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
    workspace_name = workspace_definition["Workspace"]["Acronym"]
    synchronize_workspace(workspace_definition)
    return func.HttpResponse(f"Successfully synchronized workspace users for {workspace_name}.")

@app.function_name(name="SynchronizeWorkspaceUsersQueueTrigger")
@app.service_bus_queue_trigger(arg_name="msg", queue_name="user-run-request", connection="DatahubServiceBus") # Queue Trigger
def queue_sync_workspace_users_function(msg: func.ServiceBusMessage):
    """
    Synchronizes the users in the Databricks workspace with the users in the definition file.

    Args:
        workspace_definition (ServiceBusMessage): The workspace definition file.

    Returns:
        None

    """
    message_envelope = json.loads(msg.get_body().decode('utf-8'))
    workspace_definition = message_envelope['message']
    workspace_definition = keys_upper(workspace_definition)
    synchronize_workspace(workspace_definition)
    return None
    
def keys_upper(dictionary):
    """
    Converts the key's first letter in the dictionary to uppercase.

    Args:
        dict (dict): The dictionary.

    Returns:
        dict: The dictionary with uppercase first letter keys.

    """
    res = dict()
    for key in dictionary.keys():
        if isinstance(dictionary[key], dict):
            res[key[0].upper()+key[1:]] = keys_upper(dictionary[key])
        elif isinstance(dictionary[key], list):
            if dictionary[key] and isinstance(dictionary[key][0], dict):
                res[key[0].upper()+key[1:]] = [keys_upper(item) for item in dictionary[key]]
        else:
            res[key[0].upper()+key[1:]] = dictionary[key]
    return res
   
def synchronize_workspace(workspace_definition):
    workspace_name = workspace_definition["Workspace"]["Acronym"]
    logging.info(f"Synchronizing workspace users for {workspace_name}.")

    logging.info(f"Synchronizing databricks users for {workspace_name}.")
    all_synchronized = False
    last_exception = None
    try:
        sync_databricks_workspace_users_function(workspace_definition)
        all_synchronized = all_synchronized and True
    except Exception as e:
        last_exception = e
        all_synchronized = False
        logging.error(f"Error synchronizing databricks users for {workspace_name}. {e}")
    #    return func.HttpResponse(f"Error synchronizing databricks users for {workspace_name}. {e}", status_code=500)    
    
    logging.info(f"Synchronizing keyvault users for {workspace_name}.")
    try:
        sync_keyvault_workspace_users_function(workspace_definition)
        all_synchronized = all_synchronized and True
    except Exception as e:
        last_exception = e
        all_synchronized = False
        logging.error(f"Error synchronizing keyvault users for {workspace_name}. {e}")

    logging.info(f"Synchronizing storage account policies for {workspace_name}")
    try:
        sync_storage_workspace_users_function(workspace_definition)
        all_synchronized = all_synchronized and True
    except Exception as e:
        last_exception = e
        all_synchronized = False
        logging.error(f"Error synchronizing storage account policies for {workspace_name}. {e}")

    if last_exception is not None:
        raise last_exception
    logging.info(f"Successfully synchronized workspace users for {workspace_name}.")

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
    subscription_id = os.environ["AzureSubscriptionId"]

    workspace_client = dtb_utils.get_workspace_client(databricksHost)

    # Cleanup users in workspace that aren't in AAD Graph
    dtb_utils.remove_deleted_users_in_workspace(workspace_definition, workspace_client)
    dtb_utils.synchronize_workspace_users(workspace_definition, workspace_client)

    dtb_utils.synchronize_workspace_secret_scopes(environment_name, subscription_id, workspace_definition, workspace_client)    
    #dtb_utils.synchronize_workspace_secrets(environment_name, subscription_id, workspace_definition, workspace_client)  

def sync_keyvault_workspace_users_function(workspace_definition):
    """
    Synchronizes the users in the keyvault with the users in the definition file.

    Args:
        workspace_definition (dict): The workspace definition file.

    Returns:
        None

    """
    # get environment name from environment variables
    environment_name = os.environ["DataHub_ENVNAME"]   
    subscription_id = os.environ["AzureSubscriptionId"]
    tenantId = os.environ["AzureTenantId"]

    kv_client = azkv_utils.get_keyvault_client(subscription_id, tenantId)
    azkv_utils.synchronize_access_policies(kv_client,environment_name, workspace_definition, tenantId)

    # Cleanup users in workspace that aren't in AAD Graph
    #remove_deleted_users_in_workspace(workspace_client)
    #synchronize_workspace_users(workspace_definition, workspace_client)

def sync_storage_workspace_users_function(workspace_definition):
    """
    Synchronizes the users in the storage account with the users in the definition file.

    Args:
        workspace_definition (dict): The workspace definition file.

    Returns:
        None
 
    """
    # get environment name from environment variables
    environment_name = os.environ["DataHub_ENVNAME"]   
    subscription_id = os.environ["AzureSubscriptionId"]
    tenantId = os.environ["AzureTenantId"]

    sg_client = azsg_utils.get_authorization_client(subscription_id, tenantId)
    azsg_utils.synchronize_access_policies(sg_client,subscription_id, environment_name, workspace_definition)

    # Cleanup users in workspace that aren't in AAD Graph
    #remove_deleted_users_in_workspace(workspace_client)
    #synchronize_workspace_users(workspace_definition, workspace_client) 

