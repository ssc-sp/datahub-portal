using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.CloudStorage.Configuration;

internal class ProjectCloudStorageConfiguration : IEntityTypeConfiguration<ProjectCloudStorage>
{
    public void Configure(EntityTypeBuilder<ProjectCloudStorage> builder)
    {
        builder.ToTable("Project_Cloud_Storages");

        builder.HasKey(x => x.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Provider)
               .HasMaxLength(16)
               .HasDefaultValue("Azure")
               .IsRequired();

        builder.Property(e => e.Name)
               .HasMaxLength(100);

        builder.HasOne(e => e.Project)
               .WithMany(e => e.CloudStorages)
               .HasForeignKey(e => e.ProjectId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
