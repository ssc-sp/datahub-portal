import importlib
import os
import sys
import types
import unittest
from pathlib import Path
from unittest import mock


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
                self.principal = kwargs.get("principal")
                self.privileges = kwargs.get("privileges")

            def as_dict(self):
                return self.kwargs

        class Privilege:  # pragma: no cover - simple stub
            ALL_PRIVILEGES = "ALL_PRIVILEGES"
            DATA_EDITOR = "DATA_EDITOR"
            DATA_READER = "DATA_READER"

        class AzureKeyVaultSecretScopeMetadata:  # pragma: no cover - simple stub
            def __init__(self, **kwargs):
                self.kwargs = kwargs

        class ScopeBackendType:  # pragma: no cover - simple stub
            AZURE_KEYVAULT = "AZURE_KEYVAULT"

        sdk_module.WorkspaceClient = WorkspaceClient
        iam_module.ComplexValue = ComplexValue
        catalog_module.Grant = Grant
        catalog_module.Privilege = Privilege
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

    def test_get_workspace_client_uses_azure_databricks_host(self) -> None:
        captured = {}

        class CapturingWorkspaceClient:
            def __init__(self, **kwargs):
                captured.update(kwargs)

        original_workspace_client = self.databricks_utils.WorkspaceClient
        self.databricks_utils.WorkspaceClient = CapturingWorkspaceClient

        try:
            with mock.patch.dict(
                os.environ,
                {
                    "AzureClientSecret": "secret",
                    "AzureClientId": "client-id",
                    "AzureTenantId": "tenant-id",
                },
                clear=False,
            ):
                self.databricks_utils.get_workspace_client("https://adb-123.azuredatabricks.net")
        finally:
            self.databricks_utils.WorkspaceClient = original_workspace_client

        self.assertEqual(captured["host"], "https://adb-123.azuredatabricks.net")
        self.assertNotIn("azure_environment", captured)
        self.assertEqual(captured["auth_type"], "azure-client-secret")

    def test_get_workspace_client_rejects_non_azure_host(self) -> None:
        with mock.patch.dict(
            os.environ,
            {
                "AzureClientSecret": "secret",
                "AzureClientId": "client-id",
                "AzureTenantId": "tenant-id",
            },
            clear=False,
        ):
            with self.assertRaisesRegex(ValueError, "Azure Databricks workspace host"):
                self.databricks_utils.get_workspace_client("https://adb-123.cloud.databricks.com")

    def test_synchronize_unity_catalog_permissions_applies_user_email_based_grants(self) -> None:
        class FakeGrantAPI:
            def __init__(self) -> None:
                self.calls = []

            def update(self, **kwargs):
                self.calls.append(kwargs)

        class FakeWorkspaceClient:
            def __init__(self) -> None:
                self.groups = types.SimpleNamespace(list=lambda: [])
                self.users = types.SimpleNamespace(
                    list=lambda: [
                        types.SimpleNamespace(
                            external_id="owner-id",
                            user_name="owner@example.com",
                            emails=[types.SimpleNamespace(value="owner@example.com")],
                        ),
                        types.SimpleNamespace(
                            external_id="user-id",
                            user_name="user@example.com",
                            emails=[types.SimpleNamespace(value="user@example.com")],
                        ),
                    ]
                )
                self.grants = FakeGrantAPI()

        workspace_client = FakeWorkspaceClient()
        definition_json = {
            "AppData": {"DatabricksCatalogName": "demo_catalog"},
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
        self.assertIn("owner@example.com", principals)
        self.assertIn("user@example.com", principals)

    def test_unity_catalog_role_presets_are_mapped_as_requested(self) -> None:
        owner_preset, owner_privileges = self.databricks_utils._resolve_unity_catalog_privileges_for_role("Owner")
        guest_preset, guest_privileges = self.databricks_utils._resolve_unity_catalog_privileges_for_role("Guest")
        admin_preset, admin_privileges = self.databricks_utils._resolve_unity_catalog_privileges_for_role("Admin")

        self.assertEqual("ALL_PRIVILEGES", owner_preset)
        self.assertEqual(["ALL_PRIVILEGES"], owner_privileges["catalog"])
        self.assertEqual(["ALL_PRIVILEGES"], owner_privileges["schema"])

        self.assertEqual("DATA_READER", guest_preset)
        self.assertEqual(["DATA_READER"], guest_privileges["catalog"])
        self.assertEqual(["DATA_READER"], guest_privileges["schema"])

        self.assertEqual("DATA_EDITOR", admin_preset)
        self.assertEqual(["DATA_EDITOR"], admin_privileges["catalog"])
        self.assertEqual(["DATA_EDITOR"], admin_privileges["schema"])

    def test_synchronize_unity_catalog_permissions_uses_owner_and_guest_presets(self) -> None:
        class FakeGrantAPI:
            def __init__(self) -> None:
                self.calls = []

            def update(self, **kwargs):
                self.calls.append(kwargs)

        class FakeWorkspaceClient:
            def __init__(self) -> None:
                self.groups = types.SimpleNamespace(list=lambda: [])
                self.users = types.SimpleNamespace(
                    list=lambda: [
                        types.SimpleNamespace(
                            external_id="owner-id",
                            user_name="owner@example.com",
                            emails=[types.SimpleNamespace(value="owner@example.com")],
                        ),
                        types.SimpleNamespace(
                            external_id="guest-id",
                            user_name="guest@example.com",
                            emails=[types.SimpleNamespace(value="guest@example.com")],
                        ),
                    ]
                )
                self.grants = FakeGrantAPI()

        workspace_client = FakeWorkspaceClient()
        definition_json = {
            "AppData": {"DatabricksCatalogName": "demo_catalog"},
            "Workspace": {
                "Acronym": "demo",
                "Users": [
                    {"Email": "owner@example.com", "Role": "Owner", "ObjectId": "owner-id"},
                    {"Email": "guest@example.com", "Role": "Guest", "ObjectId": "guest-id"},
                ],
            }
        }

        self.databricks_utils.synchronize_unity_catalog_permissions(definition_json, workspace_client)

        grants_by_principal_and_type = {}
        for call in workspace_client.grants.calls:
            changes = call["changes"]
            self.assertEqual(1, len(changes))
            change = changes[0]
            principal = change.kwargs["principal"] if hasattr(change, "kwargs") else change["principal"]
            privileges = change.kwargs["privileges"] if hasattr(change, "kwargs") else change["privileges"]
            grants_by_principal_and_type[(principal, call["securable_type"])] = privileges

        self.assertEqual(["ALL_PRIVILEGES"], grants_by_principal_and_type[("owner@example.com", "catalog")])
        self.assertEqual(["DATA_READER"], grants_by_principal_and_type[("guest@example.com", "catalog")])
        self.assertEqual(2, len(grants_by_principal_and_type))

    def test_synchronize_unity_catalog_permissions_raises_when_preset_missing_in_api(self) -> None:
        class FakeGrantAPI:
            def __init__(self) -> None:
                self.calls = []

            def update(self, **kwargs):
                self.calls.append(kwargs)

        class FakeWorkspaceClient:
            def __init__(self) -> None:
                self.groups = types.SimpleNamespace(list=lambda: [])
                self.users = types.SimpleNamespace(list=lambda: [])
                self.grants = FakeGrantAPI()

        workspace_client = FakeWorkspaceClient()
        definition_json = {
            "AppData": {"DatabricksCatalogName": "demo_catalog"},
            "Workspace": {
                "Acronym": "demo",
                "Users": [
                    {"Email": "guest@example.com", "Role": "Guest", "ObjectId": "guest-id"},
                ],
            }
        }

        original_get_supported = self.databricks_utils._get_unity_catalog_api_supported_privileges
        self.databricks_utils._get_unity_catalog_api_supported_privileges = lambda: {"ALL_PRIVILEGES", "DATA_EDITOR"}
        try:
            with self.assertRaisesRegex(ValueError, "DATA_READER"):
                self.databricks_utils.synchronize_unity_catalog_permissions(definition_json, workspace_client)
        finally:
            self.databricks_utils._get_unity_catalog_api_supported_privileges = original_get_supported

    def test_synchronize_unity_catalog_permissions_uses_admin_user_email(self) -> None:
        class FakeGrantAPI:
            def __init__(self) -> None:
                self.calls = []

            def update(self, **kwargs):
                self.calls.append(kwargs)

        class FakeWorkspaceClient:
            def __init__(self) -> None:
                self.groups = types.SimpleNamespace(list=lambda: [])
                self.users = types.SimpleNamespace(
                    list=lambda: [
                        types.SimpleNamespace(
                            external_id="admin-id",
                            user_name="admin@example.com",
                            emails=[types.SimpleNamespace(value="admin@example.com")],
                        )
                    ]
                )
                self.grants = FakeGrantAPI()

        workspace_client = FakeWorkspaceClient()
        definition_json = {
            "AppData": {"DatabricksCatalogName": "demo_catalog"},
            "Workspace": {
                "Acronym": "demo",
                "Users": [
                    {"Email": "admin@example.com", "Role": "Admin", "ObjectId": "admin-id"},
                ],
            }
        }

        self.databricks_utils.synchronize_unity_catalog_permissions(definition_json, workspace_client)

        self.assertGreaterEqual(len(workspace_client.grants.calls), 1)
        principals = set()
        for call in workspace_client.grants.calls:
            for change in call["changes"]:
                if isinstance(change, dict):
                    principals.add(change["principal"])
                else:
                    principals.add(change.kwargs["principal"])

        self.assertIn("admin@example.com", principals)

    def test_synchronize_unity_catalog_permissions_falls_back_to_definition_email_when_workspace_user_missing(self) -> None:
        class FakeGrantAPI:
            def __init__(self) -> None:
                self.calls = []

            def update(self, **kwargs):
                self.calls.append(kwargs)

        class FakeWorkspaceClient:
            def __init__(self) -> None:
                self.groups = types.SimpleNamespace(list=lambda: [])
                self.users = types.SimpleNamespace(list=lambda: [])
                self.grants = FakeGrantAPI()

        workspace_client = FakeWorkspaceClient()
        definition_json = {
            "AppData": {"DatabricksCatalogName": "demo_catalog"},
            "Workspace": {
                "Acronym": "demo",
                "Users": [
                    {"Email": "admin@example.com", "Role": "Admin", "ObjectId": "admin-id"},
                ],
            }
        }

        self.databricks_utils.synchronize_unity_catalog_permissions(definition_json, workspace_client)

        self.assertGreaterEqual(len(workspace_client.grants.calls), 1)
        principals = set()
        for call in workspace_client.grants.calls:
            for change in call["changes"]:
                if isinstance(change, dict):
                    principals.add(change["principal"])
                else:
                    principals.add(change.kwargs["principal"])

        self.assertIn("admin@example.com", principals)

    def test_synchronize_unity_catalog_permissions_raises_when_catalog_unresolved(self) -> None:
        class FakeGrantAPI:
            def __init__(self) -> None:
                self.calls = []

            def update(self, **kwargs):
                self.calls.append(kwargs)

        class FakeWorkspaceClient:
            def __init__(self) -> None:
                self.groups = types.SimpleNamespace(list=lambda: [])
                self.users = types.SimpleNamespace(list=lambda: [])
                self.grants = FakeGrantAPI()

        workspace_client = FakeWorkspaceClient()
        definition_json = {
            "Workspace": {
                "Acronym": "demo",
                "Users": [
                    {"Email": "admin@example.com", "Role": "Admin", "ObjectId": "admin-id"},
                ],
            },
            "AppData": {},
        }

        with self.assertRaisesRegex(ValueError, "Unable to resolve Unity Catalog name"):
            self.databricks_utils.synchronize_unity_catalog_permissions(definition_json, workspace_client)

        self.assertEqual([], workspace_client.grants.calls)

    def test_synchronize_workspace_users_reuses_existing_user_by_email_case_insensitive(self) -> None:
        class FakeWorkspaceUser:
            def __init__(self, user_name, external_id, groups=None):
                self.user_name = user_name
                self.external_id = external_id
                self.groups = groups
                self.id = "workspace-user-1"
                self.emails = [types.SimpleNamespace(value=user_name)]

        class FakeUsersAPI:
            def __init__(self):
                self.created = []
                self.deleted = []

            def list(self):
                return [FakeWorkspaceUser("Erik.Putrycz@SSC-SPC.GC.CA", "different-external-id", groups=[])]

            def create(self, **kwargs):
                self.created.append(kwargs)
                return kwargs

            def update(self, **kwargs):
                return kwargs

        class FakeWorkspaceClient:
            def __init__(self):
                self.users = FakeUsersAPI()
                self.groups = types.SimpleNamespace(
                    list=lambda: [
                        types.SimpleNamespace(display_name="project_lead", id="lead-id"),
                        types.SimpleNamespace(display_name="admins", id="admin-id"),
                        types.SimpleNamespace(display_name="project_users", id="users-id"),
                    ]
                )

        definition_json = {
            "Workspace": {
                "Users": [
                    {"Email": "erik.putrycz@ssc-spc.gc.ca", "Role": "Owner", "ObjectId": "owner-id"}
                ]
            }
        }

        workspace_client = FakeWorkspaceClient()
        original_create = self.databricks_utils.create_new_user_in_workspace
        self.databricks_utils.create_new_user_in_workspace = lambda *_args, **_kwargs: (_ for _ in ()).throw(AssertionError("user should not be created when same email exists"))

        try:
            self.databricks_utils.synchronize_workspace_users(definition_json, workspace_client)
        finally:
            self.databricks_utils.create_new_user_in_workspace = original_create

        self.assertEqual([], workspace_client.users.created)

    def test_apply_unity_catalog_grant_uses_sdk_grant_objects(self) -> None:
        class FakeGrantAPI:
            def __init__(self):
                self.calls = []

            def update(self, **kwargs):
                self.calls.append(kwargs)

        class FakeWorkspaceClient:
            def __init__(self):
                self.grants = FakeGrantAPI()

        workspace_client = FakeWorkspaceClient()
        self.databricks_utils.apply_unity_catalog_grant(
            workspace_client,
            "catalog",
            "demo",
            "project_lead",
            ["USE_CATALOG"],
        )

        self.assertEqual(1, len(workspace_client.grants.calls))
        self.assertEqual(1, len(workspace_client.grants.calls[0]["changes"]))
        self.assertTrue(hasattr(workspace_client.grants.calls[0]["changes"][0], "principal"))
        self.assertEqual("project_lead", workspace_client.grants.calls[0]["changes"][0].principal)

    def test_get_unity_catalog_targets_uses_current_metastore(self) -> None:
        class FakeWorkspaceClient:
            def __init__(self):
                self.metastores = types.SimpleNamespace(
                    current=lambda: types.SimpleNamespace(default_catalog_name="workspace-metastore")
                )

        definition_json = {"Workspace": {"Acronym": "demo"}}

        catalog_name, schema_name = self.databricks_utils.get_unity_catalog_targets(
            definition_json,
            FakeWorkspaceClient(),
        )

        self.assertEqual("workspace-metastore", catalog_name)
        self.assertEqual("default", schema_name)

    def test_get_unity_catalog_targets_prefers_explicit_catalog_override(self) -> None:
        class FakeWorkspaceClient:
            def __init__(self):
                self.metastores = types.SimpleNamespace(
                    current=lambda: types.SimpleNamespace(name="workspace-metastore")
                )

        definition_json = {
            "Workspace": {"Acronym": "demo"},
            "AppData": {"DatabricksCatalogName": "explicit-catalog"},
        }

        catalog_name, _ = self.databricks_utils.get_unity_catalog_targets(
            definition_json,
            FakeWorkspaceClient(),
        )

        self.assertEqual("explicit-catalog", catalog_name)

    def test_get_unity_catalog_targets_ignores_non_default_catalog_fields(self) -> None:
        class FakeWorkspaceClient:
            def __init__(self):
                self.metastores = types.SimpleNamespace(
                    current=lambda: types.SimpleNamespace(
                        metastore_id="meta-123",
                        default_catalog_name=None,
                        catalog_name="legacy-catalog-name",
                        name="legacy-metastore-name",
                    ),
                )

        definition_json = {"Workspace": {"Acronym": "demo"}}

        with self.assertRaisesRegex(ValueError, "Unable to resolve Unity Catalog name"):
            self.databricks_utils.get_unity_catalog_targets(definition_json, FakeWorkspaceClient())

    def test_get_unity_catalog_targets_raises_when_unresolved(self) -> None:
        class FakeWorkspaceClient:
            def __init__(self):
                self.metastores = types.SimpleNamespace(
                    current=lambda: types.SimpleNamespace(
                        metastore_id="meta-123",
                        default_catalog_name=None,
                        catalog_name=None,
                        name=None,
                    ),
                    get=lambda _metastore_id: types.SimpleNamespace(
                        name=None,
                        default_catalog_name=None,
                        catalog_name=None,
                    ),
                )

        definition_json = {"Workspace": {"Acronym": "demo"}}

        with self.assertRaisesRegex(ValueError, "Unable to resolve Unity Catalog name"):
            self.databricks_utils.get_unity_catalog_targets(definition_json, FakeWorkspaceClient())

    def test_create_new_user_in_workspace_skips_missing_group(self) -> None:
        class FakeUsersAPI:
            def __init__(self):
                self.created = []

            def create(self, **kwargs):
                self.created.append(kwargs)
                return kwargs

        class FakeWorkspaceClient:
            def __init__(self):
                self.users = FakeUsersAPI()
                self.groups = types.SimpleNamespace(
                    list=lambda: [
                        types.SimpleNamespace(display_name="project_lead", id="lead-id"),
                        types.SimpleNamespace(display_name="project_users", id="users-id"),
                    ]
                )

        workspace_client = FakeWorkspaceClient()
        user = {"Email": "admin@example.com", "Role": "Admin", "ObjectId": "admin-id"}

        result = self.databricks_utils.create_new_user_in_workspace(workspace_client, user)

        self.assertIsNotNone(result)
        self.assertEqual("admin@example.com", result["user_name"])
        self.assertNotIn("groups", result)


if __name__ == "__main__":
    unittest.main()
