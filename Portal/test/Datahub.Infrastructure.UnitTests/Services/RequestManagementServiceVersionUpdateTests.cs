using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services.Projects;
using Datahub.Infrastructure.Services;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Datahub.Infrastructure.UnitTests.Services;

[TestFixture]
public class RequestManagementServiceVersionUpdateTests
{
    private const string Email = "admin@example.com";

    private DbContextOptions<SqlServerDatahubContext> _options = null!;
    private IResourceMessagingService _messaging = null!;
    private RequestManagementService _sut = null!;

    [SetUp]
    public void Setup()
    {
        // A unique in-memory database per test so each test starts clean.
        _options = new DbContextOptionsBuilder<SqlServerDatahubContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _messaging = Substitute.For<IResourceMessagingService>();
        _messaging.CreateWorkspaceDefinition(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => DefinitionFor(call.ArgAt<string>(0)));

        _sut = new RequestManagementService(
            NullLogger<RequestManagementService>.Instance,
            new InMemoryContextFactory(_options),
            Substitute.For<IDatahubAuditingService>(),
            _messaging,
            Substitute.For<IWorkspaceVersionService>());
    }

    private sealed class InMemoryContextFactory(DbContextOptions<SqlServerDatahubContext> options)
        : IDbContextFactory<DatahubProjectDBContext>
    {
        public DatahubProjectDBContext CreateDbContext() => new SqlServerDatahubContext(options);
    }

    private WorkspaceDefinition DefinitionFor(string acronym)
    {
        using var ctx = new SqlServerDatahubContext(_options);
        var project = ctx.Projects.Single(p => p.Project_Acronym_CD == acronym);
        return new WorkspaceDefinition
        {
            Templates = [],
            Workspace = new TerraformWorkspace { Acronym = acronym, Version = project.Version },
            AppData = new WorkspaceAppData(),
            RequestingUserEmail = Email
        };
    }

    private async Task SeedAsync(params (string Acronym, string Version, DateTime? DeletedUtc)[] projects)
    {
        await using var ctx = new SqlServerDatahubContext(_options);
        foreach (var (acronym, version, deletedUtc) in projects)
        {
            ctx.Projects.Add(new Datahub_Project
            {
                Project_Acronym_CD = acronym,
                Project_Name = acronym,
                Version = version,
                Deleted_DT = deletedUtc
            });
        }

        await ctx.SaveChangesAsync();
    }

    [Test]
    public async Task TriggerBuildVersionUpdates_UpgradesOnlyLiveWorkspacesOnAnOlderPatchOfTheSameMinorVersion()
    {
        await SeedAsync(
            ("OLDPATCH", "v7.4.1", null),
            ("DELETED", "v7.4.1", DateTime.UtcNow.AddDays(-1)),
            ("DELETING", "v7.4.1", DateTime.UtcNow.AddMinutes(5)),
            ("CURRENT", "v7.4.3", null),
            ("OTHERMINOR", "v7.3.9", null));

        var result = await _sut.TriggerBuildVersionUpdates("v7.4.3", Email);

        Assert.That(result, Is.True);
        await _messaging.Received(1).SendToTerraformQueue(Arg.Any<WorkspaceDefinition>());
        await _messaging.Received(1).SendToTerraformQueue(Arg.Is<WorkspaceDefinition>(d =>
            d.Workspace.Acronym == "OLDPATCH" &&
            d.Workspace.Version == "v7.4.3" &&
            d.UpdateWorkspaceVersion));
    }
}
