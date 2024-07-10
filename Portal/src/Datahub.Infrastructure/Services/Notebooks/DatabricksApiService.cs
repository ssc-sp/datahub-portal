using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Datahub.Application.Services.Notebooks;
using Datahub.Core.Data.Databricks;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
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

    public async Task<string> GetDatabricsWorkspaceUrlAsync(string projectAcronym)
    {
        try
        {
            var workspaceDatabricksUrl = await GetWorkspaceDatabricksUrl(projectAcronym);
            return workspaceDatabricksUrl;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public async Task<bool> AddAdminToDatabricsWorkspaceAsync(AccessToken accessToken, string projectAcronym, PortalUser user)
    {
        try
        {
            var workspaceDatabricksUrl = await GetWorkspaceDatabricksUrl(projectAcronym);

            // Use the access token to call a protected web API.
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);

            // first check if user exists
            var searchResults = await GetUserByName(accessToken, workspaceDatabricksUrl, user.DisplayName);
            if (searchResults?.totalResults > 0)
            {
                var databricksUserId = searchResults.Resources[0].id;
                // need to delete
                await DeleteUser(accessToken, workspaceDatabricksUrl,databricksUserId);
            }
            var postUrl = $"{workspaceDatabricksUrl}/api/2.0/preview/scim/v2/Users";
            var postContent = BuilPostBody(user);             

            using (var userData = await httpClient.PostAsync(postUrl, postContent))
            {
                if (userData.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    return false;
                }
                var newUserData = await userData.Content.ReadAsStringAsync();
                var databricksUser = JsonSerializer.Deserialize<DatabricksUser>(newUserData);
                var databricksUserId = databricksUser?.id;

                return databricksUserId != null;
            }
        }
        catch(Exception)
        {
            return false;
        }        
    }

    private async Task<DatabricksUserList?> GetUserByName(AccessToken accessToken, string workspaceDatabricksUrl, string displayName)
    {
        var filter = $"?filter=displayName eq {displayName}";
        var queryUrl = $"{workspaceDatabricksUrl}/api/2.0/preview/scim/v2/Users/{filter}";
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);

        using var response = await httpClient.GetAsync(queryUrl);

        response.EnsureSuccessStatusCode();
        var searchResults = await response.Content.ReadAsStringAsync();

        return string.IsNullOrEmpty(searchResults)
            ? new DatabricksUserList()
            : JsonSerializer.Deserialize<DatabricksUserList>(searchResults);
    }

    private async Task DeleteUser(AccessToken accessToken, string workspaceDatabricksUrl, string databricksUserId)
    {
        var deleteUrl = $"{workspaceDatabricksUrl}/api/2.0/preview/scim/v2/Users/{databricksUserId}";
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token); 

        using var response = await httpClient.DeleteAsync(deleteUrl);
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

    private StringContent BuilPostBody(PortalUser user)
    {
        _logger.LogInformation($"Building request patch body for adding user  to databricks admin group");

        var databricksUser = new DatabricksUser
        {
            userName = user.Email,
            name = new Name { familyName = user.DisplayName },
            id = "0",
            active = true,
            emails =
            [
                new Email { primary = true, value = user.Email, type = "work", display=user.DisplayName }
            ],
            groups =
            [
                new Group { value="admins"} 
            ]
        };
        var postBody = new StringContent(JsonSerializer.Serialize(databricksUser), Encoding.UTF8, "application/json");
        return postBody;
    }
}