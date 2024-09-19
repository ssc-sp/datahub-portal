using Datahub.Core.Model.Onboarding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Achievements.Configuration;

public class GCHostingWorkspaceDetailsConfiguration : IEntityTypeConfiguration<GCHostingWorkspaceDetails>
{
    public void Configure(EntityTypeBuilder<GCHostingWorkspaceDetails> builder)
    {
        builder.ToTable("GCHostingWorkspaceDetails");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.LeadFirstName)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(e => e.LeadLastName)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(e => e.DepartmentName)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(e => e.LeadEmail)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(e => e.FinancialAuthorityFirstName)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(e => e.FinancialAuthorityLastName)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(e => e.FinancialAuthorityCostCentre)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(e => e.WorkspaceTitle)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(e => e.WorkspaceDescription)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(e => e.WorkspaceIdentifier)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(e => e.Keywords)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(e => e.AreaOfScience)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(e => e.RetentionPeriodYears)
            .IsRequired();

        builder.Property(e => e.SecurityClassification)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(e => e.GeneratesInfoBusinessValue)
            .IsRequired();

        builder.Property(e => e.ProjectTitle)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(e => e.ProjectDescription)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(e => e.ProjectStartDate)
            .IsRequired();

        builder.Property(e => e.ProjectEndDate)
            .IsRequired();

        builder.Property(e => e.CBRName)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(e => e.CBRID)
            .IsRequired()
            .HasMaxLength(64);
    }
}