using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Projects.Configuration;

public class ProjectStorageConfiguration : IEntityTypeConfiguration<ProjectStorage>
{
    public void Configure(EntityTypeBuilder<ProjectStorage> builder)
    {
        builder.ToTable("Project_Storage_Avgs");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.CloudProvider).HasMaxLength(16);

        builder.HasIndex(e => new { e.ProjectId, e.Date });
    }
}