# test_my_function.py
import unittest
from unittest.mock import MagicMock
import azure.functions as func
import json
import fsdh_utils
# importing sys
import sys
from pathlib import Path
 
# adding Folder_2/subfolder to the system path
sys.path.insert(0, '../../src/ResourceProvisioner_PyFunctions')

import function_app as app # import http_sync_workspace_users_function
#from function_app_test import main

sample_message = {
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


class TestResourceProvisioner(unittest.TestCase):


    def test_uppercasing(self):
        # testing the keys_upper function

        new_dict = app.keys_upper(sample_message)
        assert new_dict['Message']['AppData']['DatabricksHostUrl'] == "https://adb-4423571622710923.3.azuredatabricks.net"        

    def test_parse_message(self):
        # create fake ServiceBusMessage
        msg = MagicMock(spec_set=func.ServiceBusMessage)
        msg.get_body.return_value = json.dumps(sample_message).encode('utf-8')
        # Call the function.
        func_call = app.queue_sync_workspace_users_function.build().get_user_function()
        resp = func_call(msg)
        # Assert
        self.assertEqual(resp, None)

    def test_sync_all(self):
        # Arrange
        workspace_data = Path('at3_workspace_data.json').read_text()
        req = func.HttpRequest(
                    method='POST',
                    body=workspace_data.encode('utf8'), #json.dumps({'name': 'Test'}).encode('utf8'),
                    url='/api/HttpTrigger',
                    params=None)        
        #request = MagicMock(spec_set=func.HttpRequest)
        # Call the function.        
        func_call = app.http_sync_workspace_users_function.build().get_user_function()
        resp = func_call(req)        

        # Assert
        self.assertEqual(resp.status_code, 200)
        #self.assertEqual(response.get_body(), b"Hello, Azure Functions!")
        #       
        
    def test_databricks_sync(self):
        # Arrange
        workspace_data = Path('at3_workspace_data.json').read_text()
        json_workspace = json.loads(workspace_data)
        app.sync_databricks_workspace_users_function(json_workspace)
       
        #self.assertEqual(response.get_body(), b"Hello, Azure Functions!")

    def test_azkeyvault_sync(self):
        # Arrange
        workspace_data = Path('at3_workspace_data.json').read_text()
        json_workspace = json.loads(workspace_data)
     
        # Call the function.
        app.sync_keyvault_workspace_users_function(json_workspace)


    def test_storage_sync(self):
        # Arrange
        workspace_data = Path('at3_workspace_data.json').read_text()
        json_workspace = json.loads(workspace_data)
     
        # Call the function.
        app.sync_storage_workspace_users_function(json_workspace)
             
        #self.assertEqual(response.get_body(), b"Hello, Azure Functions!")       

if __name__ == '__main__':
    unittest.main()