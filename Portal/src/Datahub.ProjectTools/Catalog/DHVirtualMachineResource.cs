using Datahub.Core.Configuration;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.Projects;
using Datahub.ProjectTools.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;

namespace Datahub.ProjectTools.Catalog;

public class DHVirtualMachineResource : ActiveGitModuleResource
{
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
    private bool _serviceRequested;
    private bool _serviceCreated;

    public DHVirtualMachineResource(IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        IOptions<DataProjectsConfiguration> configuration)
    {
        _dbContextFactory = dbContextFactory;
        IsServiceConfigured = configuration.Value.VirtualMachines;
    }

    public override string[] GetTags()
    {
        throw new NotImplementedException();
    }

    protected override async Task InitializeAsync(string? userId, User graphUser, bool isProjectAdmin)
    {
        await using var projectDbContext = await _dbContextFactory.CreateDbContextAsync();
        var serviceRequests = Project.ServiceRequests;
        var serviceTerraformTemplateName =
            RequestManagementService.GetTerraformServiceType(IRequestManagementService.DATABRICKS);
        // _serviceRequested = serviceRequests.Any(r => r.RequestType == serviceTerraformTemplateName && r.Is_Completed == null);
        // _serviceCreated = serviceRequests.Any(r => r.RequestType == serviceTerraformTemplateName && r.Is_Completed != null);
    }

    protected override Type ComponentType { get; }
    protected override bool IsServiceRequested => _serviceRequested && !_serviceCreated;
    protected override string Title { get; }
    protected override string Description { get; }
    protected override string Icon { get; }
    protected override bool IsIconSVG { get; }
    protected override bool IsServiceConfigured { get; }
    protected override bool IsServiceAvailable { get; }
}