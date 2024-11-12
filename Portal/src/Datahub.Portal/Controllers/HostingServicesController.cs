using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Datahub.Core.Model.Onboarding;
using Datahub.Application.Services;
using Datahub.Infrastructure.Services;
using Datahub.Portal.Pages;
using Datahub.Core.Model.Context;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Profiling.Internal;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Data.Databricks;
using System.ComponentModel;
using Datahub.Application.Services.UserManagement;
using static DeepL.Model.Usage;

namespace Datahub.Portal.Controllers;

[ApiController]
public class HostingServicesController : ControllerBase
{
    private readonly DatahubProjectDBContext _context;
    private readonly IProjectCreationService _projectCreationService;
    private readonly IUserInformationService _userInformationService;
    private readonly IUserEnrollmentService _userEnrollmentService;

    private string message = "";

    public HostingServicesController(DatahubProjectDBContext context, IProjectCreationService projectCreationService, IUserInformationService userInformationService, IUserEnrollmentService userEnrollmentService)
    {
        _context = context;
        _projectCreationService = projectCreationService;
        _userInformationService = userInformationService;
        _userEnrollmentService = userEnrollmentService;
    }

    /// <summary>
    /// Handles the authenticated HTTP POST request to the "api/auth-echo" endpoint.
    /// </summary>
    /// <returns>The IActionResult representing the response.</returns>
    [Route("api/auth-echo")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PostAuth()
    {
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
        return await ProcessRequest(Request);
    }

    /// <summary>
    /// Logic to process the request.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<IActionResult> ProcessRequest(HttpRequest request)
    {
        try
        {
            var body = await new StreamReader(request.Body).ReadToEndAsync();
            return Ok(body);
        }
        catch (Exception ex)
        {
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

            var workspaceDetails = JsonConvert.DeserializeObject<HostingServiceInfo>(body);

            // Create a new workspace.
            string acronym = await _projectCreationService.GenerateProjectAcronymAsync(workspaceDetails.WorkspaceTitle);
            string rg = $"fsdh_proj_{acronym.ToLower()}_dev_rg";

            // Create a new workspace. (Only to be done when authentication is complete)
            var users = _context.PortalUsers.ToListAsync();
            var user = users.Result.FirstOrDefault(e => e.Email == workspaceDetails.LeadEmail);

            //if (user == null)
            //{
            //    await _userEnrollmentService.SaveRegistrationDetails(workspaceDetails.LeadEmail, "HostingServices");
            //    var userId = await _userEnrollmentService.SendUserDatahubPortalInvite(workspaceDetails.LeadEmail, default);
            //}

            var isAdded = await _projectCreationService.CreateProjectCloudHostingEndPointAsync(workspaceDetails.WorkspaceTitle, acronym, "Shared Services Canada", user);

            if (isAdded)
            {
                await _projectCreationService.SaveProjectCreationDetailsAsync(acronym, workspaceDetails.AreaOfScience);

                // Retrieve the workspace details.
                var project = await _context.Projects.FirstOrDefaultAsync(e => e.Project_Acronym_CD == acronym);

                // Create a new GC Hosting workspace record using the given details.
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
        catch (Exception ex)
        {
            return Ok(ex.ToString() + message);
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
        temp.WorkspaceTitle = input.WorkspaceTitle;
        temp.WorkspaceDescription = input.WorkspaceDescription;
        temp.WorkspaceIdentifier = input.WorkspaceIdentifier;
        temp.Subject = input.Subject;
        temp.Keywords = input.Keywords;
        temp.AreaOfScience = input.AreaOfScience;
        temp.RetentionPeriodYears = input.RetentionPeriodYears;
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
