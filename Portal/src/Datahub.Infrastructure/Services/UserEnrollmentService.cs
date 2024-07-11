using System.Net.Mail;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Onboarding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services;

public partial class UserEnrollmentService : IUserEnrollmentService
{
    private readonly ILogger<UserEnrollmentService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly DatahubPortalConfiguration _datahubPortalConfiguration;
    private readonly IDbContextFactory<DatahubProjectDBContext> _contextFactory;

    public UserEnrollmentService(
        ILogger<UserEnrollmentService> logger, 
        IHttpClientFactory httpClientFactory,
        DatahubPortalConfiguration datahubPortalConfiguration,
        IDbContextFactory<DatahubProjectDBContext> contextFactory) 
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _datahubPortalConfiguration = datahubPortalConfiguration;
        _contextFactory = contextFactory;
    }

    public bool IsValidGcEmail(string? email)
    {
        if(email == null) return false;
        
        var url = _datahubPortalConfiguration.AllowedUserEmailDomains;

        foreach (var item in url)
        {
            if (email.ToLowerInvariant().EndsWith(item.ToLowerInvariant())) return IsValidEmail(email);
        }
        return false;
    }

    public bool IsValidEmail(string? email)
    {
        try
        {
            var address = new MailAddress(email).Address;
            return true;
        }
        catch (FormatException)
        {
        }
        return false;
    }

    public async Task<string> SendUserDatahubPortalInvite(string? registrationRequestEmail, string? inviterName)
    {
        var validGcEmail = IsValidGcEmail(registrationRequestEmail);
        if (validGcEmail == false || string.IsNullOrWhiteSpace(registrationRequestEmail))
        {
            throw new InvalidOperationException("Invalid email address. Must be a valid GC email address");
        }
        
        _logger.LogInformation("Sending invite to {Email}", registrationRequestEmail);

        var payload = new Dictionary<string, JsonNode>
        {
            ["email"] = registrationRequestEmail!,
            ["inviter"] = inviterName!
        };

        var jsonBody = new JsonObject(payload!);
        var url = _datahubPortalConfiguration.DatahubGraphInviteFunctionUrl;

        var numberOfRetries = 0;
        const int maxNumberOfRetries = 5;
        string id, resultString;
        
        var content = new StringContent(jsonBody.ToString(), Encoding.UTF8, "application/json");
        do
        {
            using var client = _httpClientFactory.CreateClient();
            var result = await client.PostAsync(url, content);

            resultString = await result.Content.ReadAsStringAsync();
        
            var resultJson = JsonNode.Parse(resultString);
            id = resultJson?["data"]?["id"]?.ToString() ?? string.Empty;
        
            // try to see if function wrapped it in a "Value" object
            if (string.IsNullOrWhiteSpace(id))
            {
                id = resultJson?["Value"]?["data"]?["id"]?.ToString() ?? string.Empty;
            }
        } while (string.IsNullOrWhiteSpace(id) && numberOfRetries++ < maxNumberOfRetries);

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidOperationException($"No ID available in response '{resultString}' from {url}");
        }
        
        _logger.LogInformation("Invite sent to {Email} and received id {Id}", registrationRequestEmail, id);
        return id;
    }

    public async Task SaveRegistrationDetails(string? registrationRequestEmail, string? comment)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var selfRegistrationDetails = new SelfRegistrationDetails
        {
            Email = registrationRequestEmail,
            Comment = comment,
            CreatedAt = DateTime.UtcNow
        };
        
        context.SelfRegistrationDetails.Add(selfRegistrationDetails);
        await context.SaveChangesAsync();
    }
}