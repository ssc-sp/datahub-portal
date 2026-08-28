import importlib
import sys
import types
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]


class FunctionAppMappingTests(unittest.TestCase):
    def setUp(self) -> None:
        self._install_stub_modules()
        sys.path.insert(0, str(ROOT))
        sys.modules.pop("function_app", None)
        self.function_app = importlib.import_module("function_app")

    def tearDown(self) -> None:
        sys.modules.pop("function_app", None)

    def _install_stub_modules(self) -> None:
        azure_module = types.ModuleType("azure")
        functions_module = types.ModuleType("azure.functions")

        class HttpRequest:  # pragma: no cover - simple stub
            pass

        class HttpResponse:  # pragma: no cover - simple stub
            def __init__(self, body="", status_code=200):
                self.body = body
                self.status_code = status_code

        class FunctionApp:  # pragma: no cover - simple stub
            def function_name(self, *args, **kwargs):
                return lambda func: func

            def route(self, *args, **kwargs):
                return lambda func: func

            def service_bus_queue_trigger(self, *args, **kwargs):
                return lambda func: func

        functions_module.HttpRequest = HttpRequest
        functions_module.HttpResponse = HttpResponse
        functions_module.FunctionApp = FunctionApp
        functions_module.ServiceBusMessage = object
        azure_module.functions = functions_module

        servicebus_module = types.ModuleType("azure.servicebus")
        class ServiceBusClient:  # pragma: no cover - simple stub
            fully_qualified_namespace = "ns"
            @classmethod
            def from_connection_string(cls, *args, **kwargs):
                return cls()
            def __enter__(self):
                return self
            def __exit__(self, exc_type, exc, tb):
                return False
            def get_queue_sender(self, *args, **kwargs):
                return self
            def send_messages(self, *args, **kwargs):
                return None
        servicebus_module.ServiceBusClient = ServiceBusClient
        servicebus_module.TransportType = types.SimpleNamespace(AmqpOverWebsocket="AmqpOverWebsocket")
        servicebus_module.ServiceBusMessage = object

        bug_report_module = types.ModuleType("bug_report_message")
        bug_report_module.BugReportMessage = object

        healthcheck_module = types.ModuleType("healthcheck_message")
        class HealthcheckMessage:  # pragma: no cover - simple stub
            TYPE_WORKSPACE_SYNC = "workspace-sync"
            STATUS_HEALTHY = "healthy"
            STATUS_UNHEALTHY = "unhealthy"
            def __init__(self, *args, **kwargs):
                self.args = args
                self.kwargs = kwargs
        healthcheck_module.HealthcheckMessage = HealthcheckMessage

        queue_utils_module = types.ModuleType("lib.queue_utils")
        class MassTransitMessage:  # pragma: no cover - simple stub
            TYPE_BUG_REPORT = "bug"
            TYPE_HEALTH_CHECK_RESULT = "health"
            def __init__(self, *args, **kwargs):
                self.messageId = "message-id"
            def to_json(self):
                return "{}"
        queue_utils_module.MassTransitMessage = MassTransitMessage

        lib_module = types.ModuleType("lib")
        lib_module.__path__ = []
        databricks_utils = types.ModuleType("lib.databricks_utils")
        azkeyvault_utils = types.ModuleType("lib.azkeyvault_utils")
        azstorage_utils = types.ModuleType("lib.azstorage_utils")
        sys.modules.update({
            "azure": azure_module,
            "azure.functions": functions_module,
            "azure.servicebus": servicebus_module,
            "bug_report_message": bug_report_module,
            "healthcheck_message": healthcheck_module,
            "lib": lib_module,
            "lib.databricks_utils": databricks_utils,
            "lib.azkeyvault_utils": azkeyvault_utils,
            "lib.azstorage_utils": azstorage_utils,
            "lib.queue_utils": queue_utils_module,
        })

    def test_new_project_template_invokes_storage_sync(self) -> None:
        mappings = self.function_app.get_sync_func_mappings()
        self.assertIn("new-project-template", mappings)

        _, handler = mappings["new-project-template"]
        self.assertTrue(callable(handler))

        calls = []
        self.function_app.sync_keyvault_workspace_users_function = lambda workspace_definition: calls.append("keyvault")
        self.function_app.sync_storage_workspace_users_function = lambda workspace_definition: calls.append("storage")

        handler({"Workspace": {"Acronym": "demo"}})

        self.assertEqual(calls, ["keyvault", "storage"])


if __name__ == "__main__":
    unittest.main()
