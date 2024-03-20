using Datahub.Core.Model.Projects;
using Microsoft.Graph.Models;

namespace Datahub.ProjectTools.Catalog;

public abstract class ActiveGitModuleResource : IProjectResource
{
    public string? GetCostEstimatorLink()
    {
        return null;
    }

    protected Dictionary<string, object> Parameters =  new();

    public (Type, IDictionary<string, object>)[] GetActiveResources()
    {
        if (IsServiceAvailable && IsServiceConfigured)
            return new (Type, IDictionary<string, object>)[] { GetComponent() };
        else
            return Array.Empty<(Type type, IDictionary<string, object> parameters)>();
    }

    protected abstract bool IsServiceConfigured { get; }
    protected abstract bool IsServiceAvailable { get; }

    protected abstract Type ComponentType { get; }

    protected abstract bool IsServiceRequested { get; }

    protected abstract string Title { get; }
    protected abstract string Description { get; }
    protected abstract string Icon { get; }
    protected abstract bool IsIconSVG { get; }

    protected virtual bool IsRequestAvailable { get; } = true;

    public Datahub_Project Project { get; private set; }

    public (Type type, IDictionary<string, object> parameters)? GetInactiveResource()
    {
        if (!IsServiceConfigured || IsServiceAvailable || !IsRequestAvailable)
            return null;
        return (typeof(InactiveResource), GetInactiveParameters());
    }

    protected (Type ComponentType, Dictionary<string, object> Parameters) GetComponent()
        => (ComponentType, Parameters);

    public abstract string[] GetTags();

    public async Task<bool> InitializeAsync(Datahub_Project project, string? userId, User graphUser, bool isProjectAdmin)
    {
        if (userId is null)
            return false;
        Project = project;
        Parameters = new Dictionary<string, object>
        {
            { nameof(InactiveResource.Title), Title },
            { nameof(InactiveResource.Description), Description },
            { nameof(InactiveResource.Icon), Icon },
            { nameof(InactiveResource.IsIconSVG), IsIconSVG }
        };
        await InitializeAsync(userId, graphUser, isProjectAdmin);
        return true;
    }

    protected abstract Task InitializeAsync(string? userId, User graphUser, bool isProjectAdmin);

    protected Dictionary<string, object> GetInactiveParameters()
        => new Dictionary<string, object>
        {
            { nameof(InactiveResource.Title), Title },
            { nameof(InactiveResource.Description), Description },
            { nameof(InactiveResource.Icon), Icon },
            { nameof(InactiveResource.IsIconSVG), IsIconSVG },
            { nameof(InactiveResource.ResourceRequested), IsServiceRequested },
            { nameof(InactiveResource.Project), Project },
            { nameof(InactiveResource.ResourceType), ComponentType.Name.ToLowerInvariant() },
        };
}