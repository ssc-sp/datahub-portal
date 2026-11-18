namespace Datahub.Shared.Entities.PipelineMonitoring;

/// <summary>Versioned, immutable execution evidence published by the infrastructure monitor.</summary>
public sealed record PipelineRunObservationMessage
{
    public int SchemaVersion { get; init; } = 1;
    public Guid ObservationId { get; init; }
    public required PipelineMonitoringSource Source { get; init; }
    public DateTime ObservedAtUtc { get; init; }
    public long RunId { get; init; }
    public required string PipelineName { get; init; }
    public required string RunName { get; init; }
    public string? SourceBranch { get; init; }
    public string? SourceVersion { get; init; }
    public PipelineExecutionState State { get; init; }
    public PipelineExecutionResult Result { get; init; }
    public DateTime? SourceChangedAtUtc { get; init; }
    public DateTime? QueuedAtUtc { get; init; }
    public DateTime? StartedAtUtc { get; init; }
    public DateTime? FinishedAtUtc { get; init; }
    public bool TimelineComplete { get; init; }
    public PipelineWorkspaceReference[] Workspaces { get; init; } = [];
    public PipelineTimelineRecord[] Records { get; init; } = [];
}

/// <summary>Identity of an explicitly configured monitoring source, independent of credentials.</summary>
public sealed record PipelineMonitoringSource
{
    public required string Organization { get; init; }
    public Guid ProjectId { get; init; }
    public required string Environment { get; init; }
    public int DefinitionId { get; init; }
}

/// <summary>A workspace association supported by pipeline metadata, not request timestamps.</summary>
public sealed record PipelineWorkspaceReference
{
    public required string Acronym { get; init; }
    public PipelineWorkspaceEvidence Evidence { get; init; }
}

/// <summary>A stage or job attempt; record identity alone is insufficient across retries.</summary>
public sealed record PipelineTimelineRecord
{
    public Guid TimelineId { get; init; }
    public Guid RecordId { get; init; }
    public Guid? ParentId { get; init; }
    public int Attempt { get; init; }
    public int ChangeId { get; init; }
    public required string RecordType { get; init; }
    public required string Identifier { get; init; }
    public required string Name { get; init; }
    public string? WorkspaceAcronym { get; init; }
    public PipelineWorkspaceEvidence WorkspaceEvidence { get; init; }
    public PipelineExecutionState State { get; init; }
    public PipelineExecutionResult Result { get; init; }
    public DateTime? StartedAtUtc { get; init; }
    public DateTime? FinishedAtUtc { get; init; }
    public string? FailureSummary { get; init; }
}

/// <summary>A poll observation, including idle periods and incomplete scans.</summary>
public sealed record PipelineMonitorHealthMessage
{
    public int SchemaVersion { get; init; } = 1;
    public Guid ObservationId { get; init; }
    public required PipelineMonitoringSource Source { get; init; }
    public DateTime ObservedAtUtc { get; init; }
    public PipelineMonitorStatus Status { get; init; }
    public DateTime? LastCompleteScanAtUtc { get; init; }
    public string? ErrorCode { get; init; }
}

/// <summary>Unknown is preserved rather than inferred as success or failure.</summary>
public enum PipelineExecutionState { Unknown, Queued, Running, Completed, Cancelling, Postponed }

/// <summary>Execution result, separate from workspace resource readiness.</summary>
public enum PipelineExecutionResult { Unknown, None, Succeeded, PartiallySucceeded, Failed, Cancelled, Skipped, Abandoned }

/// <summary>Evidence for workspace mapping; Unknown must not name a workspace.</summary>
public enum PipelineWorkspaceEvidence { Unknown, RunParameter, JobIdentifier, DeploymentEnvironment }

/// <summary>Health of an individual configured pipeline source.</summary>
public enum PipelineMonitorStatus { Unknown, Healthy, Partial, Failed, Disabled }
