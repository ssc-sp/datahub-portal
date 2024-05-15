using Azure.Identity;
using Datahub.Infrastructure.Queues.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Functions.Services;
using Datahub.Infrastructure.Extensions;
using Datahub.Shared.Configuration;
using MassTransit;

namespace Datahub.Functions;

public class CreateGraphUser(
    ILoggerFactory loggerFactory,
    AzureConfig configuration,
    ISendEndpointProvider sendEndpointProvider,
    IEmailService emailService)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<CreateGraphUser>();

    [Function("CreateGraphUser")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
        HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonSerializer.Deserialize<CreateUserRequest>(requestBody);

        var userEmail = data?.email;
        if (string.IsNullOrEmpty(userEmail) || !userEmail!.Contains('@'))
        {
            return new BadRequestObjectResult("Please pass a valid email address in the request body");
        }

        var inviter = data?.inviter ?? "datahub";

        try
        {
            bool isMockInvite = data?.mockInvite == "true";
            if (isMockInvite)
            {
                return MockInviteUser(userEmail, _logger);
            }

            return await InviteUser(_logger, userEmail, inviter);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error creating user: {e.Message},\n Trace: {e.StackTrace}");
            if (e.Message.Contains("blocked from signing in"))
            {
                await SendFailureEmail(e.Message);
                throw new Exception(e.Message);
            }

            return new BadRequestResult();
        }
    }

    private IActionResult MockInviteUser(string userEmail, ILogger log)
    {
        log.LogInformation("*** Mocking the AD Graph invitation ***");

        log.LogInformation("Creating graph service client");

        // sanity check the service principal credentials
        var graphClient = GetGraphServiceClientFromEnvVariables();

        var groupId = configuration.ServicePrincipalGroupID;

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

    private async Task<IActionResult> InviteUser(ILogger log, string userEmail, string inviter)
    {
        log.LogInformation("Creating graph service client");
        var graphClient = GetGraphServiceClientFromEnvVariables();

        log.LogInformation("Sending invitation to {UserEmail}", userEmail);

        var result = await SendInvitation(userEmail, graphClient);
        var groupId = configuration.ServicePrincipalGroupID;
        var message = $"Successfully invited {userEmail} and added to group {groupId}";

        if (groupId != null)
        {
            log.LogInformation("Adding invited user {UserID} to group {GroupID}", result.InvitedUser.Id, groupId);
            await AddToGroup(result.InvitedUser.Id, groupId!, graphClient, log);

            log.LogInformation("Success, {UserEmail} ({UserID}) is in group {GroupID}", userEmail,
                result.InvitedUser.Id, groupId);
        }
        else
        {
            log.LogInformation("No group found for invited user {UserID}", result.InvitedUser.Id);
            message = $"Successfully invited {userEmail}";
        }

        // send invite email
        await SenInvitationEmail(userEmail, inviter);

        var response = new JsonObject
        {
            ["message"] = $"{message}",
            ["data"] = new JsonObject
            {
                ["email"] = userEmail,
                ["id"] = result.InvitedUser.Id
            }
        };

        return new OkObjectResult(response);
    }

    private async Task AddToGroup(string userId, string groupId, GraphServiceClient graphClient, ILogger log)
    {
        var group = await graphClient.Groups[groupId].Members.GetAsync();
        var exists = group?.Value?.Any(m => m.Id == userId) ?? false;
        if (!exists)
        {
            var requestBody = new Microsoft.Graph.Models.ReferenceCreate
            {
                OdataId = $"https://graph.microsoft.com/v1.0/directoryObjects/{userId}",
            };
            await graphClient.Groups[$"{groupId}"].Members.Ref.PostAsync(requestBody);
            log.LogInformation("Added user {UserID} to group {GroupID}", userId, groupId);
        }
        else
        {
            log.LogInformation("User {UserID} already exists in group {GroupID}", userId, groupId);
        }


        var userDirectoryObject = new DirectoryObject
        {
            Id = userId
        };
    }

    private async Task<Invitation?> SendInvitation(string userEmail, GraphServiceClient graphClient)
    {
        var invitation = new Invitation
        {
            InvitedUserEmailAddress = userEmail,
            InviteRedirectUrl = configuration.PortalUrl,
            SendInvitationMessage = false
        };

        var result = await graphClient.Invitations
            .PostAsync(invitation);
        return result;
    }

    private GraphServiceClient GetGraphServiceClientFromEnvVariables()
    {
        var scopes = new[] { "https://graph.microsoft.com/.default" };

        var options = new TokenCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
        };

        var clientSecretCredential = new ClientSecretCredential(configuration.TenantId,
            configuration.ClientId, configuration.ClientSecret, options);

        return new GraphServiceClient(clientSecretCredential, scopes);
    }

    private async Task SenInvitationEmail(string userEmail, string inviter)
    {
        var portalLink = configuration.PortalUrl ?? "";
        var contacts = new List<string>() { userEmail };

        Dictionary<string, string> bodyArgs = new()
        {
            { "{{inviter}}", inviter },
            { "{{datahub_link}}", portalLink }
        };

        var email = emailService.BuildEmail("user_invitation.html", contacts, new List<string>(), bodyArgs,
            new Dictionary<string, string>());
        if (email is not null)
        {
            await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.EmailNotificationQueueName, email);
        }
    }

    private async Task SendFailureEmail(string message)
    {
        var subject = "Datahub invitation failed";
        var body = "An error occurred while inviting a user to Datahub.\n " +
                   "Please check the logs for more details.\n " +
                   $"Error message: {message}";
        var contacts = new List<string>() { configuration.Email.AdminEmail };

        EmailRequestMessage notificationEmail = new()
        {
            To = contacts,
            Subject = subject,
            Body = body
        };

        await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.EmailNotificationQueueName, notificationEmail);
    }

    record CreateUserRequest(string email, string mockInvite, string inviter);
}