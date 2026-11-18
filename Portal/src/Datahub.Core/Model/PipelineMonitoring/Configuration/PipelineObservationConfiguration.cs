using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.PipelineMonitoring.Configuration;

public class PipelineObservationConfiguration : IEntityTypeConfiguration<PipelineObservation>
{
    public void Configure(EntityTypeBuilder<PipelineObservation> builder)
    {
        builder.ToTable("PipelineObservations");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Organization).HasMaxLength(64).IsRequired();
        builder.Property(e => e.Environment).HasMaxLength(16).IsRequired();
        builder.Property(e => e.Kind).HasMaxLength(16).IsRequired();
        builder.Property(e => e.PayloadHash).HasMaxLength(64).IsRequired();
        builder.Property(e => e.PayloadJson).IsRequired();
        builder.HasIndex(e => new { e.Organization, e.ProjectId, e.Environment, e.DefinitionId, e.RunId, e.ObservedAtUtc });
        builder.HasIndex(e => new { e.Organization, e.ProjectId, e.Environment, e.DefinitionId, e.Kind, e.ObservedAtUtc });
        builder.HasMany(e => e.Workspaces).WithOne().HasForeignKey(e => e.ObservationId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PipelineObservationWorkspaceConfiguration : IEntityTypeConfiguration<PipelineObservationWorkspace>
{
    public void Configure(EntityTypeBuilder<PipelineObservationWorkspace> builder)
    {
        builder.ToTable("PipelineObservationWorkspaces");
        builder.HasKey(e => new { e.ObservationId, e.Acronym });
        builder.Property(e => e.Acronym).HasMaxLength(64).IsRequired();
        builder.HasIndex(e => new { e.Acronym, e.ObservationId });
    }
}
