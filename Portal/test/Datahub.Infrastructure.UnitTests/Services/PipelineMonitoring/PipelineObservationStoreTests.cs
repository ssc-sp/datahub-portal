using System.Text.Json;
using System.Text.Json.Serialization;
using Datahub.Core.Model.Context;
using Datahub.Infrastructure.Services.PipelineMonitoring;
using Datahub.Shared.Entities.PipelineMonitoring;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Infrastructure.UnitTests.Services.PipelineMonitoring;

[TestFixture]
public class PipelineObservationStoreTests
{
    private readonly PipelineMonitoringSource _source = new()
    {
        Organization = "test-org", ProjectId = Guid.NewGuid(), Environment = "dev", DefinitionId = 94
    };
    private readonly DateTime _now = new(2026, 11, 17, 12, 0, 0, DateTimeKind.Utc);
    private string _databasePath = null!;
    private TestFactory _factory = null!;
    private PipelineObservationStore _store = null!;

    [SetUp]
    public async Task SetUp()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"pipeline-monitoring-{Guid.NewGuid():N}.db");
        _factory = new TestFactory(new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseSqlite($"Data Source={_databasePath};Pooling=False;Default Timeout=10").Options);
        await using var context = _factory.CreateDbContext();
        await context.Database.EnsureCreatedAsync();
        _store = new PipelineObservationStore(_factory, [_source]);
    }

    [TearDown]
    public void TearDown()
    {
        File.Delete(_databasePath);
        File.Delete(_databasePath + "-wal");
        File.Delete(_databasePath + "-shm");
    }

    [TestCase(PipelineExecutionResult.Failed)]
    [TestCase(PipelineExecutionResult.Cancelled)]
    [TestCase(PipelineExecutionResult.Succeeded)]
    [TestCase(PipelineExecutionResult.Unknown)]
    public async Task PreservesRunAndIndependentWorkspaceJobOutcomes(PipelineExecutionResult result)
    {
        var message = Run() with { Result = result };
        Assert.That(await _store.AppendAsync(message), Is.True);
        var history = await _store.ReadHistoryAsync(_source, "Run", workspaceAcronym: "WATER");
        var saved = JsonSerializer.Deserialize<PipelineRunObservationMessage>(history.Single().PayloadJson,
            new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } })!;
        Assert.Multiple(() =>
        {
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved.Result, Is.EqualTo(result));
            Assert.That(saved.Workspaces.Select(w => w.Acronym), Is.EquivalentTo(new[] { "WATER", "CLIMATE" }));
            Assert.That(saved.Records[0].Result, Is.EqualTo(PipelineExecutionResult.Succeeded));
            Assert.That(saved.Records[1].Result, Is.EqualTo(PipelineExecutionResult.Failed));
            Assert.That(saved.SourceChangedAtUtc!.Value.Kind, Is.EqualTo(DateTimeKind.Utc));
        });
    }

    [Test]
    public async Task QueuedRunCanBeStoredBeforeSourceVersionOrTimelineIsAvailable()
    {
        var queued = Run() with
        {
            State = PipelineExecutionState.Queued, Result = PipelineExecutionResult.None,
            SourceBranch = null, SourceVersion = null, SourceChangedAtUtc = null,
            TimelineComplete = false, Workspaces = [], Records = []
        };
        Assert.That(await _store.AppendAsync(queued), Is.True);
        var history = await _store.ReadHistoryAsync(_source, "Run");
        Assert.That(history.Single().PayloadJson, Does.Contain("\"State\":\"Queued\""));
    }

    [Test]
    public async Task IdenticalRedeliveryIsIdempotentAndIdReuseCannotRewriteHistory()
    {
        var message = Run();
        Assert.That(await _store.AppendAsync(message), Is.True);
        Assert.That(await _store.AppendAsync(message), Is.False);
        await Assert.ThrowsAsync<InvalidOperationException>(() => _store.AppendAsync(message with { RunName = "changed" }));
        Assert.That(await _store.ReadHistoryAsync(_source, "Run"), Has.Count.EqualTo(1));
    }

    [Test]
    public async Task ConcurrentRedeliveriesPersistOneObservationAndOneSetOfWorkspaceLinks()
    {
        var message = Run();
        var results = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => _store.AppendAsync(message)));
        await using var context = _factory.CreateDbContext();
        Assert.Multiple(() =>
        {
            Assert.That(results.Count(inserted => inserted), Is.EqualTo(1));
            Assert.That(context.PipelineObservations.Count(), Is.EqualTo(1));
            Assert.That(context.Set<Datahub.Core.Model.PipelineMonitoring.PipelineObservationWorkspace>().Count(), Is.EqualTo(2));
        });
    }

    [Test]
    public async Task DelayedObservationDoesNotOverwriteNewerEvidenceAndRetriesRetainAttemptIdentity()
    {
        var completed = Run();
        var retry = completed with
        {
            ObservationId = Guid.NewGuid(), ObservedAtUtc = _now.AddMinutes(1),
            Records = [completed.Records[1] with { Attempt = 2, ChangeId = 9, Result = PipelineExecutionResult.Succeeded }]
        };
        await _store.AppendAsync(retry);
        await _store.AppendAsync(completed);
        await _store.AppendAsync(completed with
        {
            ObservationId = Guid.NewGuid(), ObservedAtUtc = _now.AddMinutes(-1), State = PipelineExecutionState.Running,
            Result = PipelineExecutionResult.None
        });
        var history = await _store.ReadHistoryAsync(_source, "Run", runId: completed.RunId);
        Assert.Multiple(() =>
        {
            Assert.That(history, Has.Count.EqualTo(3));
            Assert.That(history[0].Id, Is.EqualTo(retry.ObservationId));
            Assert.That(history[1].Id, Is.EqualTo(completed.ObservationId));
            Assert.That(history[0].PayloadJson, Does.Contain("\"Attempt\":2"));
            Assert.That(history[1].PayloadJson, Does.Contain("\"Attempt\":1"));
        });
    }

    [Test]
    public async Task UnknownAndDeletedWorkspacesDoNotRequirePortalWorkspaceRows()
    {
        var unknown = Run() with { Workspaces = [], Records = [], TimelineComplete = false };
        await _store.AppendAsync(unknown);
        await _store.AppendAsync(Run());
        await using var context = _factory.CreateDbContext();
        Assert.Multiple(() =>
        {
            Assert.That(context.Projects.Count(), Is.Zero);
            Assert.That(context.Project_Resources2.Count(), Is.Zero);
            Assert.That(context.PipelineObservations.Count(), Is.EqualTo(2));
        });
    }

    [Test]
    public async Task IdleHealthAndFailedScansRoundTripWithoutInventingExecutions()
    {
        var healthy = new PipelineMonitorHealthMessage
        {
            ObservationId = Guid.NewGuid(), Source = _source, ObservedAtUtc = _now,
            Status = PipelineMonitorStatus.Healthy, LastCompleteScanAtUtc = _now
        };
        await _store.AppendAsync(healthy);
        await _store.AppendAsync(healthy with
        {
            ObservationId = Guid.NewGuid(), ObservedAtUtc = _now.AddMinutes(1),
            Status = PipelineMonitorStatus.Failed, ErrorCode = "ADO_READ_FAILED"
        });
        Assert.That(await _store.ReadHistoryAsync(_source, "Health"), Has.Count.EqualTo(2));
        Assert.That(await _store.ReadHistoryAsync(_source, "Run"), Is.Empty);
        await Assert.ThrowsAsync<ArgumentException>(() => _store.AppendAsync(healthy with { ErrorCode = "scan failed" }));
    }

    [Test]
    public async Task RejectsUnsupportedSourcesVersionsTimestampsAndAmbiguousMappings()
    {
        var run = Run();
        await Assert.ThrowsAsync<ArgumentException>(() => _store.AppendAsync(run with { SchemaVersion = 2 }));
        await Assert.ThrowsAsync<ArgumentException>(() => _store.AppendAsync(run with { Source = _source with { Environment = "prd" } }));
        await Assert.ThrowsAsync<ArgumentException>(() => _store.AppendAsync(run with { ObservedAtUtc = DateTime.SpecifyKind(_now, DateTimeKind.Unspecified) }));
        await Assert.ThrowsAsync<ArgumentException>(() => _store.AppendAsync(run with { Workspaces = [new() { Acronym = "WATER", Evidence = PipelineWorkspaceEvidence.Unknown }] }));
        await Assert.ThrowsAsync<ArgumentException>(() => _store.AppendAsync(run with { Records = [run.Records[0], run.Records[0]] }));
        await Assert.ThrowsAsync<ArgumentException>(() => _store.AppendAsync(run with { Records = [run.Records[0] with { WorkspaceAcronym = "UNRELATED" }] }));
    }

    [Test]
    public async Task PersistenceFailuresPropagateAndCanBeRetriedAfterRecovery()
    {
        await using (var context = _factory.CreateDbContext())
            await context.Database.ExecuteSqlRawAsync("DROP TABLE PipelineObservationWorkspaces");
        var message = Run();
        await Assert.ThrowsAsync<DbUpdateException>(() => _store.AppendAsync(message));
        await using (var context = _factory.CreateDbContext())
        {
            Assert.That(await context.PipelineObservations.CountAsync(), Is.Zero, "A failed workspace insert must roll back the observation.");
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
        }
        Assert.That(await _store.AppendAsync(message), Is.True);
    }

    [Test]
    public async Task SqliteMigrationCanBeAppliedRolledBackAndReappliedWithoutChangingWorkspaceTables()
    {
        await using var context = _factory.CreateDbContext();
        await context.Database.ExecuteSqlRawAsync("DROP TABLE PipelineObservationWorkspaces");
        await context.Database.ExecuteSqlRawAsync("DROP TABLE PipelineObservations");
        var migration = new Core.Migrations.SqliteDatahub.PipelineMonitoringFoundation();
        var generator = context.GetService<IMigrationsSqlGenerator>();
        foreach (var command in generator.Generate(migration.UpOperations))
            await context.Database.ExecuteSqlRawAsync(command.CommandText);
        Assert.That(await _store.AppendAsync(Run()), Is.True);
        foreach (var command in generator.Generate(migration.DownOperations))
            await context.Database.ExecuteSqlRawAsync(command.CommandText);
        Assert.That(await context.Projects.CountAsync(), Is.Zero);
        foreach (var command in generator.Generate(migration.UpOperations))
            await context.Database.ExecuteSqlRawAsync(command.CommandText);
        Assert.That(await _store.AppendAsync(Run()), Is.True);
    }

    [Test]
    public void SqlServerMigrationOnlyCreatesMonitoringTablesAndHasARollback()
    {
        using var context = new SqlServerDatahubContext(new DbContextOptionsBuilder<SqlServerDatahubContext>()
            .UseSqlServer("Server=unused;Database=unused;Integrated Security=True").Options);
        var migration = new Core.Migrations.PipelineMonitoringFoundation();
        var generator = context.GetService<IMigrationsSqlGenerator>();
        var up = string.Join("\n", generator.Generate(migration.UpOperations).Select(c => c.CommandText));
        var down = string.Join("\n", generator.Generate(migration.DownOperations).Select(c => c.CommandText));
        Assert.Multiple(() =>
        {
            Assert.That(up, Does.Contain("CREATE TABLE [PipelineObservations]"));
            Assert.That(up, Does.Contain("CREATE TABLE [PipelineObservationWorkspaces]"));
            Assert.That(up, Does.Not.Contain("ALTER TABLE [Projects]"));
            Assert.That(down, Does.Contain("DROP TABLE [PipelineObservationWorkspaces]"));
            Assert.That(down, Does.Contain("DROP TABLE [PipelineObservations]"));
        });
    }

    [Test]
    public void LinksStayWithinTheApprovedAzureDevOpsHost()
    {
        var url = PipelineObservationStore.GetRunUrl(_source, 17191);
        Assert.That(url.Host, Is.EqualTo("dev.azure.com"));
        Assert.That(url.Query, Is.EqualTo("?buildId=17191"));
        Assert.Throws<ArgumentException>(() => PipelineObservationStore.GetRunUrl(_source with { Organization = "evil.example/path" }, 17191));
    }

    private PipelineRunObservationMessage Run()
    {
        var timelineId = Guid.NewGuid();
        return new PipelineRunObservationMessage
        {
            ObservationId = Guid.NewGuid(), Source = _source, ObservedAtUtc = _now, SourceChangedAtUtc = _now,
            RunId = 17191, PipelineName = "execution", RunName = "workspace-dev-WATER", SourceBranch = "refs/heads/main",
            SourceVersion = "example-commit", State = PipelineExecutionState.Completed, Result = PipelineExecutionResult.Failed,
            TimelineComplete = true,
            Workspaces = [new() { Acronym = "WATER", Evidence = PipelineWorkspaceEvidence.RunParameter },
                new() { Acronym = "CLIMATE", Evidence = PipelineWorkspaceEvidence.RunParameter }],
            Records = [new() { TimelineId = timelineId, RecordId = Guid.NewGuid(), Attempt = 1, ChangeId = 7, RecordType = "Job",
                Identifier = "Project_WATER", Name = "WATER", WorkspaceAcronym = "WATER", WorkspaceEvidence = PipelineWorkspaceEvidence.JobIdentifier,
                State = PipelineExecutionState.Completed, Result = PipelineExecutionResult.Succeeded },
                new() { TimelineId = timelineId, RecordId = Guid.NewGuid(), Attempt = 1, ChangeId = 8, RecordType = "Job",
                Identifier = "Project_CLIMATE", Name = "CLIMATE", WorkspaceAcronym = "CLIMATE", WorkspaceEvidence = PipelineWorkspaceEvidence.JobIdentifier,
                State = PipelineExecutionState.Completed, Result = PipelineExecutionResult.Failed }]
        };
    }

    private sealed class TestFactory(DbContextOptions<DatahubProjectDBContext> options) : IDbContextFactory<DatahubProjectDBContext>
    {
        public DatahubProjectDBContext CreateDbContext() => new(options);
    }
}
