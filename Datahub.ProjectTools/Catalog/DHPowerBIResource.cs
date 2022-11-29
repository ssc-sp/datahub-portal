using Datahub.Core.Components;
using Datahub.Core.Components.Resources;
using Datahub.Core.Configuration;
using Datahub.Core.Data;
using Datahub.Core.EFCore;
using Datahub.Core.Services;
using Datahub.Core.Services.Projects;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.ProjectTools.Catalog
{

#nullable enable
    public class DHPowerBIResource : IProjectResource
    {
        private readonly IDbContextFactory<DatahubProjectDBContext> dbFactoryProject;
        private readonly bool isEnabled;

        public DHPowerBIResource(IDbContextFactory<DatahubProjectDBContext> dbFactoryProject,
            IOptions<DataProjectsConfiguration> configuration)
        {
            this.dbFactoryProject = dbFactoryProject;
            isEnabled = configuration.Value.PowerBI;
        }

        private Dictionary<string, object> parameters = new Dictionary<string, object>();

        public async Task<bool> InitializeAsync(Datahub_Project project, string userId, Microsoft.Graph.User graphUser, bool isProjectAdmin)
        {
            if (userId is null)
                return false;
            _project = project;
            await using var projectDbContext = await dbFactoryProject.CreateDbContextAsync();
            var serviceRequests = project.ServiceRequests;
            _powerBiServiceRequested = serviceRequests.Any(r => r.ServiceType == IRequestManagementService.POWERBI && r.Is_Completed == null);
            _powerBiServiceCreated = !string.IsNullOrEmpty(_project.PowerBI_URL) ||
                serviceRequests.Any(r => r.ServiceType == IRequestManagementService.POWERBI && r.Is_Completed.HasValue);
            parameters.Add(nameof(PowerBI.Project), _project);
            return true;
        }

        private string Title { get; } = "Power BI Workspace";
        private string Description { get; } = "Access the project Workspace in the Power BI Service";
        private string Icon { get; } = "/icons/svg/powerbi.svg";

        private Dictionary<string, object> GetInactiveParameters()
        => new Dictionary<string, object>
            {
                { nameof(InactiveResource.Title), Title },
                { nameof(InactiveResource.Description), Description },
                { nameof(InactiveResource.Icon), Icon },
                { nameof(InactiveResource.IsIconSVG), true },
                { nameof(InactiveResource.ResourceRequested), _powerBiServiceRequested },
                { nameof(InactiveResource.Project), _project },
            };

        private (Type type, IDictionary<string, object> parameters) GetComponent()
            => (typeof(PowerBI), parameters);        

        public (Type, IDictionary<string, object>)[] GetActiveResources()
        {
            if (_powerBiServiceCreated && isEnabled)
                return new[] { GetComponent() };
            else
                return Array.Empty<(Type type, IDictionary<string, object> parameters)>();
        }

        public string? GetCostEstimatorLink()
        {
            return null;
        }

        public (Type type, IDictionary<string, object> parameters)? GetInactiveResource()
        {
            if (_powerBiServiceCreated || !isEnabled)
                return null;
            return (typeof(PowerBI), GetInactiveParameters());
        }

        public string[] GetTags()
        {
            return new[] { "Power BI", "Analytics" };
        }

        public string UserId { get; set; }

        private bool _powerBiServiceRequested = false;
        private bool _powerBiServiceCreated = false;

        private Datahub_Project _project;



    }
#nullable disable
}
