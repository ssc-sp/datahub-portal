using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Datahub.Portal.Controllers
{
	[Route("[controller]")]
    [AllowAnonymous]
    public class E2EController : Controller
    {
        private readonly ILogger<ApiController> _logger;

        public E2EController(ILogger<ApiController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            //var token = Request.Headers["Authorization"].ToString();

            //if (token == null)
            //    return Unauthorized();

            //var claims = new List<Claim>()
            //{
            //};

            //var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            //var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            //await Request.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            //_logger.LogDebug($"Unauthenticated hello world: {token}");

            return Redirect("/home");
        }
    }
}
