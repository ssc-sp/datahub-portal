using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.Projects;
using Datahub.Core.Services.ProjectTools;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.ProjectTools.Catalog
{
    public class DHGitModuleResource : IProjectResource
    {
        private GitHubModule currentModule;
        private GitHubModuleDescriptor enDescriptor;
        private Datahub_Project project;
        private bool serviceRequested;
        private readonly IDbContextFactory<DatahubProjectDBContext> dbFactoryProject;

        public DHGitModuleResource(IDbContextFactory<DatahubProjectDBContext> dbFactoryProject)
        {
            this.dbFactoryProject = dbFactoryProject;
        }

        public (Type type, IDictionary<string, object> parameters)[] GetActiveResources()
        {
            return Array.Empty<(Type type, IDictionary<string, object> parameters)>();
        }

        public string? GetCostEstimatorLink()
        {
            return null;
        }

        public (Type type, IDictionary<string, object> parameters)? GetInactiveResource()
        {
            return (typeof(InactiveResource), GetInactiveParameters());
        }

        protected Dictionary<string, object> GetInactiveParameters()
            => new Dictionary<string, object>
                {
                    { nameof(InactiveResource.Title), enDescriptor.Title },
                    { nameof(InactiveResource.Description), enDescriptor.Description },
                    { nameof(InactiveResource.Icon), currentModule.Icon },
                    { nameof(InactiveResource.IsIconSVG), false },
                    { nameof(InactiveResource.ResourceRequested),serviceRequested  },
                    { nameof(InactiveResource.Project), project },
                };


        public string[] GetTags() => enDescriptor.Tags;

        public async Task<bool> InitializeAsync(Datahub_Project project, string userId, User graphUser, bool isProjectAdmin)
        {
            await using var projectDbContext = await dbFactoryProject.CreateDbContextAsync();
            this.project = project;
            var serviceRequests = project.ServiceRequests;
            serviceRequested = serviceRequests.Any(r => r.ServiceType == $"terraform:{currentModule.Path}" && r.Is_Completed == null);
            return true;
        }

        public void ConfigureGitModule(GitHubModule item)
        {
            currentModule = item;
            enDescriptor = item.Descriptors.First(l => l.Language == "en");
        }
    }
}
