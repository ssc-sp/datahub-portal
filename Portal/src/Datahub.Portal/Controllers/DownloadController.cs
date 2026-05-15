using Microsoft.AspNetCore.Mvc;

namespace Datahub.Portal.Controllers
{
    public class DownloadController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
