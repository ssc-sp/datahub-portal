# test_my_function.py
import unittest
from unittest.mock import MagicMock
import azure.functions as func
import json
# importing sys
import sys
 
# adding Folder_2/subfolder to the system path
sys.path.insert(0, 'ResourceProvisioner/src/ResourceProvisioner_PyFunctions')

from function_app import http_sync_workspace_users_function
#from function_app_test import main

class TestResourceProvisioner(unittest.TestCase):
    def test_main(self):
        # Arrange
        req = func.HttpRequest(
                    method='POST',
                    body=json.dumps({'name': 'Test'}).encode('utf8'),
                    url='/api/HttpTrigger',
                    params=None)        
        #request = MagicMock(spec_set=func.HttpRequest)
        # Call the function.
        func_call = http_sync_workspace_users_function.build().get_user_function()
        resp = func_call(req)        

        # Assert
        self.assertEqual(resp.status_code, 200)
        #self.assertEqual(response.get_body(), b"Hello, Azure Functions!")

if __name__ == '__main__':
    unittest.main()