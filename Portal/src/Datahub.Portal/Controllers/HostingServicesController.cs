using Microsoft.AspNetCore.Mvc;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Datahub.Core.Model.Onboarding;
using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Datahub.Core.Model.Achievements;
using Datahub.Application.Services.UserManagement;
using Datahub.Infrastructure.Queues.Messages;
using MassTransit;
using Datahub.Infrastructure.Extensions;
using Datahub.Shared.Configuration;
using Datahub.Application.Configuration;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;


namespace Datahub.Portal.Controllers;

[ApiController]
public class HostingServicesController : ControllerBase
{
    private readonly ILogger<HostingServicesController> _logger;
    private readonly DatahubProjectDBContext _context;
    private readonly IProjectCreationService _projectCreationService;
    private readonly IUserInformationService _userInformationService;
    private readonly IUserEnrollmentService _userEnrollmentService;
    private readonly ISendEndpointProvider _sendEndpointProvider;
    private readonly DatahubPortalConfiguration _datahubPortalConfiguration;

    private string message = "";

    public HostingServicesController(DatahubProjectDBContext context, IProjectCreationService projectCreationService, IUserInformationService userInformationService, IUserEnrollmentService userEnrollmentService, ILogger<HostingServicesController> logger, ISendEndpointProvider sendEndpointProvider, DatahubPortalConfiguration datahubPortalConfiguration)
    {
        _context = context;
        _projectCreationService = projectCreationService;
        _userInformationService = userInformationService;
        _userEnrollmentService = userEnrollmentService;
        _logger = logger;
        _sendEndpointProvider = sendEndpointProvider;
        _datahubPortalConfiguration = datahubPortalConfiguration;
    }

