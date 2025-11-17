using Datahub.Application.Services.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Datahub.Portal.Controllers;

/// <summary>
/// ACL management API for Azure Functions (AV scan integration)
/// Requires X-Api-Key header for authentication
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/storage/acl")]
public class StorageAclController : ControllerBase
{
    private readonly ILogger<StorageAclController> _logger;
    private readonly IWorkspaceAclService _aclService;
    private readonly IConfiguration _configuration;

    public StorageAclController(
        ILogger<StorageAclController> logger,
        IWorkspaceAclService aclService,
        IConfiguration configuration)
    {
        _logger = logger;
        _aclService = aclService;
        _configuration = configuration;
    }

    /// <summary>
    /// Validates API key from request header
    /// </summary>
    private bool ValidateApiKey()
    {
        var expectedApiKey = _configuration["AclFunction:ApiKey"];
        
        if (string.IsNullOrWhiteSpace(expectedApiKey))
        {
            _logger.LogWarning("API key not configured in appsettings");
            return false;
        }

        if (!Request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeader))
        {
            _logger.LogWarning("API key header missing from request");
            return false;
        }

        var providedApiKey = apiKeyHeader.ToString();
        var isValid = expectedApiKey == providedApiKey;
        
        if (!isValid)
        {
            _logger.LogWarning("Invalid API key provided");
        }
        
        return isValid;
    }

    /// <summary>
    /// Locks a single file by removing all user ACLs and setting quarantine metadata
    /// Called by Azure Function when file is uploaded for AV scanning
    /// Requires X-Api-Key header
    /// </summary>
    [HttpPost("lock-file")]
    public async Task<IActionResult> LockFile([FromBody] LockFileRequest request)
    {
        if (!ValidateApiKey())
        {
            _logger.LogWarning("Unauthorized lock-file request");
            return Unauthorized(new { Message = "Invalid or missing API key" });
        }

        if (string.IsNullOrWhiteSpace(request.WorkspaceAcronym))
        {
            return BadRequest("Workspace acronym is required");
        }

        if (string.IsNullOrWhiteSpace(request.FilePath))
        {
            return BadRequest("File path is required");
        }

        try
        {
            _logger.LogInformation(
                "Locking file in workspace {Workspace} path {Path}",
                request.WorkspaceAcronym, request.FilePath);

            var updateCount = await _aclService.RemoveAllUserAclsFromPathAsync(
                request.WorkspaceAcronym,
                request.FilePath,
                recursive: false);

            return Ok(new AclOperationResponse
            {
                Success = true,
                UpdatedCount = updateCount,
                Message = $"Successfully locked file: {request.FilePath}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking file in workspace {Workspace} path {Path}", 
                request.WorkspaceAcronym, request.FilePath);
            return StatusCode(500, new AclOperationResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Restores access to a single file after successful AV scan
    /// Called by Azure Function when scan completes successfully
    /// Requires X-Api-Key header
    /// </summary>
    [HttpPost("restore-access")]
    public async Task<IActionResult> RestoreAccess([FromBody] RestoreAccessRequest request)
    {
        if (!ValidateApiKey())
        {
            _logger.LogWarning("Unauthorized restore-access request");
            return Unauthorized(new { Message = "Invalid or missing API key" });
        }

        if (string.IsNullOrWhiteSpace(request.WorkspaceAcronym))
        {
            return BadRequest("Workspace acronym is required");
        }

        if (string.IsNullOrWhiteSpace(request.FilePath))
        {
            return BadRequest("File path is required");
        }

        try
        {
            _logger.LogInformation(
                "Restoring access in workspace {Workspace} path {Path}",
                request.WorkspaceAcronym, request.FilePath);

            var updateCount = await _aclService.SimulateScanSuccessAsync(
                request.WorkspaceAcronym,
                request.FilePath);

            return Ok(new AclOperationResponse
            {
                Success = true,
                UpdatedCount = updateCount,
                Message = $"Successfully restored access to file: {request.FilePath}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring access in workspace {Workspace} path {Path}", 
                request.WorkspaceAcronym, request.FilePath);
            return StatusCode(500, new AclOperationResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            });
        }
    }
}

public class LockFileRequest
{
    public string WorkspaceAcronym { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
}

public class RestoreAccessRequest
{
    public string WorkspaceAcronym { get; set; } = string.Empty;
    public string? FilePath { get; set; }
}

public class AclOperationResponse
{
    public bool Success { get; set; }
    public int UpdatedCount { get; set; }
    public string? Message { get; set; }
}
