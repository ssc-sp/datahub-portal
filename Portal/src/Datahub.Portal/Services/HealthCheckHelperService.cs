using Datahub.Core.Model.Health;

namespace Datahub.Portal.Services
{
    public class HealthCheckHelperService
    {
        public static InfrastructureHealthStatus GetRealStatus(InfrastructureHealthCheck health)
        {
            var realStatus = health.Status;
            var timestamp = health.HealthCheckTimeUtc;
            var now = DateTime.UtcNow;
            if (health.Status == InfrastructureHealthStatus.Healthy)
            {
                if (timestamp >= now.AddHours(-72) && timestamp < now.AddHours(-24))
                {
                    realStatus = InfrastructureHealthStatus.Degraded;
                }
                if (timestamp < now.AddHours(-72))
                {
                    realStatus = InfrastructureHealthStatus.Unhealthy;
                }
            }
            if (health.Status == InfrastructureHealthStatus.Create)
            {
                realStatus = InfrastructureHealthStatus.NeedHealthCheckRun;
            }
            return realStatus;
        }

        public static string GetStatusDetails(InfrastructureHealthCheck health)
        {
            var timeStamp = health.HealthCheckTimeUtc.ToLocalTime().ToString();
            if (health.Status == InfrastructureHealthStatus.Healthy) return timeStamp;
            return $"{health.Details} (as of {timeStamp})";
        }

        public static string GetStatusText(InfrastructureHealthCheck health)
        {
            var realStatus = GetRealStatus(health);
            return realStatus.ToString();
        }

        public static MudBlazor.Color GetColor(InfrastructureHealthCheck health)
        {
            var realStatus = GetRealStatus(health);
            return GetColor(realStatus);
        }

        public static MudBlazor.Color GetColor(InfrastructureHealthStatus status)
        {
            return status switch
            {
                InfrastructureHealthStatus.Healthy => MudBlazor.Color.Success,
                InfrastructureHealthStatus.Degraded => MudBlazor.Color.Warning,
                InfrastructureHealthStatus.Unhealthy => MudBlazor.Color.Error,
                _ => MudBlazor.Color.Default
            };
        }
    }
}