    /// <summary>
    /// Handles the authenticated HTTP POST request to the "api/auth-echo" endpoint.
    /// </summary>
    /// <returns>The IActionResult representing the response.</returns>
    [Route("api/auth-echo")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PostAuth()
    {
        _logger.LogInformation("Received authenticated request.");
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
        _logger.LogInformation("Received anonymous request.");
        return await ProcessRequest(Request);
    }

    /// <summary>
    /// Logic to process the request.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [NonAction]
    internal async Task<IActionResult> ProcessRequest(HttpRequest request)
    {
        try
        {
            var body = await new StreamReader(request.Body).ReadToEndAsync();
            var sanitizedBody = body.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", "");
            _logger.LogInformation("Received echo request body: {0}", sanitizedBody);
            return Ok(body);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error processing echo request: {0}", ex.Message);
            return Ok(ex.Message);
        }
    }

    /// <summary>
    /// Handles a request to create a new workspace from hosting services.
    /// </summary>
    /// <returns>Json containing the workspace acronym, resource group name, and tenant ID</returns>
    [Route("api/create-workspace")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PostCreateWorkspace()
    {
        try
        {
            // Deserialize the request body.
            var body = await new StreamReader(Request.Body).ReadToEndAsync();
            var savedToBlob = await SaveRequestToBlob(body);

            if (savedToBlob is UnauthorizedResult)
            {
                _logger.LogError("Failed to save request to blob storage.");
                return savedToBlob;
            }
            _logger.LogInformation("Saved request to blob storage.");

            var sanitizedBody = HttpUtility.HtmlEncode(body.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", ""));
            _logger.LogInformation("Received create workspace request body: {0}", sanitizedBody);

            var workspaceDetails = JsonConvert.DeserializeObject<HostingServiceInfo>(body);

            // Create a new workspace.
            string acronym = await _projectCreationService.GenerateProjectAcronymAsync(workspaceDetails.WorkspaceTitle);
            string rg = $"fsdh_proj_{acronym.ToLower()}_dev_rg";
            var sanitizedAcronym = HttpUtility.HtmlEncode(acronym.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", ""));
            _logger.LogInformation("Generated acronym: {0}", sanitizedAcronym);

            // Attempt to find the user in the database.
            var users = _context.PortalUsers.ToListAsync();
            var user = users.Result.FirstOrDefault(e => e.Email == workspaceDetails.LeadEmail);

            if (user == null) // If the user is not found, register the user.
            {
                _logger.LogInformation("User not found, registering user.");
                await RegisterUser(workspaceDetails.LeadEmail);
                int attempt = 0;
                
                while (user == null && attempt < 5)
                {
                    await Task.Delay(2000);
                    _logger.LogInformation("Attempt {0} to find user.", attempt);
                    user = await _context.PortalUsers.FirstOrDefaultAsync(e => e.Email == workspaceDetails.LeadEmail);
                    attempt++;
                }
            }

            // If the user is found or registered successfully, create the project.
            if (user != null)
            {
                _logger.LogInformation("User found, creating project.");
                return await CreateProject(workspaceDetails, acronym, rg, user);
            }
            else
            {
                ReportErrorCreatingWorkspace(workspaceDetails);
                return BadRequest("Failed to create workspace - Could not register workspace lead");
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex.ToString() + message);
        }
    }

    /// <summary>
    /// Saves the request to blob storage.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task<IActionResult> SaveRequestToBlob(string request)
    {
        if (_datahubPortalConfiguration?.Media?.StorageConnectionString is null)
            return Unauthorized("No token available");

        var blobReference = CloudStorageAccount.Parse(_datahubPortalConfiguration.Media.StorageConnectionString)
            .CreateCloudBlobClient()
            .GetContainerReference("hosting-requests")
            .GetBlockBlobReference(Guid.NewGuid().ToString());

        await blobReference.UploadTextAsync(request);
        return Ok();
    }

    /// <summary>
    /// Reports an error creating a workspace to the bug report queue.
    /// </summary>
    /// <param name="workspaceDetails"></param>
    /// <returns></returns>
    private async Task ReportErrorCreatingWorkspace(HostingServiceInfo workspaceDetails)
    {
        string description = $"Failed to create workspace {workspaceDetails.WorkspaceTitle} with workspace lead {workspaceDetails.LeadEmail}";

        _logger.LogError(description.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", ""));

        var bugReport = new BugReportMessage(
            UserName: "Datahub Portal",
            UserEmail: "n/a",
            UserOrganization: "FSDH",
            PortalLanguage: "n/a",
            PreferredLanguage: "n/a",
            Timezone: "UTC",
            Workspaces: "n/a",
            Topics: "Workspace Creation Failure",
            URL: "n/a",
            UserAgent: "HostingServicesController",
            Resolution: "n/a",
            LocalStorage: "n/a",
            BugReportType: BugReportTypes.SystemError,
            Description: description
        );

        await _sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.BugReportQueueName, bugReport);
    }

    /// <summary>
    /// Registers a new user in the database and sends an invite to the user.
    /// </summary>
    /// <param name="email"></param>
    /// <returns>PortalUser object for the newly created user</returns>
    [NonAction]
    private async Task<PortalUser> RegisterUser(string email)
    {
        try
        {
            await _userEnrollmentService.SaveRegistrationDetails(email, "HostingServices");
            var userId = await _userEnrollmentService.SendUserDatahubPortalInvite(email, "FSDH");
            _logger.LogInformation("User invite sent, user ID is {0}", userId);
            await _userInformationService.CreatePortalUserAsync(userId);
            return await _context.PortalUsers.FirstOrDefaultAsync(e => e.Email == email); ;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return null;
        }
    }

    /// <summary>
    /// Creates a new project and adds it to the database, returns the acronym and resource group names
    /// </summary>
    /// <param name="workspaceDetails"></param>
    /// <param name="acronym"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    [NonAction]
    private async Task<IActionResult> CreateProject(HostingServiceInfo workspaceDetails, string acronym, string rg, PortalUser user)
    {
        var sanitizedWorkspaceTitle = HttpUtility.HtmlEncode(workspaceDetails.WorkspaceTitle.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", ""));
        _logger.LogInformation("Creating project for workspace {0}", sanitizedWorkspaceTitle);
        var isAdded = await _projectCreationService.CreateProjectCloudHostingEndPointAsync(workspaceDetails.WorkspaceTitle, acronym, "Shared Services Canada", user);

        if (isAdded)
        {
            _logger.LogInformation("Project created successfully, saving project creation details.");
            await _projectCreationService.SaveProjectCreationDetailsAsync(acronym, workspaceDetails.AreaOfScience);

            // Retrieve the workspace details.
            var project = await _context.Projects.FirstOrDefaultAsync(e => e.Project_Acronym_CD == acronym);

            // Create a new GC Hosting workspace record using the given details.
            _logger.LogInformation("Creating GC Hosting workspace record.");
            GCHostingWorkspaceDetails gcHostingRecord = ConvertInputToGCHostingObject(workspaceDetails);
            gcHostingRecord.Datahub_Project = project;
            _context.GCHostingWorkspaceDetails.Add(gcHostingRecord);
            await _context.SaveChangesAsync();

            // Return the workspace acronym, resource group name, and tenant ID.
            return Ok(new object[] { acronym, rg });
        }
        else
        {
            return Ok("Failed to create workspace.");
        }
    }

    private GCHostingWorkspaceDetails ConvertInputToGCHostingObject(HostingServiceInfo input)
    {
        GCHostingWorkspaceDetails temp = new GCHostingWorkspaceDetails();
        temp.GcHostingId = input.GcHostingId;
        temp.Id = (int) input.Id;
        temp.LeadFirstName = input.LeadFirstName;
        temp.LeadLastName = input.LeadLastName;
        temp.DepartmentName = input.DepartmentName;
        temp.LeadEmail = input.LeadEmail;
        temp.FinancialAuthorityFirstName = input.FinancialAuthorityFirstName;
        temp.FinancialAuthorityLastName = input.FinancialAuthorityLastName;
        temp.FinancialAuthorityCostCentre = input.FinancialAuthorityCostCentre;
        temp.FinancialAuthorityEmail = input.FinancialAuthorityEmail;
        temp.WorkspaceBudget = Decimal.Parse(input.WorkspaceBudget);
        temp.WorkspaceTitle = input.WorkspaceTitle;
        temp.WorkspaceDescription = input.WorkspaceDescription;
        temp.WorkspaceIdentifier = input.WorkspaceIdentifier;
        temp.Subject = input.Subject;
        temp.Keywords = input.Keywords;
        temp.AreaOfScience = input.AreaOfScience;
        temp.RetentionPeriodYears = input.RetentionPeriodYears;
        temp.RetentionPeriodStartDate = input.RetentionPeriodStartDate.DateTime;
        temp.RetentionValue = input.RetentionValue;
        temp.SecurityClassification = input.SecurityClassification;
        temp.GeneratesInfoBusinessValue = input.GeneratesInfoBusinessValue;
        temp.ProjectTitle = input.ProjectTitle;
        temp.ProjectDescription = input.ProjectDescription;
        temp.ProjectStartDate = input.ProjectStartDate.DateTime;
        temp.ProjectEndDate = input.ProjectEndDate.DateTime;
        temp.CBRName = input.CBRName;
        temp.CBRID = input.CBRID;
        return temp;
    }

    public partial class HostingServiceInfo
    {
        [Newtonsoft.Json.JsonProperty("GcHostingId", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string GcHostingId { get; set; }

        [Newtonsoft.Json.JsonProperty("Id", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public long Id { get; set; }

        [Newtonsoft.Json.JsonProperty("LeadFirstName", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string LeadFirstName { get; set; }

        [Newtonsoft.Json.JsonProperty("LeadLastName", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string LeadLastName { get; set; }

        [Newtonsoft.Json.JsonProperty("DepartmentName", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string DepartmentName { get; set; }

        [Newtonsoft.Json.JsonProperty("LeadEmail", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string LeadEmail { get; set; }

        [Newtonsoft.Json.JsonProperty("FinancialAuthorityFirstName", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string FinancialAuthorityFirstName { get; set; }

        [Newtonsoft.Json.JsonProperty("FinancialAuthorityLastName", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string FinancialAuthorityLastName { get; set; }

        [Newtonsoft.Json.JsonProperty("FinancialAuthorityCostCentre", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string FinancialAuthorityCostCentre { get; set; }

        [Newtonsoft.Json.JsonProperty("FinancialAuthorityEmail", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string FinancialAuthorityEmail { get; set; }

        [Newtonsoft.Json.JsonProperty("WorkspaceBudget", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string WorkspaceBudget { get; set; }

        [Newtonsoft.Json.JsonProperty("WorkspaceTitle", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string WorkspaceTitle { get; set; }

        [Newtonsoft.Json.JsonProperty("WorkspaceDescription", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string WorkspaceDescription { get; set; }

        [Newtonsoft.Json.JsonProperty("WorkspaceIdentifier", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string WorkspaceIdentifier { get; set; }

        [Newtonsoft.Json.JsonProperty("Subject", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Subject { get; set; }

        [Newtonsoft.Json.JsonProperty("Keywords", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Keywords { get; set; }

        [Newtonsoft.Json.JsonProperty("AreaOfScience", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string AreaOfScience { get; set; }

        [Newtonsoft.Json.JsonProperty("RetentionPeriodYears", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int RetentionPeriodYears { get; set; }

        [Newtonsoft.Json.JsonProperty("RetentionPeriodStartDate", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public DateTimeOffset RetentionPeriodStartDate { get; set; }

        [Newtonsoft.Json.JsonProperty("RetentionValue", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string RetentionValue { get; set; }

        [Newtonsoft.Json.JsonProperty("SecurityClassification", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string SecurityClassification { get; set; }

        [Newtonsoft.Json.JsonProperty("GeneratesInfoBusinessValue", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool GeneratesInfoBusinessValue { get; set; }

        [Newtonsoft.Json.JsonProperty("ProjectTitle", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string ProjectTitle { get; set; }

        [Newtonsoft.Json.JsonProperty("ProjectDescription", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string ProjectDescription { get; set; }

        [Newtonsoft.Json.JsonProperty("ProjectStartDate", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.DateTimeOffset ProjectStartDate { get; set; }

        [Newtonsoft.Json.JsonProperty("ProjectEndDate", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.DateTimeOffset ProjectEndDate { get; set; }

        [Newtonsoft.Json.JsonProperty("CBRName", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string CBRName { get; set; }

        [Newtonsoft.Json.JsonProperty("CBRID", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string CBRID { get; set; }

        private System.Collections.Generic.IDictionary<string, object> _additionalProperties;

        [Newtonsoft.Json.JsonExtensionData]
        public System.Collections.Generic.IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>()); }
            set { _additionalProperties = value; }
        }

    }
}
