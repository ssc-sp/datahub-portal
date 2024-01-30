#nullable enable
using System.Transactions;
using Datahub.Application.Services;
using Datahub.Core.Data;
using Datahub.Core.Data.ResourceProvisioner;
using Datahub.Core.Enums;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Onboarding;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Core.Services.Security;
using Datahub.ProjectTools.Services;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;

namespace Datahub.Infrastructure.Services;

public class ProjectCreationService : IProjectCreationService
{
    private readonly IConfiguration _configuration;
    private readonly IDbContextFactory<DatahubProjectDBContext> _datahubProjectDbFactory;
    private readonly ILogger<ProjectCreationService> _logger;
    private readonly ServiceAuthManager _serviceAuthManager;
    private readonly IUserInformationService _userInformationService;
    private readonly IResourceMessagingService _resourceMessagingService;
    private readonly IDatahubAuditingService _auditingService;
    private readonly IDatahubCatalogSearch _datahubCatalogSearch;

    public ProjectCreationService(IConfiguration configuration, IDbContextFactory<DatahubProjectDBContext> datahubProjectDbFactory,
        ILogger<ProjectCreationService> logger, ServiceAuthManager serviceAuthManager, IUserInformationService userInformationService,
        IResourceMessagingService resourceMessagingService, IDatahubAuditingService auditingService, IDatahubCatalogSearch datahubCatalogSearch)
    {
        _configuration = configuration;
        _datahubProjectDbFactory = datahubProjectDbFactory;
        _logger = logger;
        _serviceAuthManager = serviceAuthManager;
        _userInformationService = userInformationService;
        _resourceMessagingService = resourceMessagingService;
        _auditingService = auditingService;
        _datahubCatalogSearch = datahubCatalogSearch;
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
                InterestedFeatures = interestedFeatures,
                CreatedAt = DateTime.UtcNow
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
                
                var currentPortalUser = await _userInformationService.GetCurrentPortalUserAsync();
                if (currentPortalUser is null) 
                    return false;

                await AddProjectToDb(currentPortalUser, projectName, acronym, organization);
                
                await CreateNewTemplateProjectResourceAsync(acronym); 
                
                var workspaceDefinition = await _resourceMessagingService.GetWorkspaceDefinition(acronym, currentPortalUser.Email);
                
                await _resourceMessagingService.SendToTerraformQueue(workspaceDefinition);
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

    public async Task CreateNewTemplateProjectResourceAsync(string projectAcronym)
    {
        await using var context = await _datahubProjectDbFactory.CreateDbContextAsync();
        var project = await context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);
        
        if (project is null)
        {
            _logger.LogError("Project with acronym {ProjectAcronym} not found", projectAcronym);
        }
        else
        {
            await CreateNewTemplateProjectResourceAsync(project.Project_ID);
        }
    }
    public async Task CreateNewTemplateProjectResourceAsync(int projectId)
    {
        await using var context = await _datahubProjectDbFactory.CreateDbContextAsync();
        
        var exists = context.Project_Resources2
            .Any(r => r.ProjectId == projectId
                      && r.ResourceType == TerraformTemplate.GetTerraformServiceType(TerraformTemplate.NewProjectTemplate));
        
        if (exists) return;
        
        var project = await context.Projects
            .FirstAsync(p => p.Project_ID == projectId);
        var currentPortalUser = await _userInformationService.GetCurrentPortalUserAsync();

        var newResource = new Project_Resources2
        {
            ProjectId = project.Project_ID,
            RequestedById = currentPortalUser.Id,
            ResourceType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.NewProjectTemplate)
        };
        
        await context.Project_Resources2.AddAsync(newResource);
        await context.TrackSaveChangesAsync(_auditingService);
    }
    
    private async Task AddProjectToDb(PortalUser portalUser, string projectName, string acronym, string organization) 
    {
        var sectorName = GovernmentDepartment.Departments.TryGetValue(organization, out var sector) ? sector : acronym;
        var project = new Datahub_Project()
        {
            Project_Acronym_CD = acronym,
            Project_Name = projectName,
            Project_Name_Fr = projectName,
            Sector_Name = sectorName,
            Contact_List = portalUser.Email,
            Project_Admin = portalUser.Email,
            Project_Phase = TerraformOutputStatus.PendingApproval,
            Project_Status_Desc = "Ongoing",
            Project_Status = (int)ProjectStatus.InProgress,
            Project_Budget = GetDefaultBudget()
        };
        await using var db = await _datahubProjectDbFactory.CreateDbContextAsync();
        await db.Projects.AddAsync(project);
        
        var role = Project_Role.GetAll()
            .First(r => r.Id == (int)Project_Role.RoleNames.WorkspaceLead);
            
        var projectUser = new Datahub_Project_User()
        {
            PortalUserId = portalUser.Id,
            Approved_DT = DateTime.Now,
            ApprovedPortalUserId = portalUser.Id,
            Project = project,
            RoleId = role.Id
        };
        await db.Project_Users.AddAsync(projectUser);
        
        var projectWhiteList = new Project_Whitelist()
        {
            Project = project, 
            LastUpdated = DateTime.UtcNow,
            AllowStorage = true,
            AllowDatabricks = true
        };
        await db.Project_Whitelists.AddAsync(projectWhiteList);
        
        await db.TrackSaveChangesAsync(_auditingService);
        _serviceAuthManager.InvalidateAuthCache();

        var catalogObject = new Core.Model.Catalog.CatalogObject()
        {
            ObjectType = Core.Model.Catalog.CatalogObjectType.Workspace,
            ObjectId = acronym,
            Name_English = projectName,
            Name_French = projectName,
            Desc_English = organization,
            Desc_French = organization
        };

        await _datahubCatalogSearch.AddCatalogObject(catalogObject);
    }

    private decimal GetDefaultBudget()
    {
        var value = _configuration.GetValue<int>("DefaultProjectBudget", 100);
        return Convert.ToDecimal(value);
    }
}