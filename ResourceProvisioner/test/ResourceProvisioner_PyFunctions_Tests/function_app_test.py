# test_my_function.py
import unittest
from unittest.mock import MagicMock
import azure.functions as func
import json
# importing sys
import sys
from pathlib import Path
 
# adding Folder_2/subfolder to the system path
sys.path.insert(0, '../../src/ResourceProvisioner_PyFunctions')

import function_app as app # import http_sync_workspace_users_function
#from function_app_test import main

class TestResourceProvisioner(unittest.TestCase):

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
             
        #self.assertEqual(response.get_body(), b"Hello, Azure Functions!")

if __name__ == '__main__':
    unittest.main()