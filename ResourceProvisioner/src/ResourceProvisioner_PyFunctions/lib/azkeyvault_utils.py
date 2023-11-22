from azure.identity import DefaultAzureCredential
from azure.keyvault.secrets import SecretClient
from azure.keyvault.administration import KeyVaultAccessControlClient
from azure.mgmt.keyvault import KeyVaultManagementClient
from azure.mgmt.authorization import AuthorizationManagementClient
from azure.identity import ClientSecretCredential
from azure.core.exceptions import ResourceNotFoundError
from msgraph import GraphServiceClient
from msgraph.generated.directory_objects.get_by_ids.get_by_ids_post_request_body import GetByIdsPostRequestBody
import os
from typing import NamedTuple
from azure.identity.aio import EnvironmentCredential
import asyncio
import requests

class AzClients(NamedTuple):
    """a docstring"""
    kv_client: KeyVaultManagementClient
    graph_client: GraphServiceClient

def get_keyvault_client(subscription_id, tenant_id) -> AzClients:
    """
    Retrieves a Key Vault client for the specified environment and workspace definition.

    Args:
        environment_name (str): The name of the environment.
        workspace_definition (dict): The definition of the workspace.

    Returns:
        keyvault_client: The Key Vault client object.

    """
    credential = ClientSecretCredential(
        tenant_id=tenant_id,
        client_id=os.environ["AzureClientId"],
        client_secret=os.environ["AzureClientSecret"])    
    scopes = ['https://graph.microsoft.com/.default']
    graph_client = GraphServiceClient(credentials=credential, scopes=scopes)

    kv_client = KeyVaultManagementClient(
        credential=credential,
        subscription_id=subscription_id
    )
    
    return AzClients(kv_client,graph_client)


def synchronize_access_policies(clients:AzClients, environment_name, workspace_definition):
    rg_name = f"fsdh_proj_{workspace_definition['Workspace']['Acronym']}_{environment_name}_rg"
    vault_name = f"fsdh-proj-{workspace_definition['Workspace']['Acronym']}-{environment_name}-kv"
    print(f"using vault_uri: [{rg_name}].[{vault_name}]")
    # Replace these values with your Azure Key Vault details

    # Create a SecretClient using the default Azure credential from Azure Identity

    # Get access policies
    vault = clients.kv_client.vaults.get(rg_name, vault_name)

    # collect all the object ids
    #object_ids = [policy.object_id for policy in vault.properties.access_policies]
    #output = asyncio.run(collect_ms_graph_properties(clients,object_ids))
    # Print access policies
    for policy in vault.properties.access_policies:        
        print(f"User {user} has permissions: {policy.permissions}")
        print(policy)        

async def get_user(graph_client: GraphServiceClient, object_id: str) -> bool:
    try:
        user = await graph_client.users.get(object_id)
        return user is not None
    except GraphErrorException as e:
        if e.status_code == 404:
            return None
        else:
            raise

async def collect_ms_graph_properties(clients:AzClients,object_id_list):
    
    request_body = GetByIdsPostRequestBody(
	ids = object_id_list,
	types = [
		"user",
#		"group",
#		"device",
	],)
    results = await clients.graph_client.directory_objects.get_by_ids.post(request_body)
    print (results)

    

