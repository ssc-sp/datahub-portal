using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Reflection;
using Datahub.Core.Services;
using System.IO;
using Datahub.Core.Utils;

namespace Datahub.Portal.Controllers
{
    [Route("[controller]/[action]")]
    [AllowAnonymous]
    public class ApiController : Controller
    {
        readonly ILogger<PublicController> _logger;
        private readonly IApiCallService _apiCallService;

        public ApiController(ILogger<PublicController> logger, IApiCallService apiCallService)
        {
            _logger = logger;
            _apiCallService = apiCallService;
        }

        [HttpPost]
        public async Task<IActionResult> Upload()
        {
            // [authorization] Signed JWT with the claims including the UserName 
            var authHeader = Request.Headers["Authorization"];

            // todo: validate token

            // todo: retrieve the project acronym from the authorization token
            var projectAcro = "canmetrobo";

            // get the file from the form
            var count = Request.Form.Files.Count;
            if (count != 1)
                return BadRequest();

            // create the file id
            var fileId = Guid.NewGuid().ToString();
            
            var file = Request.Form.Files[0];

            var dateStr = DateTime.UtcNow.ToString();
            var ext = Path.GetExtension(file.FileName);

            // build the storage file metadata
            Dictionary<string, string> fileMetadata = new()
            {
                { "fileid", fileId },
                { "filename", file.FileName },
                { "createdts", dateStr },
                { "lastmodifiedts", dateStr },
                { "securityclass", "Unclassified" },
                { "fileformat", ext },
                { "filesize", $"{file.Length}" }
            };

            try
            {
                var blobConnectionString = await _apiCallService.GetProjectConnectionString(projectAcro);

                BlobClientUtils blobClientUtil = new(blobConnectionString, "datahub");

                await blobClientUtil.UploadFile(file.FileName, file.OpenReadStream(), fileMetadata, (v) => { });

                return Ok($"Thanks for posting here with authorization: '{authHeader}' fileId: {fileId}.");
            }
            catch (Exception ex)
            {
                return Ok(ex);
            }
        }
    }
}
