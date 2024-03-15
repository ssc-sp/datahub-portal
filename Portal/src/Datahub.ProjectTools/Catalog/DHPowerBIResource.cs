using Datahub.Core.Configuration;
using Datahub.Core.Model.Datahub;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Datahub.ProjectTools.Catalog;

#nullable enable
public class DHPowerBIResource : ActiveGitModuleResource
{
    private readonly IDbContextFactory<DatahubProjectDBContext> dbFactoryProject;
    private readonly bool isServiceConfigured;

    public DHPowerBIResource(IDbContextFactory<DatahubProjectDBContext> dbFactoryProject,
        IOptions<DataProjectsConfiguration> configuration)
    {
        this.dbFactoryProject = dbFactoryProject;
        isServiceConfigured = configuration.Value.PowerBI;
    }

    private bool _powerBiServiceRequested = false;
    private bool _powerBiServiceCreated = false;

    protected override async Task InitializeAsync(string? userId, Microsoft.Graph.Models.User graphUser, bool isProjectAdmin)
    {
        await using var projectDbContext = await dbFactoryProject.CreateDbContextAsync();
        // var serviceRequests = Project.ProjectRequestAudits;
        // _powerBiServiceRequested = serviceRequests.Any(r => r.RequestType == IRequestManagementService.POWERBI && r.Is_Completed == null);
        _powerBiServiceCreated = !string.IsNullOrEmpty(Project.PowerBI_URL);
        Parameters.Add(nameof(PowerBI.Project), Project);            
    }

    protected override string Title => "Power BI Workspace";
    protected override string Description => "Access the project Workspace in the Power BI Service";
    protected override string Icon => "/icons/svg/powerbi.svg";
    protected override bool IsIconSVG => true;

    protected override Type ComponentType => typeof(PowerBI);

    protected override bool IsServiceRequested => _powerBiServiceRequested && !_powerBiServiceCreated;

    protected override bool IsServiceConfigured => isServiceConfigured;

    protected override bool IsServiceAvailable => _powerBiServiceCreated;

    public override string[] GetTags()
    {
        return new[] { "Power BI", "Analytics" };
    }


}
#nullable disable