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
using Microsoft.EntityFrameworkCore;
using Datahub.Core.EFCore;
using Datahub.Portal.Services.Storage;

namespace Datahub.Portal.Controllers
{
    [Route("[controller]/[action]")]
    [AllowAnonymous]
    public class ApiController : Controller
    {
        private readonly ILogger<PublicController> _logger;
        private readonly DataRetrievalService dataRetrievalService;
        private readonly IDbContextFactory<DatahubProjectDBContext> _contextFactory;
        private readonly IKeyVaultService _keyVaultService;

        public ApiController(ILogger<PublicController> logger, DataRetrievalService dataRetrievalService,
            IDbContextFactory<DatahubProjectDBContext> contextFactory, IKeyVaultService keyVaultService)
        {
            _logger = logger;
            this.dataRetrievalService = dataRetrievalService;
            _contextFactory = contextFactory;
            _keyVaultService = keyVaultService;
        }

        [HttpPost]
        public async Task<IActionResult> Upload()
        {
            // [authorization] Signed JWT with the claims including the UserName 
            var authHeader = Request.Headers["Authorization"];

            // todo: validate token
            //var testToken = await _keyVaultService.SignAsync("0123456789");

            // todo: extract the ProjectApiUser_Id fron the token
            var projectApiUserId = Guid.Parse("10A5280E-4432-411C-89AA-861FE9258A40");

            using var ctx = _contextFactory.CreateDbContext();
            var userInfo = await ctx.Project_ApiUsers.FirstAsync(e => e.ProjectApiUser_ID == projectApiUserId);

            if (userInfo == null)
                return Unauthorized();

            if (!userInfo.Enabled)
                return Unauthorized("Account is not enabled");

            // retrieve the project acronym
            var projectAcro = (userInfo.Project_Acronym_CD ?? "").ToLower();

            // get the file from the form
            var count = Request.Form.Files.Count;
            if (count != 1)
                return BadRequest("Must provide just one file.");

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
                var blobConnectionString = await dataRetrievalService.GetProjectConnectionString(projectAcro);

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
