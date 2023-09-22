import azure.functions as func
from pprint import pprint

app = func.FunctionApp()

@app.function_name(name="SynchronizeWorkspaceUsers")
@app.route(route="sync-workspace-users") # HTTP Trigger
def test_function(req: func.HttpRequest) -> func.HttpResponse:

    workspace_definition = req.get_json()

    

    return func.HttpResponse("HttpTrigger1 function processed a request!!!")