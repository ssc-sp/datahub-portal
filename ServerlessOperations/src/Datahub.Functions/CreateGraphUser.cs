using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Identity;
using Datahub.Application.Services.Notification;
using Datahub.Application.Services.UserManagement;
using Datahub.Infrastructure.Services.Azure;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using static System.Guid;

namespace Datahub.Functions;

public class CreateGraphUser(
    ILoggerFactory loggerFactory,
    AzureConfig configuration,
    IMSGraphService graphService,
    ISendEndpointProvider sendEndpointProvider,
    IGCNotifyService notifyService)
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

    [Function("AddUserToGroup")]
    public async Task<IActionResult> AddUserToGroup(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
        HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonSerializer.Deserialize<AddUserToGroupRequest>(requestBody);
        try
        {
            ValidateAddUserRequest(data);
            var graphClient = graphService.GetAuthenticatedClient();
            var groupId = configuration.ServicePrincipalGroupID;

            await AddToGroup(data.userId, groupId!, graphClient, _logger);
            return new OkResult();
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, $"Error validating adding request: {e.Message},\n Trace: {e.StackTrace}");
            return new BadRequestObjectResult("Please pass a valid user ID in the request body");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error adding user to group: {e.Message},\n Trace: {e.StackTrace}");
            return new BadRequestResult();
        }
    }

    private IActionResult MockInviteUser(string userEmail, ILogger log)
    {
        log.LogInformation("*** Mocking the AD Graph invitation ***");

        log.LogInformation("Creating graph service client");

        // sanity check the service principal credentials
        var graphClient = graphService.GetAuthenticatedClient();

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
        var graphClient = graphService.GetAuthenticatedClient();

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
        await notifyService.SendAccountCreatedNotification(userEmail);

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
        if (userId == "mockUser") return;
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

    private async Task SendFailureEmail(string message)
    {
        notifyService.SendDataHubErrorNotification(message, configuration.Email.AdminEmail);
    }

    record CreateUserRequest(string email, string mockInvite, string inviter);

    record AddUserToGroupRequest(string userId);

    private void ValidateAddUserRequest(AddUserToGroupRequest data)
    {
        if (!TryParse(data.userId, out var userId) || userId == Empty) throw new ArgumentException("Invalid user ID");
    }
}
