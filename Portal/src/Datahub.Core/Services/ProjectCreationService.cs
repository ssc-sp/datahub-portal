#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Datahub.Core.Data;
using Datahub.Core.Data.ResourceProvisioner;
using Datahub.Core.Enums;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.ResourceManager;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.Extensions.Logging;

namespace Datahub.Core.Services;

public class ProjectCreationService : IProjectCreationService
{
    
    private readonly IDbContextFactory<DatahubProjectDBContext> _datahubProjectDbFactory;
    private readonly ILogger<ProjectCreationService> logger;
    private readonly IUserInformationService _userInformationService;
    private readonly RequestQueueService requestQueueService;
    private const string NewProjectDataSensitivity = "Setup";
    
    public ProjectCreationService(IDbContextFactory<DatahubProjectDBContext> datahubProjectDbFactory,
        ILogger<ProjectCreationService> logger,
        IUserInformationService userInformationService, RequestQueueService requestQueueService)
    {
        _datahubProjectDbFactory = datahubProjectDbFactory;
        this.logger = logger;
        _userInformationService = userInformationService;
        this.requestQueueService = requestQueueService;
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
        var words = projectName.Split(' ').Select(w => new string(w.Where(char.IsLetterOrDigit).ToArray())).ToArray();
        var acronym = words.Length switch
        {
            1 => words[0][..3].ToUpperInvariant(),
            2 => string.Concat(words[0].AsSpan(0, 2), words[1].AsSpan(0, 2)).ToUpperInvariant(),
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
                if (user is null) return false;
                await AddProjectToDb(user, projectName, acronym, organization);
                var project = CreateResourceData.NewProjectTemplate(projectName, acronym, sectorName, organization, user.Mail);
                await requestQueueService.AddProjectToStorageQueue(project);
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error creating project {projectName} - {acronym} - {organization}");
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
        };
        var projectUser = new Datahub_Project_User()
        {
            User_ID = user.Id,
            Approved_DT = DateTime.Now,
            ApprovedUser = user.Id,
            IsAdmin = true,
            IsDataApprover = true,
            Project = project,
            User_Name = user.Mail
        };
        await using var db = await _datahubProjectDbFactory.CreateDbContextAsync();
        await db.Projects.AddAsync(project);
        await db.Project_Users.AddAsync(projectUser);
        await db.SaveChangesAsync();
    }



}