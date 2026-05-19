# SystemTokenCredentialService – Credential Use Cases

This document tracks all usages of `ISystemTokenCredentialService` across the codebase and describes the use case for each credential.

---

## Overview

`ISystemTokenCredentialService` exposes two `Azure.Core.TokenCredential` instances:

| Method | Identity Used | Auth Mechanism |
|---|---|---|
| `GetPortalTokenCredential()` | Portal app identity (`AzureAd.ClientId`) | `ManagedIdentityCredential` (when `PortalRunAsManagedIdentity=enabled`) or `ClientSecretCredential` |
| `GetInfraTokenCredential()` | Infrastructure service principal (`AzureAd.InfraClientId`) | `ClientSecretCredential` only (managed identity path is commented out) |

---

## `GetPortalTokenCredential()`

Used wherever the portal's own identity needs to authenticate to Azure services.

### 1. Key Vault – Secret Client (`KeyVaultCoreService`)
- **File:** `Portal/src/Datahub.Infrastructure/Services/Security/KeyVaultCoreService.cs`
- **Method:** `GetSecretClient()` → `new SecretClient(..., GetPortalTokenCredential())`
- **Use case:** Reading Azure Key Vault secrets on behalf of the portal (e.g., `datahubportal-client-secret`, arbitrary named secrets via `GetSecret()`).

### 2. Key Vault – Key Client (`KeyVaultCoreService`)
- **File:** `Portal/src/Datahub.Infrastructure/Services/Security/KeyVaultCoreService.cs`
- **Method:** `GetKeyClient()` → `new KeyClient(..., GetPortalTokenCredential())`
- **Use case:** Retrieving RSA keys stored in Key Vault as part of API token operations.

### 3. Key Vault – Encrypt API Token (`KeyVaultCoreService`)
- **File:** `Portal/src/Datahub.Infrastructure/Services/Security/KeyVaultCoreService.cs`
- **Method:** `EncryptApiTokenAsync()` → `new CryptographyClient(key.Value.Id, GetPortalTokenCredential())`
- **Use case:** Encrypting an API token string using RSA-OAEP-256 with a Key Vault-managed key.

### 4. Key Vault – Decrypt API Token (`KeyVaultCoreService`)
- **File:** `Portal/src/Datahub.Infrastructure/Services/Security/KeyVaultCoreService.cs`
- **Method:** `DecryptApiTokenAsync()` → `new CryptographyClient(key.Value.Id, GetPortalTokenCredential())`
- **Use case:** Decrypting a previously encrypted API token string using RSA-OAEP-256 with a Key Vault-managed key.

### 5. Key Vault – User Secret/Key Client Fallback for External Users (`KeyVaultUserService`)
- **File:** `Portal/src/Datahub.Infrastructure/Services/Security/KeyVaultUserService.cs`
- **Methods:** `GetSecretClient(string kvName)`, `GetKeyClient(string kvName)`
- **Use case:** When the current user is an **external user** (or no user token is available), the portal credential is used as a fallback instead of the user's own token to access workspace Key Vault secrets and keys.

---

## `GetInfraTokenCredential()`

Used wherever the **infrastructure service principal** (separate from the portal identity) must authenticate to Azure Resource Manager or other infra-level Azure APIs.

### 1. ARM Client – Storage Shared Key Access Control (`WorkspaceSharedKeyAccessControl`)
- **File:** `Portal/src/Datahub.Portal/Pages/Workspace/Settings/WorkspaceSharedKeyAccessControl.razor.cs`
- **Method:** `BuildArmClient(...)` → `new ArmClient(GetInfraTokenCredential(), subscriptionId, armOptions)`
- **Use case:** Querying and toggling the `AllowSharedKeyAccess` setting on a workspace Azure Storage Account via Azure Resource Manager.

### 2. ARM Client – PostgreSQL Firewall IP Whitelist (`DatabaseIpWhitelistTable`)
- **File:** `Portal/src/Datahub.Portal/Pages/Workspace/Database/DatabaseIpWhitelistTable.razor.cs`
- **Method:** `BuildPostgresSqlFlexibleServerResource()` → `new ArmClient(GetInfraTokenCredential())`
- **Use case:** Managing IP firewall rules (whitelist entries) on the workspace's Azure Database for PostgreSQL Flexible Server.

### 3. ARM Client – Workspace Database Page (`WorkspaceDatabasePage`)
- **File:** `Portal/src/Datahub.Portal/Pages/Workspace/Database/WorkspaceDatabasePage.razor.cs`
- **Method:** `BuildPostgresSqlFlexibleServerResource()` → `new ArmClient(GetInfraTokenCredential())`
- **Use case:** Fetching PostgreSQL Flexible Server resource details (connection info, status) for display and management on the workspace database management page.

---

## Summary Table

| Credential | Consumer Class | Azure Service | Operation |
|---|---|---|---|
| `GetPortalTokenCredential()` | `KeyVaultCoreService` | Azure Key Vault (Secrets) | Read secrets (e.g., client secret, named secrets) |
| `GetPortalTokenCredential()` | `KeyVaultCoreService` | Azure Key Vault (Keys) | Retrieve RSA keys |
| `GetPortalTokenCredential()` | `KeyVaultCoreService` | Azure Key Vault (Cryptography) | Encrypt API tokens (RSA-OAEP-256) |
| `GetPortalTokenCredential()` | `KeyVaultCoreService` | Azure Key Vault (Cryptography) | Decrypt API tokens (RSA-OAEP-256) |
| `GetPortalTokenCredential()` | `KeyVaultUserService` | Azure Key Vault (Secrets & Keys) | Fallback credential for external/unauthenticated users accessing workspace KV |
| `GetInfraTokenCredential()` | `WorkspaceSharedKeyAccessControl` | Azure Resource Manager (Storage) | Read/update Storage `AllowSharedKeyAccess` setting |
| `GetInfraTokenCredential()` | `DatabaseIpWhitelistTable` | Azure Resource Manager (PostgreSQL) | Manage PostgreSQL firewall IP whitelist rules |
| `GetInfraTokenCredential()` | `WorkspaceDatabasePage` | Azure Resource Manager (PostgreSQL) | Fetch workspace PostgreSQL server details |

---

## Notes

- **`GetPortalTokenCredential()`** supports both **System-Assigned Managed Identity** (production) and **Client Secret** (development/non-MI environments), controlled by the `PortalRunAsManagedIdentity` configuration flag.
- **`GetInfraTokenCredential()`** currently **always uses `ClientSecretCredential`** with `InfraClientId`/`InfraClientSecret`. The managed identity path exists in the code but is commented out.
- All ARM Client usages of `GetInfraTokenCredential()` are in Blazor component code-behind files (`.razor.cs`) and are triggered by user interactions in the portal UI.
