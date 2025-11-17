# Azure Function ACL Integration Guide

This guide explains how to integrate Azure Functions with the Datahub Portal ACL API for automatic file quarantine during AV scanning.

## Overview

When files are uploaded to Azure Data Lake Storage Gen2, they should be:
1. **Locked immediately** - Remove all user ACLs to prevent access
2. **Scanned for viruses** - Azure Function runs AV scan
3. **Restored on success** - Apply workspace member ACLs and clear quarantine metadata

The Portal provides two API endpoints that Azure Functions can call to manage file access during this workflow.

## API Endpoints

### Base URL
```
https://your-portal-url.azurewebsites.net/api/storage/acl
```

### 1. Lock File (POST /lock-file)
Called when a file is uploaded to quarantine it during scanning.

**Endpoint:** `POST /api/storage/acl/lock-file`

**Headers:**
```
Content-Type: application/json
X-Api-Key: your-secret-api-key
```

**Request Body:**
```json
{
  "workspaceAcronym": "PROJ123",
  "filePath": "/upload/document.pdf"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "updatedCount": 1,
  "message": "Successfully locked file: /upload/document.pdf"
}
```

### 2. Restore Access (POST /restore-access)
Called after successful AV scan to restore user access.

**Endpoint:** `POST /api/storage/acl/restore-access`

**Headers:**
```
Content-Type: application/json
X-Api-Key: your-secret-api-key
```

**Request Body:**
```json
{
  "workspaceAcronym": "PROJ123",
  "filePath": "/upload/document.pdf"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "updatedCount": 1,
  "message": "Successfully restored access to file: /upload/document.pdf"
}
```

**Important:** Both endpoints require the `filePath` parameter. They operate on a single file only (non-recursive).

## Configuration

### Portal Configuration
Add the API key to `appsettings.json`:

```json
{
  "AclFunction": {
    "ApiKey": "CHANGE-THIS-TO-SECURE-RANDOM-KEY-IN-PRODUCTION"
  }
}
```

**Important:** Use a strong, randomly generated key in production. Example generation:
```powershell
# PowerShell
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

### Azure Function Configuration
Add these application settings to your Function App:

| Setting Name | Value | Description |
|-------------|-------|-------------|
| `PORTAL_ACL_API_URL` | `https://your-portal-url.azurewebsites.net/api/storage/acl` | Base URL for ACL API |
| `PORTAL_ACL_API_KEY` | `your-secret-api-key` | Must match Portal's AclFunction:ApiKey |

## Azure Function Implementation

### Trigger: Blob Created Event
Listen for Event Grid events when files are uploaded to storage.

```csharp
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DatahubAvScan;

public class BlobUploadFunction
{
    private readonly ILogger<BlobUploadFunction> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _aclApiUrl;
    private readonly string _apiKey;

    public BlobUploadFunction(
        ILogger<BlobUploadFunction> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        
        // Load from environment variables
        _aclApiUrl = Environment.GetEnvironmentVariable("PORTAL_ACL_API_URL") 
            ?? throw new InvalidOperationException("PORTAL_ACL_API_URL not configured");
        _apiKey = Environment.GetEnvironmentVariable("PORTAL_ACL_API_KEY")
            ?? throw new InvalidOperationException("PORTAL_ACL_API_KEY not configured");
    }

    [Function("OnBlobCreated")]
    public async Task Run(
        [EventGridTrigger] EventGridEvent eventGridEvent)
    {
        try
        {
            _logger.LogInformation("Blob created: {Subject}", eventGridEvent.Subject);

            // Parse Event Grid event
            var blobUrl = eventGridEvent.Subject;
            var (workspaceAcronym, filePath) = ParseBlobUrl(blobUrl);

            if (string.IsNullOrEmpty(workspaceAcronym))
            {
                _logger.LogWarning("Could not extract workspace from blob URL: {Url}", blobUrl);
                return;
            }

            // Step 1: Lock the file immediately
            await LockFileAsync(workspaceAcronym, filePath);

            // Step 2: Perform AV scan (your AV scanner logic here)
            var scanResult = await ScanFileAsync(blobUrl);

            // Step 3: Restore access if scan passed
            if (scanResult.IsClean)
            {
                await RestoreAccessAsync(workspaceAcronym, filePath);
                _logger.LogInformation("File {Path} passed scan and access restored", filePath);
            }
            else
            {
                _logger.LogWarning("File {Path} failed scan: {Reason}", filePath, scanResult.Reason);
                // File remains locked - admin must manually review
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing blob upload event");
            throw;
        }
    }

    private async Task LockFileAsync(string workspaceAcronym, string filePath)
    {
        var request = new
        {
            workspaceAcronym = workspaceAcronym,
            filePath = filePath
        };

        var requestJson = JsonSerializer.Serialize(request);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_aclApiUrl}/lock-file")
        {
            Content = content
        };
        httpRequest.Headers.Add("X-Api-Key", _apiKey);

        var response = await _httpClient.SendAsync(httpRequest);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to lock file: {response.StatusCode} - {errorBody}");
        }

        _logger.LogInformation("Successfully locked file: {Path}", filePath);
    }

    private async Task RestoreAccessAsync(string workspaceAcronym, string filePath)
    {
        var request = new
        {
            workspaceAcronym = workspaceAcronym,
            filePath = filePath
        };

        var requestJson = JsonSerializer.Serialize(request);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_aclApiUrl}/restore-access")
        {
            Content = content
        };
        httpRequest.Headers.Add("X-Api-Key", _apiKey);

        var response = await _httpClient.SendAsync(httpRequest);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to restore access: {response.StatusCode} - {errorBody}");
        }

        _logger.LogInformation("Successfully restored access: {Path}", filePath);
    }

    private (string workspaceAcronym, string filePath) ParseBlobUrl(string blobUrl)
    {
        // Example: /blobServices/default/containers/proj123/blobs/upload/document.pdf
        // Extract workspace acronym from container name and file path
        
        var parts = blobUrl.Split('/');
        
        // Find container name (after "containers")
        var containerIndex = Array.IndexOf(parts, "containers");
        if (containerIndex == -1 || containerIndex + 1 >= parts.Length)
        {
            return (string.Empty, string.Empty);
        }

        var workspaceAcronym = parts[containerIndex + 1].ToUpperInvariant();

        // Get file path (after "blobs")
        var blobsIndex = Array.IndexOf(parts, "blobs");
        if (blobsIndex == -1 || blobsIndex + 1 >= parts.Length)
        {
            return (workspaceAcronym, string.Empty);
        }

        var filePath = "/" + string.Join("/", parts[(blobsIndex + 1)..]);

        return (workspaceAcronym, filePath);
    }

    private async Task<ScanResult> ScanFileAsync(string blobUrl)
    {
        // TODO: Implement your AV scanner logic here
        // This is a placeholder
        await Task.Delay(100);
        
        return new ScanResult
        {
            IsClean = true,
            Reason = "No threats detected"
        };
    }

    private class ScanResult
    {
        public bool IsClean { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}

public class EventGridEvent
{
    public string Subject { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime EventTime { get; set; }
    public object Data { get; set; } = new();
}
```

