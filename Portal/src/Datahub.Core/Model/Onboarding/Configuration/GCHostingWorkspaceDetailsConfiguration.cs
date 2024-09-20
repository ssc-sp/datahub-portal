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
            .IsRequired();

        builder.Property(e => e.LeadLastName)
            .IsRequired();

        builder.Property(e => e.DepartmentName)
            .IsRequired();

        builder.Property(e => e.LeadEmail)
            .IsRequired();

        builder.Property(e => e.FinancialAuthorityFirstName)
            .IsRequired();

        builder.Property(e => e.FinancialAuthorityLastName)
            .IsRequired();

        builder.Property(e => e.FinancialAuthorityCostCentre)
            .IsRequired();

        builder.Property(e => e.WorkspaceTitle)
            .IsRequired();

        builder.Property(e => e.WorkspaceDescription)
            .IsRequired();

        builder.Property(e => e.WorkspaceIdentifier)
            .IsRequired();

        builder.Property(e => e.Keywords)
            .IsRequired();

        builder.Property(e => e.AreaOfScience)
            .IsRequired();

        builder.Property(e => e.RetentionPeriodYears)
            .IsRequired();

        builder.Property(e => e.SecurityClassification)
            .IsRequired();

        builder.Property(e => e.GeneratesInfoBusinessValue);

        builder.Property(e => e.ProjectTitle)
            .IsRequired();

        builder.Property(e => e.ProjectDescription)
            .IsRequired();

        builder.Property(e => e.ProjectStartDate)
            .IsRequired();

        builder.Property(e => e.ProjectEndDate)
            .IsRequired();

        builder.Property(e => e.CBRName);

        builder.Property(e => e.CBRID)
            .IsRequired();

        builder.HasOne(e => e.Datahub_Project)
            .WithOne(l => l.GCHostingWorkspaceDetails)
            .HasForeignKey<GCHostingWorkspaceDetails>(l => l.Id)
            .OnDelete(DeleteBehavior.NoAction);
    }
}