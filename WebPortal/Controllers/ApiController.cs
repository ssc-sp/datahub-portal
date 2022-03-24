#nullable enable
using Datahub.Core.EFCore;
using Datahub.Core.Services;
using Datahub.Core.Utils;
using Datahub.Portal.Model;
using Datahub.Portal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Datahub.Metadata.DTO;
using System.Reflection;
using System.Threading;
using Datahub.Metadata.Model;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Datahub.Portal.Controllers
{
    [Route("[controller]")]
    [AllowAnonymous]
    public class ApiController : Controller
    {
        private readonly ILogger<PublicController> _logger;
        private readonly IDbContextFactory<DatahubProjectDBContext> _contextFactory;
        private readonly IKeyVaultService _keyVaultService;
        private readonly IMetadataBrokerService _metadataBrokerService;
        private readonly IPublicDataFileService _publicDataService;
        private readonly IMSGraphService _msGraphService;

        public ApiController(ILogger<PublicController> logger, 
            IDbContextFactory<DatahubProjectDBContext> contextFactory, IKeyVaultService keyVaultService, 
            IMetadataBrokerService metadataBrokerService, IPublicDataFileService publicDataService, IMSGraphService msGraphService)
        {
            _logger = logger;
            _contextFactory = contextFactory;
            _keyVaultService = keyVaultService;
            _metadataBrokerService = metadataBrokerService;
            _publicDataService = publicDataService;
            _msGraphService = msGraphService;
        }

        /* TBD!
        [HttpPost]
        public async Task<IActionResult> Upload()
        {
            // [authorization] Signed JWT with the claims including the UserName 
            var authHeader = Request.Headers["Authorization"];

            // todo: validate token
            var testToken = await _keyVaultService.EncryptApiTokenAsync("0123456789");

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
        */

        [HttpPost]
        [Route("opendata/submit")]
        public async Task<IActionResult> StartSharingOpenData([FromBody] OpenDataShareRequest data)
        {
            var authHeader = GetAuthorizationToken();

            // get the api use record
            var apiUser = await GetApiUserAsync(authHeader);
            if (apiUser is null || IsDisabledOrExpired(apiUser))
                return Unauthorized();

            // validate the model
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // check the file_id is a valid GUID 
            if (!Guid.TryParse(data.file_id, out var fileId))
                return BadRequest($"Invalid file_id, must be a valid GUID!");

            // check the file_id doesn't exists in the shared files
            var sharedFile = await _publicDataService.LoadOpenDataSharedFileInfo(fileId);
            if (sharedFile is not null)
                return BadRequest($"A share with id: {data.file_id} already exists!");

            // get the field definitions
            var fieldDefinitions = await _metadataBrokerService.GetFieldDefinitions();

            // validate choice fields
            var validateChoiceResponse = ValidateFieldChoices(data, fieldDefinitions);
            if (validateChoiceResponse is not null)
                return validateChoiceResponse;

            // validate date field formats
            var validateDateFieldsResponse = ValidateDateFields(data);
            if (validateDateFieldsResponse is not null)
                return validateDateFieldsResponse;

            // get the user id from the email contact
            var userId = await _msGraphService.GetUserIdFromEmailAsync(data.email_contact, CancellationToken.None);
            if (userId is null)
                return BadRequest($"Invalid email account '{data.email_contact}'");

            // create metadata record for external object
            await SaveMetadata(data, fieldDefinitions);

            // save a pre-filled approval form and retrieve the form id
            var approvalFormId = await SaveApprovalForm(data);

            // create open data share record
            var url = await _publicDataService.CreateExternalOpenDataSharing(approvalFormId, data.file_id, data.file_name, data.file_url,
                userId, apiUser.Project_Acronym_CD);

            return Ok(new OpenDataShareResponse(data.file_id, url));
        }

        [HttpGet]
        [Route("opendata/choices")]
        public async Task<IActionResult> GetFieldChoices()
        {
            // [authorization] Signed JWT with the claims including the UserName 
            var authHeader = GetAuthorizationToken();

            //// get the api use record
            var apiUser = await GetApiUserAsync(authHeader);
            if (apiUser is null || IsDisabledOrExpired(apiUser))
                return Unauthorized();

            // get the field definitions
            var fieldDefinitions = await _metadataBrokerService.GetFieldDefinitions();

            // get choice dictionaries
            var fieldChoices = EnumerateOpenDataShareRequestFields(fieldDefinitions).ToList();

            return Ok(fieldChoices);
        }

        [HttpPost]
        [Route("fgp/submit")]
        public async Task<IActionResult> StartSharingFgp()
        {
            // [authorization] Signed JWT with the claims including the UserName 
            var authHeader = GetAuthorizationToken();

            //// get the api use record
            var apiUser = await GetApiUserAsync(authHeader);
            if (apiUser is null || IsDisabledOrExpired(apiUser))
                return Unauthorized();

            // verify required email-contact
            var emailContact = Request.Headers["email-contact"];
            if (string.IsNullOrEmpty(emailContact))
                return BadRequest("Missing email-contact header");

            // read request json
            var requestJson = await ReadRequestAsString();

            // todo: validate json

            // parse the bear minimun data to create the approval form
            var requestSummary = ParseGeoData(requestJson);
            if (requestSummary is null)
                return BadRequest();

            // save a pre-filled approval form and retrieve the form id
            var approvalFormId = await SaveApprovalForm(requestSummary.title_en, emailContact, $"{requestSummary.title_en } / {requestSummary.title_en}");

            // save and store json request
            var shareId = await SaveGeoData(requestJson, approvalFormId);

            var url = _publicDataService.GetPublicSharedUrl($"/share/geodata/{shareId}");

            // todo
            return Ok(url);
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

        private async Task<string> SaveGeoData(string requestJson, int approvalFormId)
        {
            using var ctx = _contextFactory.CreateDbContext();

            GeoObjectShare geoObjectShare = new()
            {
                GeoObjectShare_ID = Guid.NewGuid().ToString(),
                Json_TXT = requestJson,
                ApprovalForm_ID = approvalFormId
            };

            ctx.GeoObjectShares.Add(geoObjectShare);
            await ctx.SaveChangesAsync();

            return geoObjectShare.GeoObjectShare_ID;
        }

        private Task<int> SaveApprovalForm(OpenDataShareRequest data)
        {
            return SaveApprovalForm(data.file_name, data.email_contact, $"{data.title_translated_en } / {data.title_translated_en}");
        }

        private async Task<int> SaveApprovalForm(string name, string email, string title)
        {
            Data.Forms.ShareWorkflow.ApprovalForm approvalForm = new()
            {
                Name_NAME = name,
                Email_EMAIL = email,
                Dataset_Title_TXT = title,
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
                    var splitChoices = (field.value ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim());
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

        private async Task<string> ReadRequestAsString()
        {
            using (var reader = new StreamReader(Request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
            {
                return await reader.ReadToEndAsync();
            }
        }

        private GeoDataShareRequest? ParseGeoData(string data)
        {
            return JsonSerializer.Deserialize<GeoDataShareRequest>(data);
        }
    }

    record FieldChoice(string value, string en, string fr);
    record FieldChoices(string field, List<FieldChoice> choices);
    record FieldValue(string name, string? value);
}