## Event Grid Setup

Configure Event Grid subscription to trigger the Azure Function:

1. **Event Types to Subscribe:**
   - `Microsoft.Storage.BlobCreated`

2. **Filter Configuration:**
   ```json
   {
     "subjectBeginsWith": "/blobServices/default/containers/",
     "subjectEndsWith": "",
     "includedEventTypes": ["Microsoft.Storage.BlobCreated"]
   }
   ```

3. **Delivery Properties:**
   - Event Schema: Event Grid Schema
   - Endpoint Type: Azure Function
   - Max Events Per Batch: 1 (for immediate processing)

## Security Considerations

### API Key Security
- **Never commit API keys to source control**
- Use Azure Key Vault to store API keys
- Rotate keys regularly (every 90 days recommended)
- Use different keys for dev/test/production environments

### Network Security
- Consider using Private Endpoints for Portal API
- Restrict Function App outbound traffic to Portal URL only
- Enable HTTPS only for all endpoints

### Monitoring
Monitor these metrics:
- Failed API calls (401 Unauthorized)
- Failed ACL operations (500 errors)
- Average file lock/restore times
- Files stuck in locked state

## Troubleshooting

### Issue: 401 Unauthorized
**Cause:** API key mismatch or missing header

**Solution:**
1. Verify `PORTAL_ACL_API_KEY` in Function App settings matches Portal's `AclFunction:ApiKey`
2. Check `X-Api-Key` header is being sent in requests
3. Review Portal logs for "Invalid API key provided" warnings

### Issue: 400 Bad Request - Workspace acronym or file path required
**Cause:** Missing required parameters in request body

**Solution:**
1. Verify both `workspaceAcronym` and `filePath` are provided in request
2. Check `ParseBlobUrl()` logic extracts workspace and file path correctly
3. Log full event subject and parsed values for debugging

### Issue: Files remain locked after scan
**Cause:** Function failed to call restore-access endpoint

**Solution:**
1. Check Function logs for exceptions
2. Verify network connectivity to Portal API
3. Implement retry logic with exponential backoff
4. Add dead letter queue for failed events

## Testing

### Local Testing with curl

**Lock File:**
```bash
curl -X POST https://your-portal-url.azurewebsites.net/api/storage/acl/lock-file \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: your-secret-api-key" \
  -d '{
    "workspaceAcronym": "PROJ123",
    "filePath": "/upload/test.pdf"
  }'
```

**Restore Access:**
```bash
curl -X POST https://your-portal-url.azurewebsites.net/api/storage/acl/restore-access \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: your-secret-api-key" \
  -d '{
    "workspaceAcronym": "PROJ123",
    "filePath": "/upload/test.pdf"
  }'
```

### Portal Admin UI Testing
Use the Storage Administration page (`/admin/storage`) to test ACL operations:
1. Click "Lock Files in Upload Folder" - simulates file upload quarantine
2. Click "Simulate Scan Success" - simulates successful AV scan and restore

## Additional Resources

- [Azure Event Grid Documentation](https://learn.microsoft.com/azure/event-grid/)
- [Azure Functions HTTP Trigger](https://learn.microsoft.com/azure/azure-functions/functions-bindings-http-webhook-trigger)
- [Azure Data Lake Storage Gen2 ACLs](https://learn.microsoft.com/azure/storage/blobs/data-lake-storage-access-control)
