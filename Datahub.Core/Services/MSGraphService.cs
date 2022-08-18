using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Datahub.Core.Data;
using System.Threading;
using System.Linq;

namespace Datahub.Core.Services
{
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
            await PrepareAuthenticatedClient();

            var options = new List<Option>();
            options.Add(new QueryOption("$filter", $"startswith(mail,'{filterText}')"));
            options.Add(new HeaderOption("ConsistencyLevel", "eventual"));
            options.Add(new QueryOption("$count", "true"));


            var usersPage = await _graphServiceClient.Users.Request(options).GetAsync(token);

            if (usersPage?.CurrentPage.Count > 0)
            {
                foreach (User user in usersPage)
                {
                    var newUser = GraphUser.Create(user);
                    users.Add(newUser.Id, newUser);
                }
            }

            // Fetch each page and add those results to the list
            while (usersPage.NextPageRequest != null)
            {
                usersPage = await usersPage.NextPageRequest.GetAsync();
                foreach (User user in usersPage)
                {
                    var newUser = GraphUser.Create(user);
                    users.Add(newUser.Id, newUser);
                }
            }

            return users;
        }

        private async Task PrepareAuthenticatedClient()
        {
            if (_graphServiceClient == null)
            {
                var clientSecret = await _keyVaultService.GetClientSecret();
                try
                {
                    IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                                    .Create(_configuration.GetSection("AzureAd").GetValue<string>("ClientId"))
                                    .WithTenantId(_configuration.GetSection("AzureAd").GetValue<string>("TenantId"))
                                    .WithClientSecret(clientSecret)
                                    .Build();
                    ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);

                    var httpClient = _httpClientFactory.CreateClient();
                    _graphServiceClient = new(httpClient);

                    _graphServiceClient.AuthenticationProvider = authProvider;

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
            await PrepareAuthenticatedClient();
            try
            {
                var user = await _graphServiceClient.Users.Request().Filter(filter).GetAsync(token);
                return user == null ? null : GraphUser.Create(user[0]);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching user: {filter}: {ex.Message}");
            }
            return new();
        }

		public async Task<GraphUser> GetUserFromSamAccountNameAsync(string userName, CancellationToken token)
		{
            await PrepareAuthenticatedClient();
            try
			{
                var queryOptions = new List<Microsoft.Graph.Option>()
                {
                    new Microsoft.Graph.QueryOption("$search", $"\"onPremisesSamAccountName:{userName}\""),
                    new Microsoft.Graph.QueryOption("$select", "mail,department"),
                    new Microsoft.Graph.HeaderOption("ConsistencyLevel", "eventual")
                };

                var users = await _graphServiceClient.Users.Request(queryOptions).GetAsync(token);
                var foundUser = users.FirstOrDefault();

                return foundUser is null ? null : GraphUser.Create(foundUser);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching user: {userName}: {ex.Message}");
            }
            return new();
        }
	}
}
