using Azure.Identity;
using Datahub.Infrastructure.Queues.Messages;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Datahub.Functions;

public class CreateGraphUser
{
    private readonly ILogger _logger;
    private readonly AzureConfig _configuration;
    private readonly IMediator _mediator;

    public CreateGraphUser(ILoggerFactory loggerFactory, AzureConfig configuration, IMediator mediator)
    {
        _logger = loggerFactory.CreateLogger<CreateGraphUser>();
        _configuration = configuration;
        _mediator = mediator;
    }
    
    [Function("CreateGraphUser")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestData req)
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
            _logger.LogError(e, "Error creating user");
            return new BadRequestResult();
        }
    }

    private IActionResult MockInviteUser(string userEmail, ILogger log)
    {
        log.LogInformation("*** Mocking the AD Graph invitation ***");
        
        log.LogInformation("Creating graph service client");
        
        // sanity check the service principal credentials
        var graphClient = GetGraphServiceClientFromEnvVariables();
        
        var groupId = _configuration.ServicePrincipalGroupID;
        
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
        var groupId = _configuration.ServicePrincipalGroupID;
            
        log.LogInformation("Adding invited user {UserID} to group {GroupID}", result.InvitedUser.Id, groupId);
        await AddToGroup(result.InvitedUser.Id, groupId!, graphClient, log);
            
        log.LogInformation("Success, {UserEmail} ({UserID}) is in group {GroupID}", userEmail,
            result.InvitedUser.Id, groupId);

        // send invite email
        await SenInvitationEmail(userEmail, inviter);

        var response = new JsonObject
        {
            ["message"] = $"Successfully invited {userEmail} and added to group {groupId}",
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

    private async Task<Invitation> SendInvitation(string userEmail, GraphServiceClient graphClient)
    {
        var invitation = new Invitation
        {
            InvitedUserEmailAddress = userEmail,
            InviteRedirectUrl = _configuration.PortalUrl,
            SendInvitationMessage = false
        };

        var result = await graphClient.Invitations
            .Request()
            .AddAsync(invitation);
        return result;
    }

    private GraphServiceClient GetGraphServiceClientFromEnvVariables()
    {
        var scopes = new[] {"https://graph.microsoft.com/.default"};

        var options = new TokenCredentialOptions 
        { 
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud 
        };

        var clientSecretCredential = new ClientSecretCredential(_configuration.TenantId, 
            _configuration.ClientId, _configuration.ClientSecret, options);

        return new GraphServiceClient(clientSecretCredential, scopes); 
    }

    private async Task SenInvitationEmail(string userEmail, string inviter)
    {
        var (subjectTemplate, bodyTemplate) = TemplateUtils.GetEmailTemplate("user_invitation.html");
        if (subjectTemplate is null || bodyTemplate is null)
            _logger.LogWarning("user_invitation.html is missing");

        var portalLink = _configuration.PortalUrl ?? "";
        var subject = subjectTemplate ?? "Datahub invitation";
        var body = (bodyTemplate ?? "").Replace("{{inviter}}", inviter).Replace("{{datahub_link}}", portalLink);
        var contacts = new List<string>() { userEmail };

        EmailRequestMessage notificationEmail = new()
        {
            To = contacts,
            Subject = subject,
            Body = body
        };

        await _mediator.Send(notificationEmail);
    }

    record CreateUserRequest(string email, string mockInvite, string inviter);
}