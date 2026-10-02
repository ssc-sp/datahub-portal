import importlib
import sys
import types
import unittest
from pathlib import Path
from unittest.mock import patch


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

    def test_keys_upper_converts_nested_keys(self) -> None:
        sample_message = {
            "message": {
                "appData": {
                    "databricksHostUrl": "https://example.databricks.net",
                }
            }
        }

        transformed = self.function_app.keys_upper(sample_message)

        self.assertEqual(
            transformed["Message"]["AppData"]["DatabricksHostUrl"],
            "https://example.databricks.net",
        )

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

    def test_resolve_databricks_host_url_supports_primary_key(self) -> None:
        workspace_definition = {
            "AppData": {
                "DatabricksHostUrl": "https://adb-123.azuredatabricks.net"
            }
        }

        host = self.function_app.resolve_databricks_host_url(workspace_definition)
        self.assertEqual("https://adb-123.azuredatabricks.net", host)

    def test_resolve_databricks_host_url_supports_camel_case_variant(self) -> None:
        workspace_definition = {
            "AppData": {
                "databricksHostUrl": "https://adb-456.azuredatabricks.net"
            }
        }

        host = self.function_app.resolve_databricks_host_url(workspace_definition)
        self.assertEqual("https://adb-456.azuredatabricks.net", host)

    def test_resolve_databricks_host_url_returns_none_when_missing(self) -> None:
        workspace_definition = {
            "AppData": {
                "appServiceConfiguration": None
            }
        }

        host = self.function_app.resolve_databricks_host_url(workspace_definition)
        self.assertIsNone(host)

    def test_resolve_databricks_host_url_returns_none_when_blank(self) -> None:
        workspace_definition = {
            "AppData": {
                "DatabricksHostUrl": "   "
            }
        }

        host = self.function_app.resolve_databricks_host_url(workspace_definition)
        self.assertIsNone(host)

    def test_sync_databricks_workspace_users_skips_when_host_missing(self) -> None:
        workspace_definition = {
            "Workspace": {"Acronym": "demo"},
            "AppData": {},
        }

        with patch.object(
            self.function_app.dtb_utils,
            "get_workspace_client",
            side_effect=AssertionError("get_workspace_client should not be called"),
            create=True,
        ):
            self.function_app.sync_databricks_workspace_users_function(workspace_definition)

    def test_new_sync_workspace_preserves_root_cause(self) -> None:
        workspace_definition = {
            "Workspace": {"Acronym": "demo"},
            "Templates": [{"Name": "azure-databricks"}],
        }

        def failing_sync(_workspace_definition):
            raise ValueError("databricks api failed")

        with patch.object(
            self.function_app,
            "get_sync_func_mappings",
            return_value={"azure-databricks": ("databricks users", failing_sync)},
        ), patch.object(
            self.function_app,
            "send_exception_to_service_bus",
            return_value=None,
        ), patch.object(
            self.function_app,
            "send_healthcheck_to_service_bus",
            return_value=None,
        ):
            with self.assertRaises(RuntimeError) as context:
                self.function_app.new_sync_workspace(workspace_definition)

        self.assertIn("Workspace demo had problems while synchronizing", str(context.exception))
        self.assertIsInstance(context.exception.__cause__, ValueError)
        self.assertEqual("databricks api failed", str(context.exception.__cause__))

    def test_keyvault_sync_serializes_vault_updates(self) -> None:
        import threading
        import time

        sys.path.insert(0, str(ROOT))
        lib_module = sys.modules.get("lib")
        if lib_module is not None:
            lib_module.__path__ = [str(ROOT / "lib")]
        else:
            lib_module = types.ModuleType("lib")
            lib_module.__path__ = [str(ROOT / "lib")]
            sys.modules["lib"] = lib_module

        azure_module = types.ModuleType("azure")
        identity_module = types.ModuleType("azure.identity")
        keyvault_module = types.ModuleType("azure.mgmt.keyvault")
        models_module = types.ModuleType("azure.mgmt.keyvault.models")

        class ClientSecretCredential:  # pragma: no cover - simple stub
            def __init__(self, *args, **kwargs):
                pass

        class AccessPolicyEntry:  # pragma: no cover - simple stub
            def __init__(self, tenant_id, object_id, permissions):
                self.tenant_id = tenant_id
                self.object_id = object_id
                self.permissions = types.SimpleNamespace(secrets=permissions.get("secrets", []))

        class VaultProperties:  # pragma: no cover - simple stub
            def __init__(self, access_policies=None):
                self.access_policies = access_policies or []

        class Vault:  # pragma: no cover - simple stub
            def __init__(self, access_policies=None):
                self.properties = VaultProperties(access_policies)

        class FakeKeyVaultManagementClient:  # pragma: no cover - simple stub
            def __init__(self):
                self.vaults = self
                self.calls = 0
                self.first_entered = threading.Event()
                self.allow_first_to_finish = threading.Event()

            def get(self, rg_name, vault_name):
                return Vault(access_policies=[])

            def begin_create_or_update(self, rg_name, vault_name, vault):
                self.calls += 1
                if self.calls == 1:
                    self.first_entered.set()
                    self.allow_first_to_finish.wait(timeout=2)
                return types.SimpleNamespace(result=lambda: "ok")

        identity_module.ClientSecretCredential = ClientSecretCredential
        keyvault_module.KeyVaultManagementClient = FakeKeyVaultManagementClient
        models_module.AccessPolicyEntry = AccessPolicyEntry
        models_module.VaultAccessPolicyParameters = object
        models_module.SecretPermissions = object

        sys.modules.update({
            "azure": azure_module,
            "azure.identity": identity_module,
            "azure.mgmt": types.ModuleType("azure.mgmt"),
            "azure.mgmt.keyvault": keyvault_module,
            "azure.mgmt.keyvault.models": models_module,
        })

        sys.modules.pop("lib.azkeyvault_utils", None)
        azkeyvault_utils = importlib.import_module("lib.azkeyvault_utils")
        azkeyvault_utils._keyvault_operation_lock = threading.Semaphore(1)

        definition = {
            "Workspace": {
                "Acronym": "demo",
                "Users": [{"ObjectId": "user-1", "Role": "Owner"}],
            }
        }

        client = FakeKeyVaultManagementClient()
        second_entered = []

        def first_call():
            with patch.object(azkeyvault_utils, "get_kv_reference", return_value=("rg", "vault")):
                azkeyvault_utils.synchronize_access_policies(client, "dev", definition, "tenant-id")

        def second_call():
            with patch.object(azkeyvault_utils, "get_kv_reference", return_value=("rg", "vault")):
                azkeyvault_utils.synchronize_access_policies(client, "dev", definition, "tenant-id")
                second_entered.append("entered")

        first_thread = threading.Thread(target=first_call)
        second_thread = threading.Thread(target=second_call)

        first_thread.start()
        self.assertTrue(client.first_entered.wait(timeout=2))
        second_thread.start()
        time.sleep(0.2)
        self.assertEqual(second_entered, [])

        client.allow_first_to_finish.set()
        first_thread.join(timeout=2)
        second_thread.join(timeout=2)

        self.assertEqual(len(second_entered), 1)
        self.assertTrue(azkeyvault_utils._keyvault_operation_lock.acquire(blocking=False))
        azkeyvault_utils._keyvault_operation_lock.release()

    def test_keyvault_sync_allows_parallel_updates_for_different_vaults(self) -> None:
        import threading
        import time

        sys.path.insert(0, str(ROOT))
        lib_module = sys.modules.get("lib")
        if lib_module is not None:
            lib_module.__path__ = [str(ROOT / "lib")]
        else:
            lib_module = types.ModuleType("lib")
            lib_module.__path__ = [str(ROOT / "lib")]
            sys.modules["lib"] = lib_module

        azure_module = types.ModuleType("azure")
        identity_module = types.ModuleType("azure.identity")
        keyvault_module = types.ModuleType("azure.mgmt.keyvault")
        models_module = types.ModuleType("azure.mgmt.keyvault.models")
        import importlib

        class ClientSecretCredential:  # pragma: no cover - simple stub
            def __init__(self, *args, **kwargs):
                pass

        class AccessPolicyEntry:  # pragma: no cover - simple stub
            def __init__(self, tenant_id, object_id, permissions):
                self.tenant_id = tenant_id
                self.object_id = object_id
                self.permissions = types.SimpleNamespace(secrets=permissions.get("secrets", []))

        class VaultProperties:  # pragma: no cover - simple stub
            def __init__(self, access_policies=None):
                self.access_policies = access_policies or []

        class Vault:  # pragma: no cover - simple stub
            def __init__(self, access_policies=None):
                self.properties = VaultProperties(access_policies)

        class FakeKeyVaultManagementClient:  # pragma: no cover - simple stub
            def __init__(self):
                self.vaults = self
                self.calls = 0
                self.first_entered = threading.Event()
                self.allow_first_to_finish = threading.Event()

            def get(self, rg_name, vault_name):
                return Vault(access_policies=[])

            def begin_create_or_update(self, rg_name, vault_name, vault):
                self.calls += 1
                if self.calls == 1:
                    self.first_entered.set()
                    self.allow_first_to_finish.wait(timeout=2)
                return types.SimpleNamespace(result=lambda: "ok")

        identity_module.ClientSecretCredential = ClientSecretCredential
        keyvault_module.KeyVaultManagementClient = FakeKeyVaultManagementClient
        models_module.AccessPolicyEntry = AccessPolicyEntry
        models_module.VaultAccessPolicyParameters = object
        models_module.SecretPermissions = object

        sys.modules.update({
            "azure": azure_module,
            "azure.identity": identity_module,
            "azure.mgmt": types.ModuleType("azure.mgmt"),
            "azure.mgmt.keyvault": keyvault_module,
            "azure.mgmt.keyvault.models": models_module,
        })

        sys.modules.pop("lib.azkeyvault_utils", None)
        azkeyvault_utils = importlib.import_module("lib.azkeyvault_utils")
        azkeyvault_utils._keyvault_operation_lock = threading.Semaphore(1)
        azkeyvault_utils._keyvault_operation_locks = {}

        definition_one = {
            "Workspace": {
                "Acronym": "demo",
                "Users": [{"ObjectId": "user-1", "Role": "Owner"}],
            }
        }
        definition_two = {
            "Workspace": {
                "Acronym": "other",
                "Users": [{"ObjectId": "user-2", "Role": "Owner"}],
            }
        }

        second_entered = []
        client = FakeKeyVaultManagementClient()

        def first_call():
            with patch.object(azkeyvault_utils, "get_kv_reference", return_value=("rg", "vault-1")):
                azkeyvault_utils.synchronize_access_policies(client, "dev", definition_one, "tenant-id")

        def second_call():
            with patch.object(azkeyvault_utils, "get_kv_reference", return_value=("rg", "vault-2")):
                azkeyvault_utils.synchronize_access_policies(client, "dev", definition_two, "tenant-id")
                second_entered.append("entered")

        first_thread = threading.Thread(target=first_call)
        second_thread = threading.Thread(target=second_call)

        first_thread.start()
        self.assertTrue(client.first_entered.wait(timeout=2))
        second_thread.start()
        time.sleep(0.2)
        self.assertEqual(second_entered, ["entered"])

        client.allow_first_to_finish.set()
        first_thread.join(timeout=2)
        second_thread.join(timeout=2)


if __name__ == "__main__":
    unittest.main()
