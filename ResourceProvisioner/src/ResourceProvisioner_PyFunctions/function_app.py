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
    logging.info("Synchronizing workspace users.")

    logging.info("Synchronizing databricks users.")
    sync_databricks_workspace_users_function(workspace_definition)
    
    logging.info("Synchronizing keyvault users.")
    sync_keyvault_workspace_users_function(workspace_definition)

    logging.info("Synchronizing storage account policies")
    sync_storage_workspace_users_function(workspace_definition)

    logging.info("Successfully synchronized workspace users.")
    return func.HttpResponse("Successfully synchronized workspace users.")

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
    logging.info("Synchronizing workspace users.")
    logging.info("Synchronizing databricks users.")
    sync_databricks_workspace_users_function(workspace_definition)
    logging.info("Synchronizing keyvault users.")
    sync_keyvault_workspace_users_function(workspace_definition)
    logging.info("Successfully synchronized workspace users.")
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

    
''' testing the keys_upper function
test_dict = {
          "messageId": "3d040000-57a5-f2c6-495b-08dc799c5645",
          "requestId": None,
          "correlationId": None,
          "conversationId": "3d040000-57a5-f2c6-4f14-08dc799c564e",
              "initiatorId": None,
              "sourceAddress": "sb://fsdh-service-bus-dev.servicebus.windows.net/74eb84dbb3a8_DatahubFunctions_bus_8wnyyynzwz3cpn6jbdq8ufkxyg?autodelete=300",
              "destinationAddress": "sb://fsdh-service-bus-dev.servicebus.windows.net/user-run-request",
              "responseAddress": None,
              "faultAddress": None,
              "messageType": [
                "urn:message:Datahub.Shared.Entities:WorkspaceDefinition"
              ],
              "message": {
                "templates": [
                  {
                    "name": "azure-storage-blob"
                  },
                  {
                    "name": "new-project-template"
                  },
                  {
                    "name": "azure-databricks"
                  }
                ],
                "workspace": {
                  "name": "Pull request test 2",
                  "acronym": "PRT2",
                  "budgetAmount": 100,
                  "subscriptionId": "bc4bcb08-d617-49f4-b6af-69d6f10c240b",
                  "version": "v3.4.0",
                  "storageSizeLimitInTB": 5,
                  "terraformOrganization": {
                    "name": "TODO",
                    "code": "TODO"
                  },
                  "users": [
                    {
                      "objectId": "db1a6b8d-0a8c-4070-8284-beb46ac3a5f6",
                      "email": "david.rene@ssc-spc.gc.ca",
                      "role": "Owner"
                    }
                  ]
                },
                "appData": {
                  "databricksHostUrl": "https://adb-4423571622710923.3.azuredatabricks.net",
                  "appServiceConfiguration": None
                },
                "requestingUserEmail": "terraform-output-handler"
              },
              "expirationTime": None,
              "sentTime": "2024-05-21T13:45:54.9008219Z",
              "headers": {},
              "host": {
                "machineName": "74eb84dbb3a8",
                "processName": "Datahub.Functions",
                "processId": 1085,
                "assembly": "Datahub.Functions",
                "assemblyVersion": "1.0.0.0",
                "frameworkVersion": "8.0.5",
                "massTransitVersion": "8.2.2.0",
                "operatingSystemVersion": "Unix 5.15.153.1"
              }
            }
new_dict = keys_upper(test_dict)
print(new_dict)
'''


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