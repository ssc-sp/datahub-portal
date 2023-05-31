using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Repositories.Configuration;

public class ProjectRepositoryConfiguration : IEntityTypeConfiguration<ProjectRepository>
{
    public void Configure(EntityTypeBuilder<ProjectRepository> builder)
    {
        builder.ToTable("Project_Repositories");
        
        builder.HasKey(e => e.Id);
        
        // Make the ProjectId column a foreign key to the Project table
        builder.HasOne(e => e.Project)
               .WithMany(e => e.Repositories)
               .HasForeignKey(e => e.ProjectId)
               .OnDelete(DeleteBehavior.NoAction);
        
        builder.Property(e => e.RepositoryUrl)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.IsPublic)
            .IsRequired();

    }
}