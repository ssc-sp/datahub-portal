import importlib
import sys
import types
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


class RemoveRoleAssignmentTests(unittest.TestCase):
    def setUp(self):
        self._install_stub_modules()
        sys.path.insert(0, str(ROOT))
        sys.modules.pop("lib.azstorage_utils", None)

    def tearDown(self):
        sys.modules.pop("lib.azstorage_utils", None)

    def _install_stub_modules(self):
        azure_module = types.ModuleType("azure")
        identity_module = types.ModuleType("azure.identity")

        class ClientSecretCredential:  # pragma: no cover - test stub
            pass

        identity_module.ClientSecretCredential = ClientSecretCredential

        auth_module = types.ModuleType("azure.mgmt.authorization")
        models_module = types.ModuleType("azure.mgmt.authorization.models")

        class AuthorizationManagementClient:  # pragma: no cover - test stub
            pass

        class RoleAssignmentCreateParameters:  # pragma: no cover - test stub
            def __init__(self, **kwargs):
                self.__dict__.update(kwargs)

        auth_module.AuthorizationManagementClient = AuthorizationManagementClient
        models_module.RoleAssignmentCreateParameters = RoleAssignmentCreateParameters

        lib_module = types.ModuleType("lib")
        lib_module.__path__ = [str(ROOT / "lib")]

        constants_module = types.ModuleType("lib.constants")
        constants_module.RESOURCE_PREFIX = "example"

        sys.modules.update({
            "azure": azure_module,
            "azure.identity": identity_module,
            "azure.mgmt": types.ModuleType("azure.mgmt"),
            "azure.mgmt.authorization": auth_module,
            "azure.mgmt.authorization.models": models_module,
            "lib": lib_module,
            "lib.constants": constants_module,
        })

    def test_remove_existing_role_deletes_matching_managed_blob_assignment(self):
        module = importlib.import_module("lib.azstorage_utils")

        class FakeRoleAssignment:
            def __init__(self, name, principal_id, role_definition_id):
                self.name = name
                self.principal_id = principal_id
                self.role_definition_id = role_definition_id

        class FakeRoleAssignments:
            def __init__(self):
                self.deleted = []

            def list_for_scope(self, scope):
                return [
                    FakeRoleAssignment(
                        name="assignment-1",
                        principal_id="user-123",
                        role_definition_id="/subscriptions/sub-id/providers/Microsoft.Authorization/roleDefinitions/ba92f5b4-2d11-453d-a403-e96b0029c9fe",
                    )
                ]

            def delete(self, scope, role_assignment_name):
                self.deleted.append((scope, role_assignment_name))

        class FakeClient:
            def __init__(self):
                self.role_assignments = FakeRoleAssignments()

        client = FakeClient()
        module.remove_existing_role(
            client,
            "/subscriptions/sub-id/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/account",
            "user-123",
        )

        self.assertEqual(
            client.role_assignments.deleted,
            [("/subscriptions/sub-id/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/account", "assignment-1")],
        )


if __name__ == "__main__":
    unittest.main()
