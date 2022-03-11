using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Datahub.Core.Data;
using System.Threading;

namespace Datahub.Core.Services
{
    public class MSGraphService : IMSGraphService
    {
        private GraphServiceClient graphServiceClient;
        private IConfiguration _configuration;
        private ILogger<MSGraphService> _logger;
        private IHttpClientFactory _httpClientFactory;

        public Dictionary<string, GraphUser> UsersDict { get; set; }

        public MSGraphService(IConfiguration configureOptions, 
            ILogger<MSGraphService> logger,
            IHttpClientFactory clientFactory)
        {
            //clientSecret = configuration["ClientAppSecret"];            
            _configuration = configureOptions;
            _logger = logger;
            _httpClientFactory = clientFactory;
        }
    
        public async Task<GraphUser> GetUserAsync(string userId, CancellationToken tkn = default)
        {
            PrepareAuthenticatedClient();
            try
            {
                var user = await graphServiceClient.Users.Request()
                    .Filter($"(id eq '{userId}') Or (mail eq '{userId}')")
                    .GetAsync(tkn);
                return user == null ? null : GraphUser.Create(user[0]);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching user: {userId}: {ex.Message}");
            }
            return new GraphUser();
        }

        public async Task<string> GetUserName(string userId, CancellationToken tkn)
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {             
                var user = await GetUserAsync(userId, tkn);
                return user?.DisplayName ?? "...";
            }

            return "...";
        }



        public async Task<string> GetUserEmail(string userId, CancellationToken tkn)
        {
            var user = await GetUserAsync(userId, tkn);
            return user?.Mail;
        }



        public async Task<string> GetUserIdFromEmailAsync(string email, CancellationToken tkn)
        {
            PrepareAuthenticatedClient();
            try
            {
                var user = await graphServiceClient.Users.Request()
                    .Filter($"mail eq '{email}'")
                    .GetAsync(tkn);
                return user[0].Id ?? string.Empty;
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error fetching user id for email: {email}: {ex.Message}");
            }

            return string.Empty;
        }
        

        public async Task<Dictionary<string, GraphUser>> GetUsersListAsync(string filterText, CancellationToken tkn)
        {
            Dictionary<string, GraphUser> users = new();
            PrepareAuthenticatedClient();

            var options = new List<Option>();
            options.Add(new QueryOption("$filter",$"startswith(mail,'{filterText}')"));
            options.Add(new HeaderOption("ConsistencyLevel", "eventual"));
            options.Add(new QueryOption("$count", "true"));


            var usersPage = await graphServiceClient.Users.Request(options)                
                .GetAsync(tkn);

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
 
        private void PrepareAuthenticatedClient()
        {
            if (graphServiceClient == null)
            {
                try
                {
                    //var graphService = _configuration.GetValue<MicrosoftIdentityOptions>("AzureAd");
                    IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                                    .Create(_configuration.GetSection("AzureAd").GetValue<string>("ClientId"))
                                    .WithTenantId(_configuration.GetSection("AzureAd").GetValue<string>("TenantId"))
                                    .WithClientSecret(_configuration.GetSection("AzureAd").GetValue<string>("ClientSecret"))
                                    .Build();
                    ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);

                    var httpClient = _httpClientFactory.CreateClient();
                    graphServiceClient = new GraphServiceClient(httpClient);

                    graphServiceClient.AuthenticationProvider = authProvider;

                }
                catch (Exception e)
                {
                    _logger.LogError($"Error preparing authentication client: {e.Message}");
                    throw;
                }
            }
        }
    }
}
