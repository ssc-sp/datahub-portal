using Datahub.Core.Components.Resources;
using Datahub.Core.Configuration;
using Datahub.Core.EFCore;
using Datahub.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Resources
{

#nullable enable
    public class DHPublicSharing : IProjectResource
    {
        private readonly IDbContextFactory<DatahubProjectDBContext> dbFactoryProject;
        private readonly RequestManagementService requestManagementService;
        private readonly IPublicDataFileService publicDataFileService;
        private readonly bool isEnabled;
        private bool isDataApprover;
        private int sharingRequestAwaitingApprovalCount;
        private int ownSharingRequestCount;

        public DHPublicSharing(IDbContextFactory<DatahubProjectDBContext> dbFactoryProject, 
            RequestManagementService requestManagementService, IPublicDataFileService publicDataFileService,
            IOptions<DataProjectsConfiguration> configuration)
        {
            this.dbFactoryProject = dbFactoryProject;
            this.requestManagementService = requestManagementService;
            this.publicDataFileService = publicDataFileService;
            isEnabled = configuration.Value.PublicSharing;
        }

        private Dictionary<string, object> parameters = new Dictionary<string, object>();

        public async Task Initialize(Datahub_Project project, string userId, Microsoft.Graph.User graphUser)
        {
            await using var projectDbContext = await dbFactoryProject.CreateDbContextAsync();
            if (userId != null)
            {
                isDataApprover = await projectDbContext.Project_Users
                    .Where(u => u.User_ID == userId && project == u.Project)
                    .AnyAsync(u => u.IsDataApprover);
                parameters.Add(nameof(isDataApprover), isDataApprover);
                sharingRequestAwaitingApprovalCount = await publicDataFileService.GetDataSharingRequestsAwaitingApprovalCount(project.Project_Acronym_CD);
                parameters.Add(nameof(sharingRequestAwaitingApprovalCount), sharingRequestAwaitingApprovalCount);
                ownSharingRequestCount = await publicDataFileService.GetUsersOwnDataSharingRequestsCount(project.Project_Acronym_CD, userId);
                parameters.Add(nameof(ownSharingRequestCount), ownSharingRequestCount);
            }
        }

        private (Type type, IDictionary<string, object> parameters) GetComponent()
        {
            return (typeof(PublicSharing), parameters);
        }

        public (Type, IDictionary<string, object>)[] GetActiveResources()
        {
            if (ownSharingRequestCount > 0 && isEnabled)
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
            if (ownSharingRequestCount > 0 || !isEnabled)
                return null;
            return GetComponent();
        }

        public string[] GetTags()
        {
            return new[] { "Public Sharing", "Open Data" };
        }
    }
}
