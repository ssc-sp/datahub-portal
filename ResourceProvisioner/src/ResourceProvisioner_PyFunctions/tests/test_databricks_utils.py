import importlib
import sys
import types
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]


class DatabricksUtilsUnityCatalogTests(unittest.TestCase):
    def setUp(self) -> None:
        self._install_stub_modules()
        sys.path.insert(0, str(ROOT))
        sys.modules.pop("lib.databricks_utils", None)
        self.databricks_utils = importlib.import_module("lib.databricks_utils")

    def tearDown(self) -> None:
        sys.modules.pop("lib.databricks_utils", None)

    def _install_stub_modules(self) -> None:
        databricks_module = types.ModuleType("databricks")
        sdk_module = types.ModuleType("databricks.sdk")
        service_module = types.ModuleType("databricks.sdk.service")
        iam_module = types.ModuleType("databricks.sdk.service.iam")
        catalog_module = types.ModuleType("databricks.sdk.service.catalog")
        workspace_module = types.ModuleType("databricks.sdk.service.workspace")

        class WorkspaceClient:  # pragma: no cover - simple stub
            pass

        class ComplexValue:  # pragma: no cover - simple stub
            def __init__(self, **kwargs):
                self.kwargs = kwargs

        class Grant:  # pragma: no cover - simple stub
            def __init__(self, **kwargs):
                self.kwargs = kwargs

        class AzureKeyVaultSecretScopeMetadata:  # pragma: no cover - simple stub
            def __init__(self, **kwargs):
                self.kwargs = kwargs

        class ScopeBackendType:  # pragma: no cover - simple stub
            AZURE_KEYVAULT = "AZURE_KEYVAULT"

        sdk_module.WorkspaceClient = WorkspaceClient
        iam_module.ComplexValue = ComplexValue
        catalog_module.Grant = Grant
        workspace_module.AzureKeyVaultSecretScopeMetadata = AzureKeyVaultSecretScopeMetadata
        workspace_module.ScopeBackendType = ScopeBackendType
        service_module.iam = iam_module
        service_module.catalog = catalog_module
        service_module.workspace = workspace_module
        sdk_module.service = service_module
        databricks_module.sdk = sdk_module

        sys.modules["databricks"] = databricks_module
        sys.modules["databricks.sdk"] = sdk_module
        sys.modules["databricks.sdk.service"] = service_module
        sys.modules["databricks.sdk.service.iam"] = iam_module
        sys.modules["databricks.sdk.service.catalog"] = catalog_module
        sys.modules["databricks.sdk.service.workspace"] = workspace_module

        azkv_utils_module = types.ModuleType("lib.azkeyvault_utils")
        sys.modules["lib.azkeyvault_utils"] = azkv_utils_module

        constants_module = types.ModuleType("lib.constants")
        sys.modules["lib.constants"] = constants_module

    def test_synchronize_unity_catalog_permissions_applies_role_based_grants(self) -> None:
        class FakeGrantAPI:
            def __init__(self) -> None:
                self.calls = []

            def update(self, **kwargs):
                self.calls.append(kwargs)

        class FakeWorkspaceClient:
            def __init__(self) -> None:
                self.groups = types.SimpleNamespace(
                    list=lambda: [
                        types.SimpleNamespace(display_name="project_lead", id="lead-id"),
                        types.SimpleNamespace(display_name="admins", id="admin-id"),
                        types.SimpleNamespace(display_name="project_users", id="users-id"),
                    ]
                )
                self.grants = FakeGrantAPI()

        workspace_client = FakeWorkspaceClient()
        definition_json = {
            "Workspace": {
                "Acronym": "demo",
                "Users": [
                    {"Email": "owner@example.com", "Role": "Owner", "ObjectId": "owner-id"},
                    {"Email": "user@example.com", "Role": "User", "ObjectId": "user-id"},
                ],
            }
        }

        self.databricks_utils.synchronize_unity_catalog_permissions(definition_json, workspace_client)

        self.assertGreaterEqual(len(workspace_client.grants.calls), 2)
        principals = set()
        for call in workspace_client.grants.calls:
            changes = call["changes"]
            self.assertTrue(changes)
            for change in changes:
                if isinstance(change, dict):
                    principals.add(change["principal"])
                    self.assertIn("privileges", change)
                else:
                    principals.add(change.kwargs["principal"])
                    self.assertIn("privileges", change.kwargs)
        self.assertIn("project_lead", principals)
        self.assertIn("project_users", principals)


if __name__ == "__main__":
    unittest.main()
