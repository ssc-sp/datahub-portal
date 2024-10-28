using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Datahub.Core.Model.Onboarding;
using Datahub.Application.Services;
using Datahub.Infrastructure.Services;
using Datahub.Portal.Pages;
using Datahub.Core.Model.Context;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using StackExchange.Profiling.Internal;
using System.Text.Json.Serialization;

namespace Datahub.Portal.Controllers;

[ApiController]
public class HostingServicesController : ControllerBase
{
    private readonly DatahubProjectDBContext _context;
    private readonly IProjectCreationService _projectCreationService;

    private string message = "";

    public HostingServicesController(DatahubProjectDBContext context, IProjectCreationService projectCreationService)
    {
        _context = context;
        _projectCreationService = projectCreationService;
    }

    /// <summary>
    /// Handles the authenticated HTTP POST request to the "api/auth-echo" endpoint.
    /// </summary>
    /// <returns>The IActionResult representing the response.</returns>
    [Route("api/auth-echo")]
    [Authorize]
    public async Task<IActionResult> PostAuth()
    {
        return await ProcessRequest(Request);
    }

    /// <summary>
    /// Handles the anonymous HTTP POST request to the "api/anon-echo" endpoint.
    /// </summary>
    /// <returns></returns>
    [Route("api/anon-echo")]
    [AllowAnonymous]
    public async Task<IActionResult> PostAnon()
    {
        return await ProcessRequest(Request);
    }

    /// <summary>
    /// Logic to process the request.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<IActionResult> ProcessRequest(HttpRequest request)
    {
        try
        {
            var body = await new StreamReader(request.Body).ReadToEndAsync();
            return Ok(body);
        }
        catch (Exception ex)
        {
            return Ok(ex.Message);
        }
    }

    /// <summary>
    /// Handles a request to create a new workspace from hosting services.
    /// </summary>
    /// <returns>Json containing the workspace acronym, resource group name, and tenant ID</returns>
    [Route("api/create-workspace")]
    [AllowAnonymous]
    public async Task<IActionResult> PostCreateWorkspace()
    {
        try
        {
            // Deserialize the request body.
            var body = await new StreamReader(Request.Body).ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var workspaceDetails = JsonSerializer.Deserialize<GCHostingWorkspaceDetails>(body);

            // Create a new workspace.
            string acronym = await _projectCreationService.GenerateProjectAcronymAsync(workspaceDetails.WorkspaceTitle);

            var isAdded = await _projectCreationService.CreateProjectAsync(workspaceDetails.WorkspaceTitle, acronym, "Unspecified");

            if (isAdded)
            {
                await _projectCreationService.SaveProjectCreationDetailsAsync(acronym, workspaceDetails.AreaOfScience);

                // Retrieve the workspace details.
                var project = await _context.Projects.FirstOrDefaultAsync(e => e.Project_Acronym_CD == acronym);

                workspaceDetails.Datahub_Project = project;

                _context.GCHostingWorkspaceDetails.Add(workspaceDetails);
                await _context.SaveChangesAsync();

                // Return the workspace acronym, resource group name, and tenant ID.
                return Ok(new object[] { acronym });
            }
            else
            {
                return Ok("Failed to create workspace.");
            }
        }
        catch (Exception ex)
        {
            return Ok(ex.ToString() + message);
        }
    }
}
