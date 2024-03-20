using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Onboarding.Configuration;

public class ProjectCreationDetailsConfiguration : IEntityTypeConfiguration<ProjectCreationDetails>
{
    public void Configure(EntityTypeBuilder<ProjectCreationDetails> builder)
    {
        builder.ToTable("ProjectCreationDetails");

        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.Project)
            .WithMany()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.CreatedBy)
            .WithMany()
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Property(e => e.InterestedFeatures)
            .IsRequired(false)
            .HasMaxLength(128);
    }
}