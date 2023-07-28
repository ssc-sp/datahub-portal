using System.Net;
using Microsoft.Graph;
using Azure.Identity;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

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
            _logger.LogInformation("C# HTTP trigger function processed a request");
            var graphClient = GetAuthenticatedGraphClient();
            var lockedGraphResult = await graphClient.Users
                .Request()
                .Filter("accountEnabled eq false")
                .Select("displayName,mail,id")
                .GetAsync();
            List<string> lockedUsers = new List<string>();
            foreach (var User in lockedGraphResult.ToArray())
            {
                lockedUsers.Add(User.Mail);
            }
            
            var allGraphResult = await graphClient.Users
                .Request()
                .Select("displayName,mail,id")
                .GetAsync();
            List<string> allUsers = new List<string>();
            foreach (var User in allGraphResult.ToArray())
            {
                allUsers.Add(User.Mail);
            }
            
            var dict = new Dictionary<string, List<string>>()
            {
                { "all", allUsers },
                { "locked", lockedUsers }
            };
            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(dict);
            // var graphResult = await graphClient.Groups["307ddf29-9d01-45dd-a87e-9d867c0a177d"].Members.Request()
            //     .GetAsync();
            // var response = request.CreateResponse(HttpStatusCode.OK);
            // await response.WriteAsJsonAsync(graphResult);
            return response;
        }
    }
}