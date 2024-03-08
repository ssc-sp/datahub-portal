using Datahub.Core.Model.Projects;
using Microsoft.Graph.Models;

namespace Datahub.ProjectTools;
#nullable enable
public interface IProjectResource
{
	(Type type, IDictionary<string, object> parameters)[] GetActiveResources();
	(Type type, IDictionary<string, object> parameters)? GetInactiveResource();

	public string? GetCostEstimatorLink();

	public string[] GetTags();
	Task<bool> InitializeAsync(Datahub_Project project, string? userId, User graphUser, bool isProjectAdmin);
}