namespace Datahub.Core.Model.PipelineMonitoring;

/// <summary>Append-only monitoring evidence. No resource status or workspace lifetime dependency.</summary>
public class PipelineObservation
{
    public Guid Id { get; set; }
    public required string Organization { get; set; }
    public Guid ProjectId { get; set; }
    public required string Environment { get; set; }
    public int DefinitionId { get; set; }
    public DateTime ObservedAtUtc { get; set; }
    public DateTime ReceivedAtUtc { get; set; }
    public required string Kind { get; set; }
    public long? RunId { get; set; }
    public DateTime? SourceChangedAtUtc { get; set; }
    public required string PayloadJson { get; set; }
    public required string PayloadHash { get; set; }
    public List<PipelineObservationWorkspace> Workspaces { get; set; } = [];
}

/// <summary>Searchable workspace associations that survive deletion of the Portal workspace.</summary>
public class PipelineObservationWorkspace
{
    public Guid ObservationId { get; set; }
    public required string Acronym { get; set; }
}
