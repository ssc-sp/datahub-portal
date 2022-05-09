using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
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

    private const string PORTAL_URL = "https://datahub-test.scienceprogram.cloud";


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
            log.LogInformation("Creating graph service client");
            var graphClient = GetGraphServiceClientFromEnvVariables();

            log.LogInformation("Sending invitation to {UserEmail}", userEmail);
            var result = await SendInvitation(userEmail, graphClient);

            var groupId = Environment.GetEnvironmentVariable(SP_GROUP_ID);

            log.LogInformation("Adding invited user {UserID} to group {GroupID}", result.InvitedUser.Id, groupId);
            await AddToGroup(result.InvitedUser.Id, groupId, graphClient);

            log.LogInformation("Success inviting {UserEmail} ({UserID}) to group {GroupID}", userEmail,
                result.InvitedUser.Id, groupId);
            return new OkObjectResult($"Successfully invited {userEmail} and added to group {groupId}");
        }
        catch (Exception e)
        {
            log.LogError(e, "Error creating user");
            return new InternalServerErrorResult();
        }
    }

    private static async Task AddToGroup(string userId, string groupId, GraphServiceClient graphClient)
    {
        var userDirectoryObject = new DirectoryObject
        {
            Id = userId
        };

        await graphClient.Groups[groupId].Members.References
            .Request()
            .AddAsync(userDirectoryObject);
    }

    private static async Task<Invitation> SendInvitation(string userEmail, GraphServiceClient graphClient)
    {
        var invitation = new Invitation
        {
            InvitedUserEmailAddress = userEmail,
            InviteRedirectUrl = PORTAL_URL,
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