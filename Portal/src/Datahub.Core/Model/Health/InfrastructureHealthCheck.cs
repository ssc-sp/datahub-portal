using System;

namespace Datahub.Core.Model.Health;

public class InfrastructureHealthCheck
{
    /// <summary>
    /// The database id of the health check
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The group of the health check
    ///
    /// Example: Datahub Core, Datahub Queues, Workspace Acronyms
    /// </summary>
    public string Group { get; set; }

    /// <summary>
    /// The name of the resource, a bit more specific than the group
    ///
    /// Example: Portal Database, Portal Web App, Portal Storage Account
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The url of the resource, if applicable, to open directly into Azure Portal
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// The resource type of what is being checked
    /// </summary>
    public InfrastructureHealthResourceType ResourceType { get; set; }

    /// <summary>
    /// The status of the health check
    /// </summary>
    public InfrastructureHealthStatus Status { get; set; }

    /// <summary>
    /// The time the health check was performed
    ///
    /// Note: This is in UTC and is automatically set by the database if not set
    /// </summary>
    public DateTime HealthCheckTimeUtc { get; set; }
}

/// <summary>
/// The different statuses of the health check
/// Feel free to add more if needed
/// </summary>
public enum InfrastructureHealthStatus
{
    Create,
    Expired,
    Healthy,
    Degraded,
    Unhealthy
}

/// <summary>
/// The different resource types that can be checked
/// Feel free to add more if needed
/// </summary>
public enum InfrastructureHealthResourceType
{
    AzureSqlDatabase,
    AzureStorageAccount,
    AzureKeyVault,
    AzureDatabricks,
    AzureStorageQueue,
    AzureWebApp,
    AzureFunction
}