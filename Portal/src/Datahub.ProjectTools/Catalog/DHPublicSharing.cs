using Datahub.Core.Configuration;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Datahub.ProjectTools.Catalog;

#nullable enable
public class DHPublicSharing : IProjectResource
{
	private readonly IDbContextFactory<DatahubProjectDBContext> dbFactoryProject;
	private readonly IPublicDataFileService publicDataFileService;
	private readonly bool isEnabled;
	private bool isDataApprover;
	private int sharingRequestAwaitingApprovalCount;
	private int ownSharingRequestCount;

	public DHPublicSharing(IDbContextFactory<DatahubProjectDBContext> dbFactoryProject,
		IPublicDataFileService publicDataFileService,
		IOptions<DataProjectsConfiguration> configuration)
	{
		this.dbFactoryProject = dbFactoryProject;
		this.publicDataFileService = publicDataFileService;
		isEnabled = configuration.Value.PublicSharing;
	}

	private Dictionary<string, object> parameters = new Dictionary<string, object>();

	public async Task<bool> InitializeAsync(Datahub_Project project, string? userId, Microsoft.Graph.Models.User graphUser, bool isProjectAdmin)
	{
		await using var projectDbContext = await dbFactoryProject.CreateDbContextAsync();
		isDataApprover = await projectDbContext.Project_Users
			.Where(u => u.User_ID == userId && project == u.Project)
			.AnyAsync(u => u.RoleId == (int)Project_Role.RoleNames.Admin || u.RoleId == (int)Project_Role.RoleNames.WorkspaceLead);
		parameters.Add(nameof(PublicSharing.isDataApprover), isDataApprover);
		sharingRequestAwaitingApprovalCount = await publicDataFileService.GetDataSharingRequestsAwaitingApprovalCount(project.Project_Acronym_CD);
		parameters.Add(nameof(PublicSharing.sharingRequestAwaitingApprovalCount), sharingRequestAwaitingApprovalCount);
		ownSharingRequestCount = await publicDataFileService.GetUsersOwnDataSharingRequestsCount(project.Project_Acronym_CD, userId);
		parameters.Add(nameof(PublicSharing.ownSharingRequestCount), ownSharingRequestCount);
		parameters.Add(nameof(PublicSharing.ProjectAcronym), project.Project_Acronym_CD);

		return true;
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
#nullable disable