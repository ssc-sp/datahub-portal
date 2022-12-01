﻿using Datahub.Core.Configuration;
using Datahub.Core.EFCore;
using Datahub.Core.Services;
using Datahub.Core.Services.Projects;
using Datahub.ProjectTools.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.ProjectTools.Catalog
{

#nullable enable
    public class DHDataEntry : DHURLResource
    { 

        private readonly IDbContextFactory<DatahubProjectDBContext> dbFactoryProject;
        private readonly bool _isServiceConfigured;

        public DHDataEntry(IDbContextFactory<DatahubProjectDBContext> dbFactoryProject,            
            IOptions<DataProjectsConfiguration> configuration)
        {
            this.dbFactoryProject = dbFactoryProject;
            _isServiceConfigured = true;// configuration.Value.;
        }

        private bool _dataEntryServiceRequested = false;
        private bool _dataEntryServiceCreated = false;

        protected override async Task InitializeAsync(string userId, Microsoft.Graph.User graphUser, bool isProjectAdmin)
        {
            await using var projectDbContext = await dbFactoryProject.CreateDbContextAsync();
            var serviceRequests = Project.ServiceRequests;
            _dataEntryServiceRequested = false; //serviceRequests.Any(r => r.ServiceType == IRequestManagementService. && r.Is_Completed == null);
            _dataEntryServiceCreated = !string.IsNullOrEmpty(Project.WebForms_URL);
            parameters.Add(nameof(DataEntry.Project), Project);
        }

        protected override string Title => "Data Entry";
        protected override string Description => "Start the Data Entry application enabled for this project";
        protected override string Icon => "fad fa-keyboard card-icon fa-xs";
        protected override bool IsIconSVG => false;

        protected override Type ComponentType => typeof(DataEntry);

        protected override bool IsServiceRequested => _dataEntryServiceRequested && !_dataEntryServiceCreated;

        protected override bool IsServiceConfigured => _isServiceConfigured;

        protected override bool IsServiceAvailable => _dataEntryServiceCreated;
        protected override bool IsRequestAvailable => false;


        public override string[] GetTags()
        {
            return new[] { "Data Entry", "Web Forms" };
        }


    }
#nullable disable
}
