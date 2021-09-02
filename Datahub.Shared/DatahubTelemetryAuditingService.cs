using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using NRCan.Datahub.Shared.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NRCan.Datahub.Shared
{
    public class DatahubTelemetryAuditingService : IDatahubAuditingService
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly IUserInformationService _userInformationService;

        public DatahubTelemetryAuditingService(IUserInformationService userInformationService)
        {
            _telemetryClient = new TelemetryClient(TelemetryConfiguration.CreateDefault());
            _userInformationService = userInformationService;
        }
        
        /// <inheritdoc>
        public async Task TrackDataEvent(string objectId, string table, AuditChangeType changeType, params (string key, string value)[] details)
        {
            var properties = new Dictionary<string, string>
            {
                [nameof(objectId)] = objectId,
                [nameof(table)] = table,
                [nameof(changeType)] = changeType.ToString(),
            };
            await AppendIdentity(properties);
            _telemetryClient.TrackEvent("DataEvent", AppendDetails(properties, details));
        }

        /// <inheritdoc>
        public async Task TrackSecurityEvent(string scope, string table, AuditChangeType changeType, params (string key, string value)[] details)
        {
            var properties = new Dictionary<string, string>
            {
                [nameof(scope)] = scope,
                [nameof(table)] = table,
                [nameof(changeType)] = changeType.ToString()
            };
            await AppendIdentity(properties);
            _telemetryClient.TrackEvent("SecurityEvent", AppendDetails(properties, details));
        }

        /// <inheritdoc>
        public async Task TrackAdminEvent(string scope, string table, AuditChangeType changeType, params (string key, string value)[] details)
        {
            var properties = new Dictionary<string, string>
            {
                [nameof(scope)] = scope,
                [nameof(table)] = table,
                [nameof(changeType)] = changeType.ToString()
            };
            await AppendIdentity(properties);
            _telemetryClient.TrackEvent("SecurityEvent", AppendDetails(properties, details));
        }

        private async Task AppendIdentity(Dictionary<string, string> dictionary)
        {
            var user = await _userInformationService.GetUserAsync();
            if (user != null)
            {
                dictionary["userId"] = user.Id;
                dictionary["userName"] = user.DisplayName;
                dictionary["userPrincipalName"] = user.UserPrincipalName;
            }
        }

        static Dictionary<string, string> AppendDetails(Dictionary<string, string> dictionary, (string key, string value)[] properties)
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
}
