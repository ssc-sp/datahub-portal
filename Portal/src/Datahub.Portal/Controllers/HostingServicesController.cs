using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Datahub.Portal.Controllers;

[ApiController]
[Authorize]
public class HostingServicesController : ControllerBase
{
    /// <summary>
    /// Handles the HTTP POST request to the "api/gc-hosting" endpoint.
    /// </summary>
    /// <returns>The IActionResult representing the response.</returns>
    [HttpPost("api/auth-echo")]
    public async Task<IActionResult> PostAuth()
    {
        try
        {
            var body = await new StreamReader(Request.Body).ReadToEndAsync();
            return Ok(body);
        }
        catch (Exception ex)
        {
            return Ok(ex.Message);
        }
    }

    [HttpPost("api/anon-echo")]
    [AllowAnonymous]
    public async Task<IActionResult> PostAnon()
    {
        try
        {
            var body = await new StreamReader(Request.Body).ReadToEndAsync();
            return Ok(body);
        }
        catch (Exception ex)
        {
            return Ok(ex.Message);
        }
    }
}
