using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;

namespace Datahub.Functions;

public static class RegisterNewUser
{
    [FunctionName("RegisterNewUser")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request");


        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        
        string emailAddress = data?.emailAddress;
        if(string.IsNullOrWhiteSpace(emailAddress)) 
            return new BadRequestObjectResult("Please pass an email address in the request body");

        var scopes = new[] {"https://graph.microsoft.com/.default"};
        
        var clientId = Environment.GetEnvironmentVariable("ClientId");        
        var clientSecret = Environment.GetEnvironmentVariable("ClientSecret");
        var tenantId = Environment.GetEnvironmentVariable("TenantId");
        var redirectUri = Environment.GetEnvironmentVariable("RedirectUri");
        

        var options = new TokenCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
        };

        var clientSecretCredential = new ClientSecretCredential(
            tenantId, clientId, clientSecret, options);

        var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

        var invitation = new Invitation
        {
            InvitedUserEmailAddress = emailAddress,
            InviteRedirectUrl = redirectUri,
            SendInvitationMessage = true
        };

        await graphClient.Invitations
            .Request()
            .AddAsync(invitation);

        return new OkObjectResult($"Successfully sent invitation to new user at {emailAddress}");
    }
}