using Datahub.Application.Services.Security;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data;
using Datahub.Infrastructure.Services.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace Datahub.Infrastructure.Services.UserManagement;

public class MSGraphService(ILogger<MSGraphService> logger, IHttpClientFactory clientFactory,
    ISystemTokenCredentialService systemTokenCredentialService) : IMSGraphService
{
    private readonly ILogger<MSGraphService> _logger = logger;
    private readonly IHttpClientFactory _httpClientFactory = clientFactory;
    private readonly ISystemTokenCredentialService _systemTokenCredentialService = systemTokenCredentialService;
    private GraphServiceClient _graphServiceClient = null!;

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
        var client = GetAuthenticatedClient();

        var usersPage = await client.Users.GetAsync(
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

    public GraphServiceClient GetAuthenticatedClient()
    {
        if (_graphServiceClient == null)
        {
            try
            {
                _graphServiceClient = new(_systemTokenCredentialService.GetTokenCredential());
            }
            catch (Exception e)
            {
                _logger.LogError($"Error preparing authentication client: {e.Message}");
                throw;
            }
        }
        return _graphServiceClient;
    }

    private async Task<GraphUser> QueryUserAsync(string filter, CancellationToken token)
    {
        var client = GetAuthenticatedClient();
        try
        {
            var user = await client.Users.GetAsync(
                requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Filter = filter;
                }, token
            );
            
            if (user is null || user.Value is null || user.Value.Count == 0)
            {
                throw new InvalidOperationException($"User not found: {filter}");
            }
            return GraphUser.Create(user.Value[0]);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching user: {filter}: {ex.Message}");
            throw;
        }        
    }

    public async Task<GraphUser> GetUserFromSamAccountNameAsync(string userName, CancellationToken token)
    {
        var client = GetAuthenticatedClient();
        try
        {
            var user = await client.Users.GetAsync(
                requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Search = $"\"onPremisesSamAccountName:{userName}\"";
                    requestConfiguration.QueryParameters.Select = new[] { "mail", "department" };
                    requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                }, token
            );
            if (user is null || user.Value is null || user.Value.Count == 0)
            {
                throw new InvalidOperationException($"User not found: {userName}");
            }

            return GraphUser.Create(user.Value[0]);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching user: {userName}: {ex.Message}");
            throw;
        }        
    }
}
