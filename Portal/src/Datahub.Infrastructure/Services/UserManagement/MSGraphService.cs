using Azure.Identity;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace Datahub.Infrastructure.Services.UserManagement;

public class MSGraphService : IMSGraphService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MSGraphService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IKeyVaultService _keyVaultService;
    private GraphServiceClient _graphServiceClient;

    public Dictionary<string, GraphUser> UsersDict { get; set; }

    public MSGraphService(IConfiguration configureOptions, ILogger<MSGraphService> logger, IHttpClientFactory clientFactory,
        IKeyVaultService keyVaultService)
    {
        _configuration = configureOptions;
        _logger = logger;
        _httpClientFactory = clientFactory;
        _keyVaultService = keyVaultService;
    }

    public async Task<GraphUser> GetUserAsync(string userId, CancellationToken token)
    {
        return await QueryUserAsync($"id eq '{userId}'", token);
    }

    public async Task<GraphUser> GetUserFromEmailAsync(string email, CancellationToken token)
    {
        return await QueryUserAsync($"mail eq '{email}'", token);
    }

    public async Task<string> GetUserName(string userId, CancellationToken token)
    {
        if (!string.IsNullOrWhiteSpace(userId))
        {
            var user = await GetUserAsync(userId, token);
            return user?.DisplayName ?? "...";
        }
        return "...";
    }

    public async Task<string> GetUserEmail(string userId, CancellationToken token)
    {
        var user = await GetUserAsync(userId, token);
        return user?.Mail;
    }

    public async Task<string> GetUserIdFromEmailAsync(string email, CancellationToken token)
    {
        var user = await GetUserFromEmailAsync(email, token);
        return user?.Id ?? string.Empty;
    }

    public async Task<Dictionary<string, GraphUser>> GetUsersListAsync(string filterText, CancellationToken token)
    {
        Dictionary<string, GraphUser> users = new();
        PrepareAuthenticatedClient();

        var usersPage = await _graphServiceClient.Users.GetAsync(
            requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Filter = $"startswith(mail,'{filterText}')";
                requestConfiguration.QueryParameters.Count = true;
                requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");                
            },token
            );

        var pageIterator = PageIterator<User, UserCollectionResponse>.CreatePageIterator(
            _graphServiceClient,
            usersPage,
            // Callback executed for each item in
            // the collection
            (user) =>
            {
                var newUser = GraphUser.Create(user);
                users.Add(newUser.Id, newUser);
                return true;
            });

        await pageIterator.IterateAsync();

        return users;
    }

    private void PrepareAuthenticatedClient()
    {
        if (_graphServiceClient == null)
        {
            try
            {
                //see https://learn.microsoft.com/en-us/graph/sdks/choose-authentication-providers?tabs=csharp
                // using Azure.Identity;
                var options = new ClientSecretCredentialOptions
                {
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
                };
                var clientCertCredential = new ClientSecretCredential(
                    _configuration.GetSection("AzureAd").GetValue<string>("TenantId"),
                    _configuration.GetSection("AzureAd").GetValue<string>("ClientId"), 
                    _configuration.GetSection("AzureAd").GetValue<string>("ClientSecret"), options);
                var httpClient = _httpClientFactory.CreateClient();
                _graphServiceClient = new(httpClient, clientCertCredential);


            }
            catch (Exception e)
            {
                _logger.LogError($"Error preparing authentication client: {e.Message}");
                throw;
            }
        }
    }

    private async Task<GraphUser> QueryUserAsync(string filter, CancellationToken token)
    {
        PrepareAuthenticatedClient();
        try
        {
            var user = await _graphServiceClient.Users.GetAsync(
                requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Filter = filter;
                }, token
            );
            
            
            return user.Value == null ? null : GraphUser.Create(user.Value[0]);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching user: {filter}: {ex.Message}");
        }
        return new();
    }

    public async Task<GraphUser> GetUserFromSamAccountNameAsync(string userName, CancellationToken token)
    {
        PrepareAuthenticatedClient();
        try
        {
            var user = await _graphServiceClient.Users.GetAsync(
                requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Search = $"\"onPremisesSamAccountName:{userName}\"";
                    requestConfiguration.QueryParameters.Select = new[] { "mail", "department" };
                    requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                }, token
            );


            return user.Value == null ? null : GraphUser.Create(user.Value[0]);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching user: {userName}: {ex.Message}");
        }
        return new();
    }
}