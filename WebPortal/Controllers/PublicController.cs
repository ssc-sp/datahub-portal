using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Datahub.Portal.Services;
using Datahub.Core.Data;
using Datahub.Core.Services;
using Datahub.Portal.Services.Storage;

namespace Datahub.Portal.Controllers
{
    [Route("[controller]/[action]")]
    [AllowAnonymous]
    public class PublicController: Controller
    {
        private readonly DataRetrievalService dataRetrievalService;

        private ILogger<PublicController> _logger { get; set; }
        private MyDataService _apiService { get; set; }
        private IPublicDataFileService _pubFileService { get; set; }

        public PublicController(
            ILogger<PublicController> logger, 
            MyDataService apiService,
            IPublicDataFileService pubFileService,
            DataRetrievalService dataRetrievalService
            )
        {
            _logger = logger;
            _apiService = apiService;
            _pubFileService = pubFileService;
            this.dataRetrievalService = dataRetrievalService;
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

            var uri = await dataRetrievalService.GetUserDelegationSasBlob(DataRetrievalService.DEFAULT_CONTAINER_NAME, filemd.filename, project);

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

            var uri = await dataRetrievalService.DownloadFile(DataRetrievalService.DEFAULT_CONTAINER_NAME, filemd,null);
            return Redirect(uri.ToString());
        }

        [Route("{fileId}")]
        public async Task<IActionResult> DownloadFile(string fileId)
        {
            var remoteIp = HttpContext.Connection.RemoteIpAddress;

            try
            {
                var fileIdGuid = Guid.Parse(fileId);
                var result = await _pubFileService.DownloadPublicUrlSharedFile(fileIdGuid, remoteIp);
                if (result == null)
                {
                    _logger.LogError($"File not found: {fileId}");
                    return NotFound();
                }
                else
                {
                    return Redirect(result.ToString());
                }
            }
            catch (FormatException)
            {
                _logger.LogError($"Invalid file id (not a guid): {fileId}");
                return NotFound();
            }
        }
    }
}