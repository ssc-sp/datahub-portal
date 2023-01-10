using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.Security;
using Datahub.Portal.Data.Forms;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Portal.Services;

public partial class RegistrationService
{
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbFactory;
    private readonly ILogger<RegistrationService> _logger;
    private static IConfiguration _configuration;

    [GeneratedRegex("^([\\w\\.\\-]+)@([\\w\\-]+)(\\.gc\\.ca)$")]
    private static partial Regex GC_CA_Regex();
    
    [GeneratedRegex("^([\\w\\.\\-]+)@(canada\\.ca)$")]
    private static partial Regex Canada_CA_Regex();
    
    public RegistrationService(
        IDbContextFactory<DatahubProjectDBContext> dbFactory,
        ILogger<RegistrationService> logger,
        IConfiguration configuration
    )
    {
        _dbFactory = dbFactory;
        _logger = logger;
        _configuration = configuration;
    }

    private bool IsValidGcEmail(string email)
    {
        const string oldRegex = @"^([\w\.\-]+)@([\w\-]+)(\.gc\.ca)$";
        const string newRegex = @"^([\w\.\-]+)@(canada\.ca)$";

        var reOld = GC_CA_Regex();
        var reNew = Canada_CA_Regex();

        return reOld.IsMatch(email) || reNew.IsMatch(email);
    }

    public async Task<string> SendUserInvite(string registrationRequestEmail, bool mockInvite = false)
    {
        var validGcEmail = IsValidGcEmail(registrationRequestEmail);
        if (validGcEmail == false)
        {
            throw new InvalidOperationException("Invalid email address. Must be a valid GC email address");
        }

        using var client = new HttpClient();
        var payload = new Dictionary<string, JsonNode>
        {
            ["email"] = registrationRequestEmail,
            ["mockInvite"] = mockInvite,
        };

        var jsonBody = new JsonObject(payload);
        var url = _configuration["datahub-create-graph-user-url"];

        var content = new StringContent(jsonBody.ToString(), Encoding.UTF8, "application/json");
        var result = await client.PostAsync(url, content);

        var resultString = await result.Content.ReadAsStringAsync();
        if (mockInvite)
        {
            return resultString;
        }

        var resultJson = JsonNode.Parse(resultString);
        return resultJson?["data"]?["id"]?.ToString();
    }


}