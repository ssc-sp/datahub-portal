using Datahub.Core.EFCore;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.ProjectTools.Catalog
{
    public abstract class DHURLResource : IProjectResource
    {
        public string? GetCostEstimatorLink()
        {
            return null;
        }

        protected Dictionary<string, object> parameters;

        public (Type, IDictionary<string, object>)[] GetActiveResources()
        {
            if (IsServiceAvailable && IsServiceConfigured)
                return new[] { GetComponent() };
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

        protected (Type type, IDictionary<string, object> parameters) GetComponent()
            => (ComponentType, parameters);

        public abstract string[] GetTags();
        public async Task<bool> InitializeAsync(Datahub_Project project, string userId, User graphUser, bool isProjectAdmin)
        {
            if (userId is null)
                return false;
            Project = project;
            parameters = new Dictionary<string, object>
                {
                    { nameof(InactiveResource.Title), Title },
                    { nameof(InactiveResource.Description), Description },
                    { nameof(InactiveResource.Icon), Icon },
                    { nameof(InactiveResource.IsIconSVG), IsIconSVG } };
            await InitializeAsync(userId, graphUser, isProjectAdmin);
            return true;
        }

        protected abstract Task InitializeAsync(string userId, User graphUser, bool isProjectAdmin);
        protected Dictionary<string, object> GetInactiveParameters()
            => new Dictionary<string, object>
                {
                    { nameof(InactiveResource.Title), Title },
                    { nameof(InactiveResource.Description), Description },
                    { nameof(InactiveResource.Icon), Icon },
                    { nameof(InactiveResource.IsIconSVG), IsIconSVG },
                    { nameof(InactiveResource.ResourceRequested),IsServiceRequested  },
                    { nameof(InactiveResource.Project), Project },
                };
    }
}
