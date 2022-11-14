using Datahub.Core.Configuration;
using Datahub.Core.EFCore;
using Datahub.Core.Services;
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
    public class DHDatabricksResource : IProjectResource
    {
        private readonly IDbContextFactory<DatahubProjectDBContext> dbFactoryProject;
        private readonly RequestManagementService requestManagementService;
        private readonly IPublicDataFileService publicDataFileService;
        private readonly bool isEnabled;
        private bool isDataApprover;
        private int sharingRequestAwaitingApprovalCount;
        private int ownSharingRequestCount;
        private Datahub_Project _project;
        private bool _canViewDatabricks;

        public DHDatabricksResource(IDbContextFactory<DatahubProjectDBContext> dbFactoryProject,
            RequestManagementService requestManagementService, IPublicDataFileService publicDataFileService,
            IOptions<DataProjectsConfiguration> configuration)
        {
            this.dbFactoryProject = dbFactoryProject;
            this.requestManagementService = requestManagementService;
            this.publicDataFileService = publicDataFileService;
            isEnabled = configuration.Value.Databricks;
        }

        private static string GetPropertyName<T>(Expression<Func<T,object>> expression)
        {
            return GetMemberName(expression.Body);
        }

        private static string GetMemberName(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return ((MemberExpression)expression).Member.Name;
                case ExpressionType.Convert:
                    return GetMemberName(((UnaryExpression)expression).Operand);
                default:
                    throw new NotSupportedException(expression.NodeType.ToString());
            }
        }

        private (Type type, IDictionary<string, object> parameters) GetComponent()
        {
            var parameters = new Dictionary<string, object>();            
            parameters.Add(GetPropertyName<Databricks>(t => t.RequestManagementService), requestManagementService);
            parameters.Add(GetPropertyName<Databricks>(t => t.Project), _project);
            return (typeof(Databricks), parameters);
        }

        public (Type type, IDictionary<string, object> parameters)[] GetActiveResources()
        {
            if (!isEnabled || !_canViewDatabricks)
            {
                return Array.Empty<(Type type, IDictionary<string, object> parameters)>();
            }
            else
            {
                return new[] { GetComponent() }; 
            }
        }

        public string GetCostEstimatorLink()
        {
            throw new NotImplementedException();
        }

        public (Type type, IDictionary<string, object> parameters)? GetInactiveResource()
        {
            if (!isEnabled || !_canViewDatabricks)
            {
                return null;
            }
            else
            {
                return GetComponent();
            }
        }

        public string[] GetTags()
        {
            throw new NotImplementedException();
        }

        public async Task InitializeAsync(Datahub_Project project, string userId, User graphUser, bool isProjectAdmin)
        {
            _project = project;
            _canViewDatabricks = isProjectAdmin || !string.IsNullOrEmpty(project.Databricks_URL);

        }
    }
}
