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

        [HttpGet]
        [Route("opendata/choices")]
        public async Task<IActionResult> GetFieldChoices()
        {
            // [authorization] Signed JWT with the claims including the UserName 
            //var authHeader = GetAuthorizationToken();

            //// get the api use record
            //var apiUser = await GetApiUserAsync(authHeader);
            //if (apiUser is null || IsDisabledOrExpired(apiUser))
            //    return Unauthorized();

            // get the field definitions
            var fieldDefinitions = await _metadataBrokerService.GetFieldDefinitions();

            // get choice dictionaries
            var fieldChoices = EnumerateOpenDataShareRequestFields(fieldDefinitions).ToList();

            return Ok(fieldChoices);
        }

        private string GetAuthorizationToken() => Request.Headers["DH-Auth-Key"];

        private bool IsDisabledOrExpired(Datahub_ProjectApiUser apiUser) 
            => !apiUser.Enabled || (apiUser.Expiration_DT.HasValue && apiUser.Expiration_DT.Value > DateTime.UtcNow);

        private async Task SaveMetadata(OpenDataShareRequest data, FieldDefinitions fieldDefinitions)
        {
            var fieldValues = await _metadataBrokerService.GetObjectMetadataValues(data.file_id);
            foreach (var field in EnumerateFieldValues(data))
            {
                var definition = fieldDefinitions.Get(field.name);
                if (definition is not null)
                {
                    fieldValues.SetValue(field.name, field.value ?? string.Empty);
                }
            }
            await _metadataBrokerService.SaveMetadata(fieldValues, anonymous: true);
        }

        private async Task<int> SaveApprovalForm(OpenDataShareRequest data)
        {
            Data.Forms.ShareWorkflow.ApprovalForm approvalForm = new()
            {
                Name_NAME = data.file_name,
                Email_EMAIL = data.email_contact,
                Dataset_Title_TXT = $"{data.title_translated_en } / {data.title_translated_en}",
                Type_Of_Data_TXT = "Data"
            };
            return await _metadataBrokerService.SaveApprovalForm(approvalForm);
        }

        private async Task<Datahub_ProjectApiUser?> GetApiUserAsync(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return null;

                var apiClientId = await _keyVaultService.DecryptApiTokenAsync(token);
                Guid apiClientGuid = Guid.Parse(apiClientId);

                using var ctx = _contextFactory.CreateDbContext();
                var apiUser = await ctx.Project_ApiUsers.FirstOrDefaultAsync(u => u.ProjectApiUser_ID == apiClientGuid);

                return apiUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve API Client data from given token {token}.");
                return null;
            }
        }

        static IEnumerable<FieldChoices> EnumerateOpenDataShareRequestFields(FieldDefinitions fieldDefinitions)
        {
            var fields = typeof(OpenDataShareRequest).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields)
            {
                var definition = fieldDefinitions.Get(field.Name);
                if (definition is not null && definition.HasChoices)
                {
                    var choices = definition.Choices.Select(c => new FieldChoice(c.Value_TXT, c.Label_English_TXT, c.Label_French_TXT)).ToList();
                    yield return new FieldChoices(field.Name, choices); 
                }
            }
        }

        private IActionResult? ValidateFieldChoices(OpenDataShareRequest data, FieldDefinitions fieldDefinitions)
        {
            foreach (var field in EnumerateFieldValues(data))
            {
                var definition = fieldDefinitions.Get(field.name);
                if (definition is not null && definition.HasChoices)
                {
                    var splitChoices = (field.value ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var choice in splitChoices)
                    {
                        var choiceValue = definition.GetChoiceTextValue(choice, true);
                        if (string.IsNullOrEmpty(choiceValue))
                        {
                            return BadRequest($"Field {field.name} with invalid choice '{field.value}'.");
                        }
                    }
                }
            }
            return null;
        }

        private IActionResult? ValidateDateFields(OpenDataShareRequest data)
        {
            if (!IsValidDate(data.date_published))
                return BadRequest("Invalid 'date_published'");

            if (!string.IsNullOrEmpty(data.time_period_coverage_start) && !IsValidDate(data.time_period_coverage_start))
                return BadRequest("Invalid 'time_period_coverage_start'");

            if (!string.IsNullOrEmpty(data.time_period_coverage_end) && !IsValidDate(data.time_period_coverage_end))
                return BadRequest("Invalid 'time_period_coverage_end'");

            return null;
        }

        static bool IsValidDate(string dateStr)
        {
            return DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ignored);
        }

        private IEnumerable<FieldValue> EnumerateFieldValues(OpenDataShareRequest data)
        {
            var props = data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in props)
            {
                if (Attribute.IsDefined(prop, typeof(MetadataField)))
                {
                    yield return new FieldValue(prop.Name, prop.GetValue(data, null)?.ToString());
                }
            }
        }
    }

    record FieldChoice(string value, string en, string fr);
    record FieldChoices(string field, List<FieldChoice> choices);
    record FieldValue(string name, string? value);
}
