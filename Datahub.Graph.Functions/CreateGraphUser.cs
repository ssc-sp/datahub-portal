using System;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Web.Http;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Newtonsoft.Json;

namespace Datahub.Graph.Functions;

public static class CreateGraphUser
{
    private const string TENANT_ID = "TENANT_ID";
    private const string CLIENT_ID = "FUNC_SP_CLIENT_ID";
    private const string CLIENT_SECRET = "FUNC_SP_CLIENT_SECRET";
    private const string SP_GROUP_ID = "SP_GROUP_ID";

    private const string PORTAL_URL = "PORTAL_URL";

    [FunctionName("CreateGraphUser")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
        HttpRequest req, ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        string userEmail = data?.email;

        if (string.IsNullOrEmpty(userEmail) || !userEmail!.Contains('@'))
        {
            return new BadRequestObjectResult("Please pass a valid email address in the request body");
        }

        try
        {
            bool isMockInvite = data.mockInvite == "true";
            if (isMockInvite)
            {
                return MockInviteUser(userEmail, log);
            }
            
            return await InviteUser(userEmail, log);
        }
        catch (Exception e)
        {
            log.LogError(e, "Error creating user");
            return new InternalServerErrorResult();
        }
    }

    private static IActionResult MockInviteUser(string userEmail, ILogger log)
    {
        log.LogInformation("*** Mocking the AD Graph invitation ***");
        
        log.LogInformation("Creating graph service client");
        
        // sanity check the service principal credentials
        var graphClient = GetGraphServiceClientFromEnvVariables();
        
        var groupId = Environment.GetEnvironmentVariable(SP_GROUP_ID);
        
        var response = new JsonObject
        {
            ["message"] = $"Successfully FAKE invited {userEmail} and added to group {groupId}",
            ["data"] = new JsonObject
            {
                ["email"] = userEmail,
                ["id"] = "00000000-0000-0000-0000-000000000000"
            } 
        };
            
        return new OkObjectResult(response.ToString());
    }

    private static async Task<IActionResult> InviteUser(string userEmail, ILogger log)
    {
        log.LogInformation("Creating graph service client");
        var graphClient = GetGraphServiceClientFromEnvVariables();

        log.LogInformation("Sending invitation to {UserEmail}", userEmail);
        
        var result = await SendInvitation(userEmail, graphClient);
        var groupId = Environment.GetEnvironmentVariable(SP_GROUP_ID);
            
        log.LogInformation("Adding invited user {UserID} to group {GroupID}", result.InvitedUser.Id, groupId);
        await AddToGroup(result.InvitedUser.Id, groupId, graphClient, log);
            
        log.LogInformation("Success, {UserEmail} ({UserID}) is in group {GroupID}", userEmail,
            result.InvitedUser.Id, groupId);
        
        var response = new JsonObject
        {
            ["message"] = $"Successfully invited {userEmail} and added to group {groupId}",
            ["data"] = new JsonObject
            {
                ["email"] = userEmail,
                ["id"] = result.InvitedUser.Id
            } 
        };
            
        return new OkObjectResult(response.ToString());
    }

    private static async Task AddToGroup(string userId, string groupId, GraphServiceClient graphClient, ILogger log)
    {
        var userDirectoryObject = new DirectoryObject
        {
            Id = userId
        };

        var exists = graphClient.Groups[groupId].Members.Request().GetAsync().Result.Any(m => m.Id == userId);
        if (!exists)
        {
            await graphClient.Groups[groupId].Members.References
                .Request()
                .AddAsync(userDirectoryObject);
            log.LogInformation("Added user {UserID} to group {GroupID}", userId, groupId);
        }
        else
        {
            log.LogInformation("User {UserID} already exists in group {GroupID}", userId, groupId);
        }
    }

    private static async Task<Invitation> SendInvitation(string userEmail, GraphServiceClient graphClient)
    {
        var invitation = new Invitation
        {
            InvitedUserEmailAddress = userEmail,
            InviteRedirectUrl = Environment.GetEnvironmentVariable(PORTAL_URL),
            SendInvitationMessage = true
        };

        var result = await graphClient.Invitations
            .Request()
            .AddAsync(invitation);
        return result;
    }

    private static GraphServiceClient GetGraphServiceClientFromEnvVariables()
    {
        var scopes = new[] {"https://graph.microsoft.com/.default"};

        var tenantId = Environment.GetEnvironmentVariable(TENANT_ID);
        var clientId = Environment.GetEnvironmentVariable(CLIENT_ID);
        var clientSecret = Environment.GetEnvironmentVariable(CLIENT_SECRET);

        var options = new TokenCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
        };

        var clientSecretCredential = new ClientSecretCredential(
            tenantId, clientId, clientSecret, options);

        var graphClient = new GraphServiceClient(clientSecretCredential, scopes);
        return graphClient;
    }

    private static async Task<GraphServiceClient> GetGraphServiceClientFromAkv()
    {
        var scopes = new[] {"https://graph.microsoft.com/.default"};

        var keyVaultName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
        var kvUri = "https://" + keyVaultName + ".vault.azure.net";
        var secretClient = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

        var tenantId = (await secretClient.GetSecretAsync(TENANT_ID)).Value.Value;
        var clientId = (await secretClient.GetSecretAsync(CLIENT_ID)).Value.Value;
        var clientSecret = (await secretClient.GetSecretAsync(CLIENT_SECRET)).Value.Value;


        var options = new TokenCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
        };

        var clientSecretCredential = new ClientSecretCredential(
            tenantId, clientId, clientSecret, options);

        var graphClient = new GraphServiceClient(clientSecretCredential, scopes);
        return graphClient;
    }
}