using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.PipelineMonitoring;
using Datahub.Shared.Entities.PipelineMonitoring;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Services.PipelineMonitoring;

/// <summary>Stores immutable snapshots; live consumers and current-state projections are separate concerns.</summary>
public sealed class PipelineObservationStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };

    private readonly IDbContextFactory<DatahubProjectDBContext> _factory;
    private readonly HashSet<PipelineMonitoringSource> _sources;
    private readonly TimeProvider _time;

    public PipelineObservationStore(IDbContextFactory<DatahubProjectDBContext> factory,
        IEnumerable<PipelineMonitoringSource> approvedSources, TimeProvider? timeProvider = null)
    {
        _factory = factory;
        _sources = approvedSources.ToHashSet();
        _time = timeProvider ?? TimeProvider.System;
        if (_sources.Count == 0)
            throw new ArgumentException("At least one explicitly approved monitoring source is required.", nameof(approvedSources));
        foreach (var source in _sources)
            ValidateSource(source);
    }

    /// <summary>Returns false for an identical redelivery; conflicting reuse of an ID is rejected.</summary>
    public Task<bool> AppendAsync(PipelineRunObservationMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ValidateEnvelope(message.SchemaVersion, message.ObservationId, message.Source, message.ObservedAtUtc);
        Require(message.RunId > 0, "Run ID must be positive.");
        Text(message.PipelineName, 256, "Pipeline name");
        Text(message.RunName, 256, "Run name");
        if (message.SourceBranch is not null)
            Text(message.SourceBranch, 256, "Source branch");
        if (message.SourceVersion is not null)
            Text(message.SourceVersion, 128, "Source version");
        Require(Enum.IsDefined(message.State) && Enum.IsDefined(message.Result), "Unsupported execution state or result.");
        Utc(message.SourceChangedAtUtc);
        Utc(message.QueuedAtUtc);
        Utc(message.StartedAtUtc);
        Utc(message.FinishedAtUtc);
        Require(message.Workspaces is not null && message.Workspaces.Length <= 256, "Invalid workspace collection.");
        Require(message.Records is not null && message.Records.Length <= 1024, "Invalid timeline collection.");
        var workspaces = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var workspace in message.Workspaces!)
        {
            Require(workspace is not null, "Null workspace reference.");
            Text(workspace!.Acronym, 64, "Workspace acronym");
            Require(workspaces.Add(workspace.Acronym), "Duplicate workspace reference.");
            Require(Enum.IsDefined(workspace.Evidence) && workspace.Evidence != PipelineWorkspaceEvidence.Unknown,
                "A named workspace requires mapping evidence.");
        }
        var records = new HashSet<(Guid, Guid, int)>();
        foreach (var record in message.Records!)
        {
            Require(record is not null, "Null timeline record.");
            Require(record!.TimelineId != Guid.Empty && record.RecordId != Guid.Empty && record.Attempt > 0 && record.ChangeId >= 0,
                "Invalid timeline identity, revision or attempt.");
            Require(records.Add((record.TimelineId, record.RecordId, record.Attempt)), "Duplicate timeline attempt.");
            Require(record.RecordType is "Stage" or "Job", "Only stage and job records are supported.");
            Text(record.Identifier, 512, "Timeline identifier");
            Text(record.Name, 256, "Timeline name");
            Require(Enum.IsDefined(record.State) && Enum.IsDefined(record.Result) && Enum.IsDefined(record.WorkspaceEvidence),
                "Unsupported timeline value.");
            if (record.WorkspaceAcronym is null)
                Require(record.WorkspaceEvidence == PipelineWorkspaceEvidence.Unknown, "Unmapped records must have unknown evidence.");
            else
                Require(workspaces.Contains(record.WorkspaceAcronym) && record.WorkspaceEvidence != PipelineWorkspaceEvidence.Unknown,
                    "Timeline workspace must have a corresponding evidenced run association.");
            Require(record.FailureSummary is null || record.FailureSummary.Length <= 2048, "Failure summary exceeds its limit.");
            Utc(record.StartedAtUtc);
            Utc(record.FinishedAtUtc);
        }
        return AppendAsync(message.ObservationId, message.Source, message.ObservedAtUtc, "Run", message.RunId,
            message.SourceChangedAtUtc, message, workspaces, cancellationToken);
    }

    /// <summary>Stores heartbeat evidence even when no execution exists.</summary>
    public Task<bool> AppendAsync(PipelineMonitorHealthMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ValidateEnvelope(message.SchemaVersion, message.ObservationId, message.Source, message.ObservedAtUtc);
        Require(Enum.IsDefined(message.Status), "Unsupported monitor health.");
        Utc(message.LastCompleteScanAtUtc);
        Require(message.LastCompleteScanAtUtc is null || message.LastCompleteScanAtUtc <= message.ObservedAtUtc,
            "Last complete scan cannot follow the observation.");
        Require(message.ErrorCode is null || message.ErrorCode.Length <= 128, "Error code exceeds its limit.");
        Require(message.Status != PipelineMonitorStatus.Healthy ||
            (message.LastCompleteScanAtUtc == message.ObservedAtUtc && message.ErrorCode is null),
            "Healthy requires a complete current scan without an error.");
        return AppendAsync(message.ObservationId, message.Source, message.ObservedAtUtc, "Health", null,
            null, message, [], cancellationToken);
    }

    /// <summary>Reads bounded historical evidence, ordered by observation time, never delivery time.</summary>
    public async Task<List<PipelineObservation>> ReadHistoryAsync(PipelineMonitoringSource source, string kind,
        long? runId = null, string? workspaceAcronym = null, int limit = 100, CancellationToken cancellationToken = default)
    {
        ValidateSource(source);
        Require(_sources.Contains(source), "Monitoring source is not approved.");
        Require(kind is "Run" or "Health", "Invalid observation kind.");
        Require(limit is > 0 and <= 500, "History limit must be between 1 and 500.");
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        var query = context.PipelineObservations.AsNoTracking().Where(e => e.Organization == source.Organization &&
            e.ProjectId == source.ProjectId && e.Environment == source.Environment && e.DefinitionId == source.DefinitionId && e.Kind == kind);
        if (runId is not null)
            query = query.Where(e => e.RunId == runId);
        if (workspaceAcronym is not null)
            query = query.Where(e => e.Workspaces.Any(w => w.Acronym == workspaceAcronym.ToUpperInvariant()));
        return await query.OrderByDescending(e => e.ObservedAtUtc).ThenByDescending(e => e.Id)
            .Take(limit).ToListAsync(cancellationToken);
    }

    /// <summary>Builds a source-bound link without accepting an arbitrary message URL.</summary>
    public static Uri GetRunUrl(PipelineMonitoringSource source, long runId)
    {
        ValidateSource(source);
        Require(runId > 0, "Run ID must be positive.");
        return new Uri($"https://dev.azure.com/{Uri.EscapeDataString(source.Organization)}/{source.ProjectId:D}/_build/results?buildId={runId}");
    }

    private async Task<bool> AppendAsync<T>(Guid id, PipelineMonitoringSource source, DateTime observedAtUtc,
        string kind, long? runId, DateTime? sourceChangedAtUtc, T payload, IEnumerable<string> workspaces, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(payload, SerializerOptions);
        Require(Encoding.UTF8.GetByteCount(json) <= 192 * 1024, "Observation exceeds the message size budget.");
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        var existingHash = await context.PipelineObservations.Where(e => e.Id == id).Select(e => e.PayloadHash)
            .SingleOrDefaultAsync(cancellationToken);
        if (existingHash is not null)
            return CheckDuplicate(existingHash, hash);

        context.PipelineObservations.Add(new PipelineObservation
        {
            Id = id, Organization = source.Organization, ProjectId = source.ProjectId, Environment = source.Environment,
            DefinitionId = source.DefinitionId, ObservedAtUtc = observedAtUtc, ReceivedAtUtc = _time.GetUtcNow().UtcDateTime,
            Kind = kind, RunId = runId, SourceChangedAtUtc = sourceChangedAtUtc, PayloadJson = json, PayloadHash = hash,
            Workspaces = workspaces.Select(w => new PipelineObservationWorkspace { ObservationId = id, Acronym = w.ToUpperInvariant() }).ToList()
        });
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            // A concurrent delivery may win the unique key race. Only that exact payload is a duplicate.
            await using var verification = await _factory.CreateDbContextAsync(cancellationToken);
            existingHash = await verification.PipelineObservations.Where(e => e.Id == id).Select(e => e.PayloadHash)
                .SingleOrDefaultAsync(cancellationToken);
            if (existingHash is null)
                throw;
            return CheckDuplicate(existingHash, hash);
        }
    }

    private void ValidateEnvelope(int version, Guid id, PipelineMonitoringSource source, DateTime observedAtUtc)
    {
        ValidateSource(source);
        Require(_sources.Contains(source), "Monitoring source is not approved.");
        Require(version == 1 && id != Guid.Empty, "Unsupported schema version or missing observation ID.");
        Require(observedAtUtc != default, "Observation time is required.");
        Utc(observedAtUtc);
    }

    private static void ValidateSource(PipelineMonitoringSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        Text(source.Organization, 64, "Organization");
        Text(source.Environment, 16, "Environment");
        Require(source.Organization.All(c => char.IsAsciiLetterOrDigit(c) || c == '-') &&
            source.Environment.All(c => char.IsAsciiLetterOrDigit(c) || c == '-'), "Invalid source name.");
        Require(source.ProjectId != Guid.Empty && source.DefinitionId > 0, "Project and definition identity are required.");
    }

    private static bool CheckDuplicate(string existingHash, string hash)
    {
        if (existingHash != hash)
            throw new InvalidOperationException("Observation ID was reused with a different payload.");
        return false;
    }

    private static void Text(string? value, int maxLength, string field) =>
        Require(!string.IsNullOrWhiteSpace(value) && value.Length <= maxLength && value == value.Trim(), $"Invalid {field}.");

    private static void Utc(DateTime? value) =>
        Require(value is null || value.Value.Kind == DateTimeKind.Utc, "Timestamps must be UTC.");

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new ArgumentException(message);
    }
}
