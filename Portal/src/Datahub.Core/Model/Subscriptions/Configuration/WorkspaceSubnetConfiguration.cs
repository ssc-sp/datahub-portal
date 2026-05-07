using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Subscriptions.Configuration;

public class WorkspaceSubnetConfiguration : IEntityTypeConfiguration<WorkspaceSubnet>
{
    public void Configure(EntityTypeBuilder<WorkspaceSubnet> builder)
    {
        builder.ToTable("WorkspaceSubnets");
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.ProjectId, e.SubnetId }).IsUnique();

        builder.HasOne(e => e.Project)
            .WithMany(p => p.WorkspaceSubnets)
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Subnet)
            .WithMany(s => s.WorkspaceSubnets)
            .HasForeignKey(e => e.SubnetId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
