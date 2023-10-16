using System;

namespace Datahub.Core.Model.Health;

public class InfrastructureHealthCheck
{
    public int Id { get; set; }
    public string Group { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }

    public InfrastructureHealthResourceType ResourceType { get; set; }
    public InfrastructureHealthStatus Status { get; set; }

    public DateTime HealthCheckTimeUtc { get; set; }
}

public enum InfrastructureHealthStatus
{
    Create,
    Expired,
    Healthy,
    Degraded,
    Unhealthy
}

public enum InfrastructureHealthResourceType
{
    AzureSqlDatabase,
    AzureStorageAccount,
    AzureKeyVault,
    AzureDatabricks,
    AzureStorageQueue,
    AzureWebApp
}