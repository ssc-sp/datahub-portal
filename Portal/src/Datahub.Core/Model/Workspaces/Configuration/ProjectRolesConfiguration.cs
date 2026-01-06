using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Projects.Configuration;

public class ProjectRolesConfiguration : IEntityTypeConfiguration<Project_Role>
{
    public void Configure(EntityTypeBuilder<Project_Role> builder)
    {
        // Seed the database with predefined project roles
        builder.HasData(Project_Role.GetAll());

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
