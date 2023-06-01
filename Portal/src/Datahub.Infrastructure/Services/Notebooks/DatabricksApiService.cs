using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Application.Services.Notebooks;
using Datahub.Core.Data.Databricks;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Repositories;
using Datahub.ProjectTools.Utils;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Rest;

namespace Datahub.Infrastructure.Services.Notebooks;

public class DatabricksApiService : IDatabricksApiService
{
    private readonly ILogger<DatabricksApiService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;

    public DatabricksApiService(
        ILogger<DatabricksApiService> logger,
        IHttpClientFactory httpClientFactory,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _dbContextFactory = dbContextFactory;
    }

 

    public async Task<List<RepositoryInfo>> ListWorkspaceRepositories(string projectAcronym, string accessToken)
    {
        var workspaceDatabricksUrl = await GetWorkspaceDatabricksUrl(projectAcronym);

        // Use the access token to call a protected web API.
        var httpClient = _httpClientFactory.CreateClient();
        var queryUrl = $"{workspaceDatabricksUrl}/api/2.0/repos";
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await httpClient.GetAsync(queryUrl);
        
        response.EnsureSuccessStatusCode();
        var contentString = await response.Content.ReadAsStringAsync();
        // var contentString = Stub();
        var content = JsonSerializer.Deserialize<JsonObject>(contentString);
        
        var existingRepositoryPreferences = await GetWorkspaceRepositoryVisibilitiesAsync(projectAcronym);

        var results = content?["repos"]?
            .AsArray()
            .Select(node => new RepositoryInfo(node))
            .ToList() ?? new List<RepositoryInfo>();

        foreach (var repositoryInfo in results)
        {
            if (existingRepositoryPreferences.TryGetValue(repositoryInfo.Url, out var publicPreference))
            {
                repositoryInfo.IsPublic = publicPreference;
            }
        }

        return results;
    }

    public async Task<bool> SetWorkspaceRepositoryVisibility(string projectAcronym, string repositoryUrl, bool isPublic)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        var project = await dbContext.Projects
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);

        if (project == null)
        {
            _logger.LogError("Project with acronym {ProjectAcronym} not found", projectAcronym);
            return false;
        }
        
        var projectRepository = await dbContext.ProjectRepositories
            .FirstOrDefaultAsync(pr => pr.ProjectId == project.Project_ID 
                                       && pr.RepositoryUrl == repositoryUrl);

        if (projectRepository == null)
        {
            projectRepository = new ProjectRepository()
            {
                Project = project,
                RepositoryUrl = repositoryUrl,
                IsPublic = isPublic
            };
            
            dbContext.ProjectRepositories.Add(projectRepository);
        }
        
        projectRepository.IsPublic = isPublic;
        
        await dbContext.SaveChangesAsync();

        return true;
    }

    private string Stub()
    {
        return @"{
        ""repos"": [
        {
            ""id"": 1551602177705819,
            ""path"": ""/Repos/sean.stilwell@ssc-spc.gc.ca/radarsat1-scripts"",
            ""url"": ""https://github.com/ssc-sp/radarsat1-scripts.git"",
            ""provider"": ""gitHub"",
            ""branch"": ""main"",
            ""head_commit_id"": ""ef0a2dae3e4318acd8d5e614da0cf9ffc3e6497d""
        },
        {
            ""id"": 2809546570892951,
            ""path"": ""/Repos/david.rene@ssc-spc.gc.ca/Google-API-on-Databricks"",
            ""url"": ""https://github.com/davidreneuw/Google-API-on-Databricks.git"",
            ""provider"": ""gitHub"",
            ""branch"": ""master"",
            ""head_commit_id"": ""b8832dc6a3af7cc076947167a7aa9129dddef338""
        },
        {
            ""id"": 4027945598515864,
            ""path"": ""/Repos/sean.stilwell@ssc-spc.gc.ca/datahub-demos"",
            ""url"": ""https://github.com/ssc-sp/datahub-demos.git"",
            ""provider"": ""gitHub"",
            ""branch"": ""main"",
            ""head_commit_id"": ""79550b79fc4624cc6b67d8ac05267ff596b985e3""
        }
        ]
    }";
    }

    private async Task<Dictionary<string, bool>> GetWorkspaceRepositoryVisibilitiesAsync(string projectAcronym)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var project = await dbContext.Projects
            .Include(p => p.Resources)
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);
        
        if (project == null)
        {
            _logger.LogError("Project with acronym {ProjectAcronym} not found", projectAcronym);
            throw new ArgumentException($"Project with acronym {projectAcronym} not found");
        }
        
        return await dbContext.ProjectRepositories
            .Where(pr => pr.ProjectId == project.Project_ID)
            .ToDictionaryAsync(pr => pr.RepositoryUrl, pr => pr.IsPublic);
    }

    private async Task<string> GetWorkspaceDatabricksUrl(string projectAcronym)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var project = await dbContext.Projects
            .Include(p => p.Resources)
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);

        if (project == null)
        {
            _logger.LogError("Project with acronym {ProjectAcronym} not found", projectAcronym);
            throw new ArgumentException($"Project with acronym {projectAcronym} not found");
        }

        var databricksUrl = TerraformVariableExtraction.ExtractDatabricksUrl(project);
        if (databricksUrl == null)
        {
            _logger.LogError("Databricks url not found for project {ProjectAcronym}", projectAcronym);
            throw new ArgumentException($"Databricks url not found for project {projectAcronym}");
        }

        return databricksUrl;
    }
}