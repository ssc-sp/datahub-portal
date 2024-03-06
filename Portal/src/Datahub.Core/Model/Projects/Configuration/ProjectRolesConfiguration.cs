using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Projects.Configuration;

public class ProjectRolesConfiguration : IEntityTypeConfiguration<ProjectRole>
{
    public void Configure(EntityTypeBuilder<ProjectRole> builder)
    {
        builder.HasData(ProjectRole.GetAll());

        builder.ToTable("Project_Roles");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(128);
    }
}