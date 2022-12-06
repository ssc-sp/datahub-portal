using Datahub.Core.Configuration;
using Datahub.Core.EFCore;
using Datahub.Core.Services;
using Datahub.Core.Services.Projects;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.ProjectTools.Catalog
{
    public class DHStorageResource: DHURLResource
    {
        private readonly IDbContextFactory<DatahubProjectDBContext> dbFactoryProject;
        private readonly bool _isServiceConfigured;
        private readonly bool isServiceConfigured;

        public DHStorageResource(IDbContextFactory<DatahubProjectDBContext> dbFactoryProject,
            IOptions<DataProjectsConfiguration> configuration)
        {
            this.dbFactoryProject = dbFactoryProject;
            _isServiceConfigured = configuration.Value.Storage;
        }

        private bool _azStorageServiceRequested = false;
        private bool _azStorageServiceCreated = false;

        protected override async Task InitializeAsync(string userId, Microsoft.Graph.User graphUser, bool isProjectAdmin)
        {
            await using var projectDbContext = await dbFactoryProject.CreateDbContextAsync();
            var serviceRequests = Project.ServiceRequests.Where(r => r.ServiceType == IRequestManagementService.STORAGE).ToList();
            _azStorageServiceRequested = serviceRequests.Any(r=> r.Is_Completed == null);
            _azStorageServiceCreated = serviceRequests.Any(pr => pr.Is_Completed != null); 

            parameters.Add(nameof(Storage.Project), Project);
        }

        protected override string Title => "Project Storage";
        protected override string Description => "Access the storage account for this project to upload, download and share datasets with all project members.";
        protected override string Icon => "fas fa-hdd fa-2x";
        protected override bool IsIconSVG => false;

        protected override Type ComponentType => typeof(Storage);

        protected override bool IsServiceRequested => _azStorageServiceRequested && !_azStorageServiceCreated;

        protected override bool IsServiceConfigured => _isServiceConfigured;

        protected override bool IsServiceAvailable => _azStorageServiceCreated;

        public override string[] GetTags()
        {
            return new[] { "Cloud Storage", "Azure"};
        }
    }
}
