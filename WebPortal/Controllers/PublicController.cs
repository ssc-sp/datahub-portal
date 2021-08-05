using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NRCan.Datahub.Shared.Data;
using NRCan.Datahub.Shared.Services;

namespace NRCan.Datahub.Portal.Controllers
{
    [Route("[controller]/[action]")]
    [AllowAnonymous]
    public class PublicController: Controller
    {
        private ILogger<PublicController> _logger { get; set; }
        private IApiService _apiService { get; set; }

        public PublicController(ILogger<PublicController> logger, IApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        public IActionResult HelloWorld()
        {
            _logger.LogDebug("Unauthenticated hello world");
            return Ok("hello world");
        }

        public async Task<IActionResult> BlobTest()
        {
            var filemd = new FileMetaData()
            {
                filename = "privacy.html",
                name = "privacy.html"
            };

            var project = "canmetrobo";

            _logger.LogDebug($"Downloading {filemd.filename} from project {project}");

            var uri = await _apiService.GetUserDelegationSasBlob(filemd, project);

            return Redirect(uri.ToString());
        }

        public async Task<IActionResult> DataLakeTest()
        {
            var filemd = new FileMetaData()
            {
                folderpath = "nrcan-rncan.gc.ca/alexander.khavich",
                filename = "serious.gif"
            };

            _logger.LogDebug($"Downloading {filemd.filename}");

            var uri = await _apiService.DownloadFile(filemd);
            return Redirect(uri.ToString());
        }
    }
}