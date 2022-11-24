#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Datahub.Core.Data;
using Datahub.Core.Data.ResourceProvisioner;
using Datahub.Core.EFCore;
using Foundatio.Queues;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;

namespace Datahub.Core.Services;

public class ProjectCreationService : IProjectCreationService
{
    
    private readonly HttpClient _httpClient;
    private readonly IDbContextFactory<DatahubProjectDBContext> _datahubProjectDbFactory;
    private readonly IUserInformationService _userInformationService;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly IConfiguration _configuration;

    private const string NewProjectDataSensitivity = "Setup";
    private const string ResourceRunEndpoint = "api/ResourceRun";
    
    public ProjectCreationService(HttpClient httpClient,
        IDbContextFactory<DatahubProjectDBContext> datahubProjectDbFactory,
        IUserInformationService userInformationService, ITokenAcquisition tokenAcquisition, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _datahubProjectDbFactory = datahubProjectDbFactory;
        _userInformationService = userInformationService;
        _tokenAcquisition = tokenAcquisition;
        _configuration = configuration;
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
        acronym ??= await GenerateProjectAcronymAsync(projectName);
        var sectorName = GovernmentDepartment.Departments.TryGetValue(organization, out var sector) ? sector : acronym;
        var user = await _userInformationService.GetAuthenticatedUser();
        var scopes = _configuration["ResourceProvisionerApi:Scopes"].Split(' ');
        var token = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
        await AddProjectToDb(projectName, acronym, organization, user.Identity?.Name);
        var project = new CreateResourceData(projectName, acronym, sectorName, organization);
        await AddProjectToStorageQueue(project);
        return await SendProvisionerRequestAsync(project, token);
    }
    private async Task<bool> SendProvisionerRequestAsync(CreateResourceData project, string token)
    {
        var stringContent = new StringContent(JsonSerializer.Serialize(project), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        var response = await _httpClient.PostAsync(ResourceRunEndpoint, stringContent);
        return response.IsSuccessStatusCode;
    }
    private async Task AddProjectToDb(string projectName, string acronym, string organization, string? userEmail) 
    {
        var sectorName = GovernmentDepartment.Departments.TryGetValue(organization, out var sector) ? sector : acronym;
        var project = new Datahub_Project()
        {
            Project_Acronym_CD = acronym,
            Project_Name = projectName,
            Sector_Name = sectorName,
            Contact_List = userEmail,
            Project_Admin = userEmail,
            Data_Sensitivity = NewProjectDataSensitivity,
            Project_Status_Desc = "Test",
        };
        await using var db = await _datahubProjectDbFactory.CreateDbContextAsync();
        await db.Projects.AddAsync(project);
        await db.SaveChangesAsync();
    }


    private async Task AddProjectToStorageQueue(CreateResourceData project)
    {
        using IQueue<CreateResourceData> queue = new AzureStorageQueue<CreateResourceData>(new AzureStorageQueueOptions<CreateResourceData>()
        {
            ConnectionString = _configuration["ProjectCreationQueue:ConnectionString"],
            Name = _configuration["ProjectCreationQueue:Name"],
        });
        await queue.EnqueueAsync(project);
    }

}