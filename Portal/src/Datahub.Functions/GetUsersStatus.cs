using System.Net;
using Microsoft.Graph;
using Azure.Identity;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Contrib.WaitAndRetry;

namespace Datahub.Functions
{
    public class GetUsersStatus
    {
        private readonly ILogger _logger;
        private readonly AzureConfig _configuration;
        private readonly IMediator _mediator;
        
        
        public GetUsersStatus(ILoggerFactory loggerFactory, AzureConfig configuration, IMediator mediator)
        {
            _logger = loggerFactory.CreateLogger<CreateGraphUser>();
            _configuration = configuration;
            _mediator = mediator;
        }
        
        private GraphServiceClient GetAuthenticatedGraphClient()
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            var clientSecretCredential = new ClientSecretCredential(
                _configuration.TenantId, _configuration.ClientId, _configuration.ClientSecret, options);

            return new(clientSecretCredential, scopes);
        }
        
        [Function("GetUsersStatus")]
        public async Task<HttpResponseData> GetUsersDetails(
            [HttpTrigger(AuthorizationLevel.Function, "get")]
            HttpRequestData request)
        {
            _logger.LogInformation("C# GetUsersStatus HTTP trigger function processed a request");
            var graphClient = GetAuthenticatedGraphClient();
            
            //////////////// Getting locked users
            _logger.LogInformation("Fetching locked users...");

            var lockedGraphResult = await graphClient.Users.GetAsync(
                requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Filter = "accountEnabled eq false";
                    requestConfiguration.QueryParameters.Select = new[] { "displayName","mail","id" };
                });


            var lockedUsers = lockedGraphResult?.Value?.Select(x => x.Mail).ToList();
            _logger.LogInformation("Processed locked users");

            //////////////// Getting Service Principal group members
            _logger.LogInformation("Fetching all SP group members...");
            var groupGraphResult = await graphClient.Groups[$"{_configuration.ServicePrincipalGroupID}"]
                .Members
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = new[] { "mail" };
                });
            var groupArray = JArray.Parse(JsonConvert.SerializeObject(groupGraphResult));
            var groupUsers = groupArray.Select(x => x["Mail"].ToString()).ToList();
            _logger.LogInformation("Processed SP group members");
            
            //////////////// Building response, intersect with group users because we only care about members of that group
            var dict = new Dictionary<string, List<string>>()
            {
                { "all", groupUsers },
                { "locked", lockedUsers.Intersect(groupUsers).ToList() }
            };
            
            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(dict);
            return response;
        }
    }
}