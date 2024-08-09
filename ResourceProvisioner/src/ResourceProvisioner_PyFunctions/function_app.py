import azure.functions as func
import logging
import lib.databricks_utils as dtb_utils
import lib.azkeyvault_utils as azkv_utils
import lib.azstorage_utils as azsg_utils
import azure.servicebus as servicebus
import os
import json
import bug_report_message as brm
import healthcheck_message as hcm

#from lib.databricks_utils import get_workspace_client, remove_deleted_users_in_workspace, synchronize_workspace_users
#from azure.servicebus import ServiceBusClient, ServiceBusMessage

ERROR_DETAILS_JOINER = "; "

app = func.FunctionApp()

def get_config():
    asb_connection_str = os.getenv('DatahubServiceBus')
    queue_name = os.getenv('AzureServiceBusQueueName4Bugs') or "bug-report"
    check_results_queue_name = os.getenv('AzureServiceBusQueueName4Results') or "infrastructure-health-check-results"
    return asb_connection_str, queue_name, check_results_queue_name

def get_sync_func_mappings():
    mappings = {
        "new-project-template": ("keyvault users", sync_keyvault_workspace_users_function),
        "azure-storage-blob": ("storage account policies", sync_storage_workspace_users_function),
        "azure-databricks": ("databricks users", sync_databricks_workspace_users_function)
    }
    return mappings

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
    # synchronize_workspace(workspace_definition)
    new_sync_workspace(workspace_definition)
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
    # synchronize_workspace(workspace_definition)
    new_sync_workspace(workspace_definition)
    return None

def send_exception_to_service_bus(exception_message):
    asb_connection_str, queue_name, check_results_queue_name = get_config()
    bug_report = brm.BugReportMessage(
        UserName="Datahub Portal",
        UserEmail="",
        UserOrganization="",
        PortalLanguage="",
        PreferredLanguage="",
        Timezone="",
        Workspaces="",
        Topics="Workspace Syncronization",
        URL="",
        UserAgent="",
        Resolution="",
        LocalStorage="",
        BugReportType="Synchronizing workspace error",
        Description=exception_message
    )
    with servicebus.ServiceBusClient.from_connection_string(asb_connection_str, transport_type=servicebus.TransportType.AmqpOverWebsocket) as client:
        with client.get_queue_sender(queue_name) as sender:
            message = servicebus.ServiceBusMessage(bug_report.to_json())
            sender.send_messages(message)
            print(f"Sent message to queue: {queue_name}")

def send_healthcheck_to_service_bus(message):
    try:
        asb_connection_str, queue_name, check_results_queue_name = get_config()
        with servicebus.ServiceBusClient.from_connection_string(asb_connection_str, transport_type=servicebus.TransportType.AmqpOverWebsocket) as client:
            with client.get_queue_sender(check_results_queue_name) as sender:
                message = servicebus.ServiceBusMessage(message.to_json())
                sender.send_messages(message)
                print(f"Sent message to queue: {check_results_queue_name}")
    except Exception as e:
        logging.error(f"An error occurred while sending health check to service bus: {e}")

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
   
def new_sync_workspace(workspace_definition):
    sync_mappings = get_sync_func_mappings()
    logging.info("Got template mappings")

    errors = []

    workspace_name = workspace_definition["Workspace"]["Acronym"]
    logging.info(f"Synchronizing workspace users for {workspace_name}")

    template_names = [t["Name"] for t in workspace_definition["Templates"]]
    logging.info(f"Got template names: {template_names}")

    for t in template_names:
        name, sync_fn = sync_mappings[t]
        logging.debug(f"name: {name}, func: {sync_fn.__name__}")

        try:
            logging.info(f"Synchronizing {name} for {workspace_name}.")
            sync_fn(workspace_definition)
        except Exception as e:
            error_msg = f"Error synchronizing {name} for {workspace_name}: {e}"
            logging.error(error_msg)
            errors.append(error_msg)
            send_exception_to_service_bus(error_msg)
    
    errors_joined = ERROR_DETAILS_JOINER.join(errors)
    health_status = hcm.HealthcheckMessage.STATUS_UNHEALTHY if errors_joined else hcm.HealthcheckMessage.STATUS_HEALTHY
    health_msg = hcm.HealthcheckMessage(hcm.HealthcheckMessage.TYPE_WORKSPACE_SYNC, "workspaces", workspace_name, errors_joined, health_status)
    send_healthcheck_to_service_bus(health_msg)
    if (health_status == hcm.HealthcheckMessage.STATUS_HEALTHY):
        logging.info(f"Successfully synced workspace {workspace_name}")
    else:
        overall_sync_error = f"Workspace {workspace_name} had problems while synchronizing: {errors_joined}"
        logging.error(overall_sync_error)
        raise RuntimeError(overall_sync_error)

def synchronize_workspace(workspace_definition):
    workspace_name = workspace_definition["Workspace"]["Acronym"]
    logging.info(f"Synchronizing workspace users for {workspace_name}.")

    logging.info(f"Synchronizing databricks users for {workspace_name}.")
    all_synchronized = False
    last_exception = None
    healthcheck_message = hcm.HealthcheckMessage(3, "workspaces", workspace_name, "")
    try:
        sync_databricks_workspace_users_function(workspace_definition)
        all_synchronized = all_synchronized and True
        healthcheck_message.Status = 2
    except Exception as e:
        exception_message = f"Error synchronizing databricks users for {workspace_name}: {e}"
        logging.exception(exception_message)    
        send_exception_to_service_bus(exception_message)
        last_exception = e
        all_synchronized = False
        healthcheck_message.Status = 4
        healthcheck_message.Details = exception_message
    #    return func.HttpResponse(f"Error synchronizing databricks users for {workspace_name}. {e}", status_code=500)    
    
    logging.info(f"Synchronizing keyvault users for {workspace_name}.")
    try:
        sync_keyvault_workspace_users_function(workspace_definition)
        all_synchronized = all_synchronized and True
    except Exception as e:
        exception_message = f"Error synchronizing keyvault users for {workspace_name}: {e}"
        logging.exception(exception_message)    
        send_exception_to_service_bus(exception_message)
        last_exception = e
        all_synchronized = False
        healthcheck_message.Status = 4
        healthcheck_message.Details = healthcheck_message.Details + exception_message

    logging.info(f"Synchronizing storage account policies for {workspace_name}")
    try:
        sync_storage_workspace_users_function(workspace_definition)
        all_synchronized = all_synchronized and True
    except Exception as e:
        exception_message = f"Error synchronizing storage account policies for {workspace_name}: {e}"
        logging.exception(exception_message)    
        send_exception_to_service_bus(exception_message)
        last_exception = e
        all_synchronized = False
        healthcheck_message.Status = 4
        healthcheck_message.Details = healthcheck_message.Details + exception_message

    send_healthcheck_to_service_bus(healthcheck_message)
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

    # TODO: send a DatabricksSync (type 9) health check result to the queue

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

