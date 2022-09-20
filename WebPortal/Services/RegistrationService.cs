using System.Text;
using System.Text.Json.Nodes;
using Datahub.Core.EFCore;
using Datahub.Core.Services;
using Datahub.Portal.Data.Forms;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Portal.Services;

public class RegistrationService
{
    public const string SELF_SIGNUP = "self-signup page";
    private const string DEFAULT_DATABRICKS_URL = "https://adb-5415916054641848.8.azuredatabricks.net/";
    private const string DEFAULT_PROJECT_STATUS = "Ongoing";

    private readonly IDbContextFactory<DatahubProjectDBContext> _dbFactory;
    private readonly ILogger<RegistrationService> _logger;

    private readonly List<string> _blacklistedAcronyms = new()
    {
        "new",
    };

    private readonly IKeyVaultService _keyVaultService;

    public RegistrationService(
        IDbContextFactory<DatahubProjectDBContext> dbFactory, 
        ILogger<RegistrationService> logger,
        IKeyVaultService keyVaultService
        )
    {
        _dbFactory = dbFactory;
        _logger = logger;
        _keyVaultService = keyVaultService;
    }

    public async Task<bool> IsValidRegistrationRequest(Datahub_Registration_Request registrationRequest)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var exists = await db.Registration_Requests
            .FirstOrDefaultAsync(r => r.Email == registrationRequest.Email);

        if (exists == null)
            return false;

        var validAcronym = await IsValidUniqueProjectAcronym(exists.ProjectAcronym);

        return validAcronym;
    }

    public async Task<bool> IsValidUniqueProjectAcronym(string projectAcronym)
    {
        if (string.IsNullOrWhiteSpace(projectAcronym) || _blacklistedAcronyms.Any(a => a.ToLower() == projectAcronym.ToLower()))
            return false;
        
        await using var db = await _dbFactory.CreateDbContextAsync();
        
        return !await db.Projects.AnyAsync(p =>
            p.Project_Acronym_CD.ToLower() == projectAcronym.ToLower());
    }
    
    public async Task SubmitRegistration(BasicIntakeForm basicIntakeForm, string createdBy)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var exists = await db.Registration_Requests
            .FirstOrDefaultAsync(r => r.Email == basicIntakeForm.Email);

        if (exists != null)
        {
            _logger.LogInformation("Registration request for {Email} already exists", basicIntakeForm.Email);
            exists.UpdatedBy = createdBy;
            exists.ProjectName = basicIntakeForm.ProjectName;
            exists.DepartmentName = basicIntakeForm.DepartmentName;
            exists.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _logger.LogInformation("Submitting new Registration request for {Email} from {Department} on {Project}]",
                basicIntakeForm.Email, basicIntakeForm.DepartmentName, basicIntakeForm.ProjectName);

            var registrationRequest = new Datahub_Registration_Request()
            {
                Email = basicIntakeForm.Email,
                DepartmentName = basicIntakeForm.DepartmentName,
                ProjectName = basicIntakeForm.ProjectName,
                CreatedBy = createdBy,
                CreatedAt = DateTime.Now,
            };
            await db.Registration_Requests.AddAsync(registrationRequest);
        }

        await db.SaveChangesAsync();
    }

    public async Task CreateProject(Datahub_Registration_Request registrationRequest, string adminUserId)
    {
        if (!await IsValidRegistrationRequest(registrationRequest))
            throw new InvalidOperationException("Invalid registration request");
        
        if(string.IsNullOrWhiteSpace(adminUserId))
            throw new ArgumentNullException(nameof(adminUserId));
        
        
        await using var db = await _dbFactory.CreateDbContextAsync();
        BasicIntakeForm.DepartmentDictionary.TryGetValue(registrationRequest.DepartmentName, out var mappedSectorName);

        var project = new Datahub_Project()
        {
            Project_Acronym_CD = registrationRequest.ProjectAcronym,
            Project_Name = registrationRequest.ProjectName,
            Branch_Name = registrationRequest.DepartmentName,
            Sector_Name = mappedSectorName ?? registrationRequest.DepartmentName,
            Contact_List = registrationRequest.Email,
            Project_Admin = registrationRequest.Email,
            
            Databricks_URL = DEFAULT_DATABRICKS_URL,
            Project_Status_Desc = DEFAULT_PROJECT_STATUS,
        };  
        
        await db.Projects.AddAsync(project);

        registrationRequest.Status = Datahub_Registration_Request.STATUS_CREATED;
        registrationRequest.UpdatedAt = DateTime.UtcNow;
        registrationRequest.UpdatedBy = adminUserId;
        
        db.Entry(registrationRequest).State = EntityState.Modified;
        
        await db.SaveChangesAsync();
    }

    public async Task AddUserToExistingProject(Datahub_Registration_Request registrationRequest, string adminUserId, bool isProjectAdmin = false)
    {
        if (string.IsNullOrWhiteSpace(adminUserId))
            throw new ArgumentNullException(nameof(adminUserId));
        
        if (string.IsNullOrWhiteSpace(registrationRequest.Email))
            throw new ArgumentNullException(nameof(registrationRequest.Email));
        
        await using var db = await _dbFactory.CreateDbContextAsync();
        
        var user = await db.Project_Users
            .FirstOrDefaultAsync(u => u.User_Name == registrationRequest.Email);
        
        var project = await db.Projects
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == registrationRequest.ProjectAcronym);
        
        if(project == null)
            throw new InvalidOperationException($"Project with acronym {registrationRequest.ProjectAcronym} not found");
        
        if(user != null)
            throw new InvalidOperationException($"User already associated with project {registrationRequest.ProjectAcronym}");

        var guid = await SendUserInvite(registrationRequest.Email);
        user = new Datahub_Project_User
        {
            User_Name = registrationRequest.Email,
            User_ID = guid,
            ApprovedUser = adminUserId,
            Approved_DT = DateTime.Now,
            IsAdmin = isProjectAdmin,
            IsDataApprover = true,
            Project = project
        };

        await db.Project_Users.AddAsync(user);
        
        registrationRequest.Status = Datahub_Registration_Request.STATUS_COMPLETED;
        registrationRequest.UpdatedAt = DateTime.UtcNow;
        registrationRequest.UpdatedBy = adminUserId;
        
        db.Entry(registrationRequest).State = EntityState.Modified;
        await db.SaveChangesAsync();
    }

    public async Task<string> SendUserInvite(string registrationRequestEmail, bool mockInvite = false)
    {
        using var client = new HttpClient();

        var payload = new Dictionary<string, JsonNode>
        {
            ["email"] = registrationRequestEmail,
            ["mockInvite"] = mockInvite,
        };

        var jsonBody = new JsonObject(payload);
        var url = await _keyVaultService.GetSecret("datahub-create-graph-user-url");
        
        var content = new StringContent(jsonBody.ToString(), Encoding.UTF8, "application/json");
        var result = await client.PostAsync(url, content);
        
        var resultString  = await result.Content.ReadAsStringAsync();
        if (mockInvite)
        {
            return resultString;
        }
        
        var resultJson = JsonNode.Parse(resultString);
        return resultJson?["data"]?["id"]?.ToString();
    }
}