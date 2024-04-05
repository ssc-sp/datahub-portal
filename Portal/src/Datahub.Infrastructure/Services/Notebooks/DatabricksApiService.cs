using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Application.Services.Notebooks;
using Datahub.Core.Data.Databricks;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Repositories;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Core.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services.Notebooks;

public class DatabricksApiService : IDatabricksApiService
{
    private readonly ILogger<DatabricksApiService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
    private readonly IDatahubCatalogSearch _datahubCatalogSearch;

    public DatabricksApiService(
        ILogger<DatabricksApiService> logger,
        IHttpClientFactory httpClientFactory,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        IDatahubCatalogSearch datahubCatalogSearch)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _dbContextFactory = dbContextFactory;
        _datahubCatalogSearch = datahubCatalogSearch;
    }


    public async Task<List<ProjectRepository>> ListDisplayedWorkspaceRepositoriesAsync(string projectAcronym)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var project = await dbContext.Projects
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);

        if (project == null)
        {
            _logger.LogError("Project with acronym {ProjectAcronym} not found", projectAcronym);
            return new List<ProjectRepository>();
        }

        var results = await dbContext.ProjectRepositories
            .Where(pr => pr.ProjectId == project.Project_ID
                         && pr.IsPublic)
            .ToListAsync();

        return results;
    }

    public async Task<List<RepositoryInfoDto>> ListWorkspaceRepositoriesAsync(string projectAcronym, string accessToken)
    {
        var workspaceDatabricksUrl = await GetWorkspaceDatabricksUrl(projectAcronym);

        // Use the access token to call a protected web API.
        var httpClient = _httpClientFactory.CreateClient();
        var queryUrl = $"{workspaceDatabricksUrl}/api/2.0/repos";
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await httpClient.GetAsync(queryUrl);

        response.EnsureSuccessStatusCode();
        var contentString = await response.Content.ReadAsStringAsync();
        var content = JsonSerializer.Deserialize<JsonObject>(contentString);

        var existingRepositoryPreferences = await GetWorkspaceRepositoryVisibilitiesAsync(projectAcronym);

        var results = content?["repos"]?
            .AsArray()
            .Select(node => new RepositoryInfoDto(node))
            .ToList() ?? new List<RepositoryInfoDto>();

        foreach (var repositoryInfo in results)
        {
            if (existingRepositoryPreferences.TryGetValue(repositoryInfo.Url, out var publicPreference))
            {
                repositoryInfo.IsPublic = publicPreference;
            }
        }

        return results;
    }

    public async Task<bool> UpdateWorkspaceRepository(string projectAcronym, RepositoryInfoDto repositoryInfoDto)
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
                                       && pr.RepositoryUrl == repositoryInfoDto.Url);

        if (projectRepository == null)
        {
            projectRepository = new ProjectRepository();
            dbContext.ProjectRepositories.Add(projectRepository);
        }
            
        projectRepository.Project = project;
        projectRepository.RepositoryUrl = repositoryInfoDto.Url;
        projectRepository.IsPublic = repositoryInfoDto.IsPublic;
        projectRepository.Branch = repositoryInfoDto.Branch;
        projectRepository.HeadCommitId = repositoryInfoDto.HeadCommitId;
        projectRepository.Provider = repositoryInfoDto.Provider;
        projectRepository.Path = repositoryInfoDto.Path;
        
        await dbContext.SaveChangesAsync();

        var catalogObject = new Core.Model.Catalog.CatalogObject()
        {
            ObjectType = Core.Model.Catalog.CatalogObjectType.Repository,
            ObjectId = project.Project_Acronym_CD,
            Name_English = project.Project_Name,
            Name_French = project.Project_Name_Fr,
            Desc_English = project.Project_Summary_Desc,
            Desc_French = project.Project_Summary_Desc_Fr
        };

        await _datahubCatalogSearch.AddCatalogObject(catalogObject);

        return true;
    }

    public async Task<bool> AddAdminToDatabricsWorkspaceAsync(string projectAcronym, string accessToken, string databricksUserId)
    {
        var workspaceDatabricksUrl = await GetWorkspaceDatabricksUrl(projectAcronym);

        // Use the access token to call a protected web API.
        var httpClient = _httpClientFactory.CreateClient();
        var queryUrl = $"{workspaceDatabricksUrl}/api/2.0/preview/scim/v2/Users/{databricksUserId}";
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var patchContent = BuildPatchBody(databricksUserId);        

        using var response = await httpClient.PatchAsync(queryUrl, patchContent);

        response.EnsureSuccessStatusCode();

        return response.StatusCode == System.Net.HttpStatusCode.OK;
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

    private StringContent BuildPatchBody(string databricksUserId)
    {
        _logger.LogInformation($"Building request patch body for adding user {databricksUserId} to databricks admin group");

        var patchData = new UserPatchRequest
        {
            Schemas = ["urn:ietf:params:scim:api:messages:2.0:PatchOp"],
            Operations = 
            [
                new UserPatchOperation
                {
                    Op="add",
                    Path="admins",
                    Value = new DatabricksUser
                    { 
                        id = databricksUserId
                    }
                }
            ]
        };
        var patchBody = new StringContent(JsonSerializer.Serialize(patchData), Encoding.UTF8, "application/json");
        return patchBody;
    }
}