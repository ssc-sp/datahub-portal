using Datahub.Core.Configuration;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.Projects;
using Datahub.ProjectTools.Services;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Datahub.ProjectTools.Catalog;

#nullable enable
public class DHDatabricksResource : ActiveGitModuleResource
{

	private readonly IDbContextFactory<DatahubProjectDBContext> _dbFactoryProject;

	private bool _databricksServiceRequested;
	private bool _databricksServiceCreated;

	public DHDatabricksResource(IDbContextFactory<DatahubProjectDBContext> dbFactoryProject,
		IOptions<DataProjectsConfiguration> configuration)
	{
		_dbFactoryProject = dbFactoryProject;
		IsServiceConfigured = configuration.Value.Databricks;
	}

	protected override async Task InitializeAsync(string? userId, Microsoft.Graph.Models.User graphUser, bool isProjectAdmin)
	{
		await using var projectDbContext = await _dbFactoryProject.CreateDbContextAsync();
		// var serviceRequests = Project.ProjectRequestAudits;
		// var serviceTerraformTemplateName =
		// TerraformTemplate.GetTerraformServiceType(IRequestManagementService.DATABRICKS);
		// _databricksServiceRequested = serviceRequests.Any(r => r.RequestType == serviceTerraformTemplateName && r.Is_Completed == null);
		// _databricksServiceCreated = serviceRequests.Any(r => r.RequestType == serviceTerraformTemplateName && r.Is_Completed != null);

		Parameters.Add(nameof(Databricks.Project), Project);
	}

	protected override string Title => "Azure Databricks";
	protected override string Description => "Run your Python, R and SQL notebooks in the cloud with Databricks for analytics, machine learning and data pipelines";
	protected override string Icon => "/icons/svg/databricks.svg";
	protected override bool IsIconSVG => true;

	protected override Type ComponentType => typeof(Databricks);

	protected override bool IsServiceRequested => _databricksServiceRequested;


	protected override bool IsServiceConfigured { get; }

	protected override bool IsServiceAvailable => _databricksServiceCreated;

	public override string[] GetTags()
	{
		return new[] { "Databricks", "Jupyter Notebooks", "Analytics" };
	}


}
#nullable disable