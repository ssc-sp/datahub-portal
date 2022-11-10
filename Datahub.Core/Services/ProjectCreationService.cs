using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Datahub.Core.Data.ResourceProvisioner;
using Datahub.Core.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Core.Services;

public class ProjectCreationService : IProjectCreationService
{
    
    private readonly HttpClient _httpClient;
    private readonly IDbContextFactory<DatahubProjectDBContext> _datahubProjectDbFactory;
    private readonly IUserInformationService _userInformationService;

    private const string NewProjectDataSensitivity = "Setup";
    public const string ResourceRunEndpoint = "ResourceRun";
    private static readonly ResourceTemplate CreateResourceTemplate = new()
    {
        Name = "new-project-template",
        Version = "latest",
    };
    
    public ProjectCreationService(HttpClient httpClient,
        IDbContextFactory<DatahubProjectDBContext> datahubProjectDbFactory,
        IUserInformationService userInformationService)
    {
        _httpClient = httpClient;
        _datahubProjectDbFactory = datahubProjectDbFactory;
        _userInformationService = userInformationService;
    }
    public async Task<string> GenerateProjectAcronymAsync(string projectName)
    {
        await using var db = await _datahubProjectDbFactory.CreateDbContextAsync();
        var existingAcronyms = db.Projects.Select(p => p.Project_Acronym_CD).ToArray();
        return await GenerateProjectAcronymAsync(projectName, existingAcronyms);
    }

    public async Task<string> GenerateProjectAcronymAsync(string projectName, IEnumerable<string> existingAcronyms)
    {
        var words = projectName.Split(' ');
        var acronym = words.Length switch
        {
            1 => words[0][..3].ToUpperInvariant(),
            2 => string.Concat(words[0].AsSpan(0, 2), words[1].AsSpan(0, 2)).ToUpperInvariant(),
            _ => words.Select(w => w[0]).Aggregate("", (a, b) => a + b).ToUpperInvariant()
        };
        var enumerable = existingAcronyms as string[] ?? existingAcronyms.ToArray();
        if (!enumerable.Contains(acronym)) return acronym;
        var largestNumber = enumerable.Where(a => a.StartsWith(acronym)).
            Select(a => a.Length > acronym.Length && int.TryParse(a[acronym.Length..], out var n) ? n : 0
            ).Max();
        acronym += (largestNumber + 1).ToString();
        return await Task.FromResult(acronym);
    }

    public async Task CreateProjectAsync(string projectName, string organization)
    {
        var acronym = await GenerateProjectAcronymAsync(projectName);
        await CreateProjectAsync(projectName, acronym, organization);
    }
    public async Task CreateProjectAsync(string projectName, string acronym, string organization)
    {
        acronym ??= await GenerateProjectAcronymAsync(projectName);
        var requestSuccessful = await SendProvisionerRequestAsync(projectName, acronym, organization);
        if (requestSuccessful)
        {
            var user = await _userInformationService.GetAuthenticatedUser();
            await CreateProjectAsync(projectName, acronym, organization, user.Identity?.Name);
        }
    }
    private async Task<bool> SendProvisionerRequestAsync(string projectName, string acronym, string organization)
    {
        var requestBody = new CreateResourceData
        {
            Templates = new List<ResourceTemplate>() { CreateResourceTemplate },
            Workspace = new ResourceWorkspace()
            {
                Name = projectName,
                Acronym = acronym
            }
        };
        var response = await _httpClient.PostAsJsonAsync(ResourceRunEndpoint, requestBody);
        return response.IsSuccessStatusCode;
    }
    private async Task CreateProjectAsync(string projectName, string acronym, string organization, string userEmail) 
    {
        var project = new Datahub_Project()
        {
            Project_Acronym_CD = acronym,
            Project_Name = projectName,
            Branch_Name = organization,
            Contact_List = userEmail,
            Project_Admin = userEmail,
            Data_Sensitivity = NewProjectDataSensitivity,
        };
        await using var db = await _datahubProjectDbFactory.CreateDbContextAsync();
        await db.Projects.AddAsync(project);
        await db.SaveChangesAsync();
    }
    
}