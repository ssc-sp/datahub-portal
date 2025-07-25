using Datahub.Shared.Entities;

namespace Datahub.Portal.Services
{
    public static class HealthCheckPageUtil
    {
        private const int STALE_AGE_HOURS = 24;
        private const int EXPIRED_AGE_HOURS = 72;

        private static double GetAgeInHours(InfrastructureHealthCheck infrastructureHealthCheck) => (DateTime.UtcNow - infrastructureHealthCheck.HealthCheckTimeUtc).TotalHours;

        public static InfrastructureHealthStatus GetDisplayStatus(InfrastructureHealthCheck infrastructureHealthCheck) => infrastructureHealthCheck.Status switch
        {
            InfrastructureHealthStatus.Healthy => GetAgeInHours(infrastructureHealthCheck) < STALE_AGE_HOURS ? InfrastructureHealthStatus.Healthy : InfrastructureHealthStatus.NeedHealthCheckRun,
            _ => infrastructureHealthCheck.Status
        };

        public static string GetDisplayStatusText(InfrastructureHealthCheck infrastructureHealthCheck) => GetDisplayStatus(infrastructureHealthCheck).ToString();

        public static MudBlazor.Color GetDisplayColor(InfrastructureHealthCheck infrastructureHealthCheck) => infrastructureHealthCheck.Status switch
        {
            InfrastructureHealthStatus.Healthy => GetAgeBasedColor(GetAgeInHours(infrastructureHealthCheck)),
            InfrastructureHealthStatus.Degraded => MudBlazor.Color.Warning,
            InfrastructureHealthStatus.Unhealthy => MudBlazor.Color.Error,
            _ => MudBlazor.Color.Default
        };

        private static MudBlazor.Color GetAgeBasedColor(double age) => age < STALE_AGE_HOURS ? MudBlazor.Color.Success :
            age < EXPIRED_AGE_HOURS ? MudBlazor.Color.Warning :
            MudBlazor.Color.Error;

        public static string GetStatusDetails(InfrastructureHealthCheck health)
        {
            var timeStamp = health.HealthCheckTimeUtc.ToLocalTime().ToString();
            if (health.Status == InfrastructureHealthStatus.Healthy) return timeStamp;
            return $"{health.Details} (as of {timeStamp})";
        }

    }
}
