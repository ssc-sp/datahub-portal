# Infra Request: Storage Queues for ClamAV Scan Results

Please provision the Storage Queue resources needed by the ClamAV scan completion flow.

## Queue names

- `clamav-scan-result` - completion message consumed by `VirusScanNotificationHandler`
- `virus-scan-user-status` - downstream user-status processing queue
- `email-notification` - email delivery queue consumed by `EmailNotificationHandler`

## Connection string key

- `DatahubStorageQueue:ConnectionString`

## Message shape

The queue message may be either a direct JSON payload or wrapped in a `message` envelope.

### `VirusScanNotificationMessage` for `clamav-scan-result`

```json
{
  "message": {
    "workspaceAcronym": "ABC1",
    "userObjectId": "<aad-oid>",
    "fileName": "report.pdf",
    "blobPath": "container/path/report.pdf",
    "scanStatus": "Clean",
    "scanCompletedOn": "2024-01-01T12:00:00Z",
    "fileSizeBytes": 102400,
    "storageAccountName": "workspace-storage",
    "containerName": "datahub",
    "correlationId": "<optional-correlation-id>"
  }
}
```

### `EmailRequestMessage` for `email-notification`

```json
{
  "message": {
    "to": ["recipient@example.com"],
    "subject": "Virus scan completed for report.pdf",
    "body": "<p>...</p>"
  }
}
```

### `VirusScanUserStatusMessage` for `virus-scan-user-status`

```json
{
  "message": {
    "workspaceAcronym": "ABC1",
    "uploaderObjectId": "<aad-oid>",
    "uploaderEmail": "user@example.com",
    "uploaderName": "User Name",
    "fileName": "report.pdf",
    "blobPath": "container/path/report.pdf",
    "scanStatus": "Clean",
    "scanCompletedOn": "2024-01-01T12:00:00Z",
    "fileSizeBytes": 102400,
    "fileHashSha256": "...",
    "storageAccountName": "workspace-storage",
    "containerName": "datahub",
    "scanEngine": "ClamAV",
    "correlationId": "<optional-correlation-id>",
    "aclsApplied": true,
    "metadata": {
      "key": "value"
    }
  }
}
```

## Done criteria

1. Queue `clamav-scan-result` exists in the target storage account.
2. Queue `email-notification` exists in the target storage account.
3. Queue `virus-scan-user-status` exists in the target storage account.
4. Datahub function app has `DatahubStorageQueue:ConnectionString` configured.
5. A test message in `clamav-scan-result` is consumed successfully by `VirusScanNotificationHandler`.
