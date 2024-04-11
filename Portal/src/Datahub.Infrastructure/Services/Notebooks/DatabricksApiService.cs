using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Datahub.Application.Services.Notebooks;
using Datahub.Core.Data.Databricks;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Repositories;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Core.Utils;
using Datahub.Infrastructure.Services.Azure;
using Datahub.Shared.Clients;
using Datahub.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services.Notebooks;

public class DatabricksApiService : IDatabricksApiService
{
    const string DATABRICKS_SCOPE = "2ff814a6-3304-4ab8-85cb-cd0e6f879c1d/.default";
    private readonly ILogger<DatabricksApiService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
    private readonly IDatahubCatalogSearch _datahubCatalogSearch;
    private readonly AzureManagementService _azureManagementService;
    private readonly IAzureServicePrincipalConfig _configuration;


    public DatabricksApiService(
        ILogger<DatabricksApiService> logger,
        IHttpClientFactory httpClientFactory,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        IDatahubCatalogSearch datahubCatalogSearch,
        IAzureServicePrincipalConfig configuration,
        AzureManagementService azureManagementService)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _dbContextFactory = dbContextFactory;
        _datahubCatalogSearch = datahubCatalogSearch;
        _configuration = configuration;
        _azureManagementService = azureManagementService;
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

    public async Task<bool> AddAdminToDatabricsWorkspaceAsync(string projectAcronym, PortalUser user)
    {
        try
        {
            var workspaceDatabricksUrl = await GetWorkspaceDatabricksUrl(projectAcronym);

            // Use the access token to call a protected web API.
            var accessToken = await GetAccessToken();
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);

            var postUrl = $"{workspaceDatabricksUrl}/api/2.0/preview/scim/v2/Users";
            var postContent = BuilPostBody(user);             

            using (var userData = await httpClient.PostAsync(postUrl, postContent))
            {
                if (userData.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return false;
                }
                var newUserData = await userData.Content.ReadAsStringAsync();
                var databricksUser = JsonSerializer.Deserialize<dynamic>(newUserData);
                var databricksUserId = databricksUser?.Id;
                var queryUrl = $"{workspaceDatabricksUrl}/api/2.0/preview/scim/v2/Users/{databricksUserId}";
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);

                var patchContent = BuildPatchBody(databricksUserId);

                using var response = await httpClient.PatchAsync(postUrl, postContent);

                response.EnsureSuccessStatusCode();

                return response.StatusCode == System.Net.HttpStatusCode.OK;
            }
        }
        catch(Exception ex)
        {
            return false;
        }        
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

    private Task<AccessToken> GetAccessToken()
    {
        var _config = new AzureDevOpsConfiguration();
        _config.TenantId =_configuration.TenantId;
        _config.ClientId = _configuration.ClientId;
        _config.ClientSecret = _configuration.ClientSecret;
        var clientProvider = new AzureDevOpsClient(_config);
        return clientProvider.GetAccessToken(DATABRICKS_SCOPE);
    }

    private StringContent BuilPostBody(PortalUser user)
    {
        _logger.LogInformation($"Building request patch body for adding user  to databricks admin group");

        var newUser = new
        {
            schemas = new[] { "urn:ietf:params:scim:schemas:core:2.0:User" },
            userName = user.Email,
            name = new
            {
                familyName = user.DisplayName
            },
            emails = new[]
            {
                new
                {
                    value = user.Email,
                    type = "work",
                    primary = true
                }
            }
        };
        var postBody = new StringContent(JsonSerializer.Serialize(newUser), Encoding.UTF8, "application/json");
        return postBody;
    }
    private StringContent BuildPatchBody(string databricksUserId)
    {
        _logger.LogInformation($"Building request patch body for adding user {databricksUserId} to databricks admin group");

        var patchData = new 
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            operations = new[]
            {
                new
                {
                    op="add",
                    path="admins",
                    value = new 
                    {
                        id = databricksUserId
                    }
                }
            }
        };
        var patchBody = new StringContent(JsonSerializer.Serialize(patchData), Encoding.UTF8, "application/json");
        return patchBody;
    }
}