using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services;

public partial class UserEnrollmentService : IUserEnrollmentService
{
    private readonly ILogger<UserEnrollmentService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly DatahubPortalConfiguration _datahubPortalConfiguration;

    [GeneratedRegex("^([\\w\\.\\-]+)@([\\w\\-]+)(\\.gc\\.ca)$")]
    private static partial Regex GC_CA_Regex();
    
    [GeneratedRegex("^([\\w\\.\\-]+)@(canada\\.ca)$")]
    private static partial Regex Canada_CA_Regex();
    
    public UserEnrollmentService(ILogger<UserEnrollmentService> logger, IHttpClientFactory httpClientFactory,
        DatahubPortalConfiguration datahubPortalConfiguration) 
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _datahubPortalConfiguration = datahubPortalConfiguration;
    }

    public bool IsValidGcEmail(string? email)
    {
        if(email == null) return false;
        
        var reOld = GC_CA_Regex();
        var reNew = Canada_CA_Regex();

        return reOld.IsMatch(email) || reNew.IsMatch(email);
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
        } while (string.IsNullOrWhiteSpace(id) || numberOfRetries++ < maxNumberOfRetries);

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidOperationException($"No ID available in response '{resultString}' from {url}");
        }
        
        _logger.LogInformation("Invite sent to {Email} and received id {Id}", registrationRequestEmail, id);
        return id;
    }
}