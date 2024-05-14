using Datahub.Application.Services;
using Datahub.Application.Services.UserManagement;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;

namespace Datahub.Infrastructure.Services;

public class DatahubTelemetryAuditingService : IDatahubAuditingService
{
    private readonly TelemetryClient telemetryClient;
    private readonly IUserInformationService userInformationService;

    public DatahubTelemetryAuditingService(IUserInformationService userInformationService, IOptions<TelemetryConfiguration> telemetryConfig)
    {
        telemetryClient = new TelemetryClient(telemetryConfig.Value);
        this.userInformationService = userInformationService;
    }

    /// <inheritdoc/>
    public async Task TrackDataEvent(string objectId, string table, AuditChangeType changeType, bool anonymous, params (string Key, string Value)[] details)
    {
        var properties = new Dictionary<string, string>
        {
            [nameof(objectId)] = objectId,
            [nameof(table)] = table,
            [nameof(changeType)] = changeType.ToString(),
        };
        await AppendIdentity(properties, anonymous);
        telemetryClient.TrackEvent("DataEvent", AppendDetails(properties, details));
        telemetryClient.Flush();
    }

    /// <inheritdoc/>
    public async Task TrackSecurityEvent(string scope, string table, AuditChangeType changeType, params (string Key, string Value)[] details)
    {
        var properties = new Dictionary<string, string>
        {
            [nameof(scope)] = scope,
            [nameof(table)] = table,
            [nameof(changeType)] = changeType.ToString()
        };
        await AppendIdentity(properties);
        telemetryClient.TrackEvent("SecurityEvent", AppendDetails(properties, details));
        telemetryClient.Flush();
    }

    /// <inheritdoc/>
    public async Task TrackAdminEvent(string scope, string table, AuditChangeType changeType, params (string Key, string Value)[] details)
    {
        var properties = new Dictionary<string, string>
        {
            [nameof(scope)] = scope,
            [nameof(table)] = table,
            [nameof(changeType)] = changeType.ToString()
        };
        await AppendIdentity(properties);
        telemetryClient.TrackEvent("AdminEvent", AppendDetails(properties, details));
        telemetryClient.Flush();
    }

    /// <inheritdoc/>
    public async Task TrackException(Exception exception, params (string Key, string Value)[] details)
    {
        var properties = new Dictionary<string, string>();
        await AppendIdentity(properties);
        telemetryClient.TrackException(exception, AppendDetails(properties, details));
        telemetryClient.Flush();
    }

    /// <inheritdoc/>
    public async Task TrackEvent(string name, params (string Key, string Value)[] details)
    {
        var properties = new Dictionary<string, string>();
        await AppendIdentity(properties);
        telemetryClient.TrackEvent(name, AppendDetails(properties, details));
        telemetryClient.Flush();
    }

    private async Task AppendIdentity(Dictionary<string, string> dictionary, bool anonymous = false)
    {
        var user = anonymous ? await userInformationService.GetAnonymousGraphUserAsync() : await userInformationService.GetCurrentGraphUserAsync();
        if (user != null)
        {
            dictionary["userId"] = user.Id;
            dictionary["userName"] = user.DisplayName;
            dictionary["userPrincipalName"] = user.UserPrincipalName;
        }
    }

    private static Dictionary<string, string> AppendDetails(Dictionary<string, string> dictionary, (string Key, string Value)[] properties)
    {
        foreach (var (key, value) in properties)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
            }
        }
        return dictionary;
    }
}