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

        //public GraphUser GetUser(string userId)
        //{
        //    if (!string.IsNullOrWhiteSpace(userId))
        //    {
        //        if (UsersDict != null && UsersDict.ContainsKey(userId))
        //        {
        //            var user = UsersDict[userId];
                    
        //            return UsersDict[userId];
        //        }
        //    }

        //    return null;
        //}

        public async Task<GraphUser> GetUserAsync(string userId)
        {
            PrepareAuthenticatedClient();
            var user = await graphServiceClient.Users.Request()
                .Filter($"id eq '{userId}'")
                .GetAsync();
            return user == null ? null : GraphUser.Create(user[0]);
        }

        public async Task<string> GetUserName(string userId)
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {             
                var user = await GetUserAsync(userId);
                return user?.DisplayName ?? "...";
            }

            return "...";
        }



        public async Task<string> GetUserEmail(string userId)
        {
            var user = await GetUserAsync(userId);
            return user?.Mail;
        }



        public async Task<string> GetUserIdFromEmailAsync(string email)
        {
            PrepareAuthenticatedClient();
            var user = await graphServiceClient.Users.Request()
                .Filter($"mail eq '{email}'")
                .GetAsync();
            return user[0].Id ?? string.Empty;
        }
        

        public async Task<Dictionary<string, GraphUser>> GetUsersListAsync(string filterText)
        {
            Dictionary<string, GraphUser> users = new();
            PrepareAuthenticatedClient();

            var options = new List<Option>();
            options.Add(new QueryOption("$filter",$"startswith(mail,'{filterText}')"));
            options.Add(new HeaderOption("ConsistencyLevel", "eventual"));
            options.Add(new QueryOption("$count", "true"));


            var usersPage = await graphServiceClient.Users.Request(options)                
                .GetAsync();

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
        //public async Task LoadUsersAsync()
        //{
        //    try
        //    {
        //        if (UsersDict == null)
        //        {
        //            UsersDict = new();
        //            //_logger.LogInformation("Entering Log Users");
        //            //UsersDict = new Dictionary<string, GraphUser>();
        //            //PrepareAuthenticatedClient();

        //            //IGraphServiceUsersCollectionPage usersPage = await graphServiceClient.Users.Request().GetAsync();

        //            //// Add the first page of results to the user list                    
        //            //if (usersPage?.CurrentPage.Count > 0)
        //            //{
        //            //    foreach (User user in usersPage)
        //            //    {
        //            //        var newUser = GraphUser.Create(user);
        //            //        UsersDict.Add(newUser.Id, newUser);
        //            //    }
        //            //}

        //            //// Fetch each page and add those results to the list
        //            //while (usersPage.NextPageRequest != null)
        //            //{
        //            //    usersPage = await usersPage.NextPageRequest.GetAsync();
        //            //    foreach (User user in usersPage)
        //            //    {
        //            //        var newUser = GraphUser.Create(user);
        //            //        UsersDict.Add(newUser.Id, newUser);
        //            //    }
        //            //}

        //            //// add anonymous user
        //            //var anonUser = UserInformationServiceConstants.GetAnonymousUser();
        //            //if (anonUser != null)
        //            //{
        //            //    var newUser = GraphUser.Create(anonUser);
        //            //    UsersDict.Add(newUser.Id, newUser);
        //            //}

        //            ////var user1 = UsersDict.Values.Where(u => u.Mail.ToLower() == "natasha.lestage@nrcan-rncan.gc.ca").FirstOrDefault().Id;
                    
                    
        //            //_logger.LogInformation("Exiting Log Users");
        //        }
        //    }
        //    catch (ServiceException e)
        //    {
        //        if (e.InnerException is MsalUiRequiredException || e.InnerException is MicrosoftIdentityWebChallengeUserException)
        //            throw;
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError(e, $"Error Loading Users Async");
        //    }

        //}
        
        //public async Task<Dictionary<string, GraphUser>> GetUsersAsync()
        //{
        //    try
        //    {
        //        if (UsersDict == null)
        //        {
        //            await LoadUsersAsync();
        //        }

        //        return UsersDict;
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError(e,"Could not get users");
        //    }

        //    return new Dictionary<string, GraphUser>();
        //}

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
                    Console.WriteLine($"Error preparing authentication client: {e.Message}");
                    throw;
                }
            }
        }
    }
}
