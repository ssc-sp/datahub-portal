using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Datahub.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services;

public partial class UserEnrollmentService : IUserEnrollmentService
{
    // private readonly IDbContextFactory<DatahubProjectDBContext> _dbFactory;
    private readonly ILogger<UserEnrollmentService> _logger;
    private readonly IConfiguration? _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    [GeneratedRegex("^([\\w\\.\\-]+)@([\\w\\-]+)(\\.gc\\.ca)$")]
    private static partial Regex GC_CA_Regex();
    
    [GeneratedRegex("^([\\w\\.\\-]+)@(canada\\.ca)$")]
    private static partial Regex Canada_CA_Regex();
    
    public UserEnrollmentService(
        // IDbContextFactory<DatahubProjectDBContext> dbFactory,
        ILogger<UserEnrollmentService> logger,
        IConfiguration? configuration,
        IHttpClientFactory httpClientFactory
    ) 
    {
        // _dbFactory = dbFactory;
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    private bool IsValidGcEmail(string email)
    {
        var reOld = GC_CA_Regex();
        var reNew = Canada_CA_Regex();

        return reOld.IsMatch(email) || reNew.IsMatch(email);
    }

    public async Task<string> SendUserDatahubPortalInvite(string registrationRequestEmail)
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
        };

        var jsonBody = new JsonObject(payload!);
        string? url = _configuration?["datahub-create-graph-user-url"];

        var content = new StringContent(jsonBody.ToString(), Encoding.UTF8, "application/json");
        using var client = _httpClientFactory.CreateClient();
        var result = await client.PostAsync(url, content);

        var resultString = await result.Content.ReadAsStringAsync();
        
        var resultJson = JsonNode.Parse(resultString);
        var id = resultJson?["data"]?["id"]?.ToString() ?? string.Empty;
        
        _logger.LogInformation("Invite sent to {Email} and received id {Id}", registrationRequestEmail, id);
        return id;
    }
}