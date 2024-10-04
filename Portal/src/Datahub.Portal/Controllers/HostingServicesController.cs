using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Datahub.Portal.Controllers;

[ApiController]
public class HostingServicesController : ControllerBase
{
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
}
