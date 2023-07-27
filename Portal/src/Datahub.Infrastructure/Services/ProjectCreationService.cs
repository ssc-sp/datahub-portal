#nullable enable
using System.Transactions;
using Datahub.Application.Services;
using Datahub.Core.Data;
using Datahub.Core.Data.ResourceProvisioner;
using Datahub.Core.Enums;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Onboarding;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services;
using Datahub.Core.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace Datahub.Infrastructure.Services;

public class ProjectCreationService : IProjectCreationService
{
    private readonly IConfiguration _configuration;
    private readonly IDbContextFactory<DatahubProjectDBContext> _datahubProjectDbFactory;
    private readonly ILogger<ProjectCreationService> _logger;
    private readonly ServiceAuthManager serviceAuthManager;
    private readonly IUserInformationService _userInformationService;
    private readonly IResourceRequestService _resourceRequestService;
    private readonly IDatahubAuditingService _auditingService;

    public ProjectCreationService(IConfiguration configuration, IDbContextFactory<DatahubProjectDBContext> datahubProjectDbFactory,
        ILogger<ProjectCreationService> logger, ServiceAuthManager serviceAuthManager, IUserInformationService userInformationService, 
        IResourceRequestService resourceRequestService, IDatahubAuditingService auditingService)
    {
        _configuration = configuration;
        _datahubProjectDbFactory = datahubProjectDbFactory;
        _logger = logger;
        this.serviceAuthManager = serviceAuthManager;
        _userInformationService = userInformationService;
        _resourceRequestService = resourceRequestService;
        _auditingService = auditingService;
    }

    public async Task<bool> AcronymExists(string acronym)
    {
        await using var db = await _datahubProjectDbFactory.CreateDbContextAsync();
        return await db.Projects.AnyAsync(p => p.Project_Acronym_CD == acronym);
    }
    public async Task<string> GenerateProjectAcronymAsync(string projectName)
    {
        await using var db = await _datahubProjectDbFactory.CreateDbContextAsync();
        var existingAcronyms = db.Projects.Select(p => p.Project_Acronym_CD).ToArray();
        return await GenerateProjectAcronymAsync(projectName, existingAcronyms);
    }

    public async Task<string> GenerateProjectAcronymAsync(string projectName, IEnumerable<string> existingAcronyms)
    {
        var words = projectName.Split(' ')
            .Select(w => new string(w.Where(char.IsLetterOrDigit).ToArray()))
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();
        var acronym = words.Length switch
        {
            1 => new string(words[0].Take(3).ToArray()).ToUpperInvariant(),
            2 => new string(words[0].Take(2).Concat(words[1].Take(2)).ToArray()).ToUpperInvariant(),
            _ => words.Select(w => w[0]).Aggregate("", (a, b) => a + b).ToUpperInvariant()
        };
        var enumerable = existingAcronyms.ToArray();
        if (!enumerable.Contains(acronym)) return acronym;
        var largestNumber = enumerable.Where(a => a.StartsWith(acronym)).
            Select(a => a.Length > acronym.Length && int.TryParse(a[acronym.Length..], out var n) ? n : 0
            ).Max();
        acronym += (largestNumber + 1).ToString();
        return await Task.FromResult(acronym);
    }

    public async Task<bool> CreateProjectAsync(string projectName, string organization)
    {
        var acronym = await GenerateProjectAcronymAsync(projectName);
        return await CreateProjectAsync(projectName, acronym, organization);
    }

    public async Task SaveProjectCreationDetailsAsync(string projectAcronym, string interestedFeatures)
    {
        await using var context = await _datahubProjectDbFactory.CreateDbContextAsync();
        var project = await context.Projects.FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);

        if (project is null)
        {
            _logger.LogError("Project with acronym {ProjectAcronym} not found", projectAcronym);
        }
        else
        {
            var user = await _userInformationService.GetCurrentPortalUserAsync();
            var newProjectCreationDetails = new ProjectCreationDetails
            {
                ProjectId = project.Project_ID,
                CreatedById = user.Id,
                InterestedFeatures = interestedFeatures
            };
            
            await context.ProjectCreationDetails.AddAsync(newProjectCreationDetails);
            await context.TrackSaveChangesAsync(_auditingService);
        }
    }

    public async Task<bool> CreateProjectAsync(string projectName, string? acronym, string organization)
    {
        using (var scope = new TransactionScope(
           TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                acronym ??= await GenerateProjectAcronymAsync(projectName);
                var sectorName = GovernmentDepartment.Departments.TryGetValue(organization, out var sector) ? sector : acronym;
                
                var user = await _userInformationService.GetCurrentGraphUserAsync();
                if (user is null) 
                    return false;

                await AddProjectToDb(user, projectName, acronym, organization);

                var project = CreateResourceData.NewProjectTemplate(projectName, acronym, sectorName, organization, 
                    user.Mail, Convert.ToDouble(GetDefaultBudget()));

                await _resourceRequestService.AddProjectToStorageQueue(project);
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating project {projectName} - {acronym} - {organization}");
                return false;
            }
        }
    }
    
    private async Task AddProjectToDb(User user, string projectName, string acronym, string organization) 
    {
        var sectorName = GovernmentDepartment.Departments.TryGetValue(organization, out var sector) ? sector : acronym;
        var project = new Datahub_Project()
        {
            Project_Acronym_CD = acronym,
            Project_Name = projectName,
            Sector_Name = sectorName,
            Contact_List = user.Mail,
            Project_Admin = user.Mail,
            Project_Phase = TerraformOutputStatus.PendingApproval,
            Project_Status_Desc = "Ongoing",
            Project_Status = (int)ProjectStatus.InProgress,
            Project_Budget = GetDefaultBudget()
        };
        await using var db = await _datahubProjectDbFactory.CreateDbContextAsync();
        await db.Projects.AddAsync(project);
        if (string.IsNullOrWhiteSpace(user.Id)) throw new InvalidOperationException("Cannot add user without ID");
        var portalUser = await _userInformationService.GetPortalUserAsync(user.Id);
        var role = Project_Role.GetAll()
            .First(r => r.Id == (int)Project_Role.RoleNames.WorkspaceLead);
            
        var projectUser = new Datahub_Project_User()
        {
            PortalUser = portalUser,
            Approved_DT = DateTime.Now,
            ApprovedPortalUser = portalUser,
            Project = project,
            Role = role
        };
        await db.Project_Users.AddAsync(projectUser);
        await db.TrackSaveChangesAsync(_auditingService);
        serviceAuthManager.InvalidateAuthCache();
    }

    private decimal GetDefaultBudget()
    {
        var value = _configuration.GetValue<int>("DefaultProjectBudget", 400);
        return Convert.ToDecimal(value);
    }
}