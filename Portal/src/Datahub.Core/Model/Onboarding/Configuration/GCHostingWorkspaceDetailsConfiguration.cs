using Datahub.Core.Configuration;
using Datahub.Core.Model.Onboarding;
using Datahub.Metadata.Model;
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
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.LeadLastName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.DepartmentName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.LeadEmail)
            .HasMaxLength(ConfigurationConstants.EMAIL_MAX_LENGTH)
            .IsRequired();

        builder.Property(e => e.FinancialAuthorityFirstName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.FinancialAuthorityLastName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.FinancialAuthorityCommitmentIsRef)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.FinancialAuthorityCommitmentIsOrg)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.FinancialAuthorityEmail)
            .HasMaxLength(ConfigurationConstants.EMAIL_MAX_LENGTH)
            .IsRequired();

        builder.Property(e => e.WorkspaceBudget)
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(e => e.WorkspaceName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.WorkspaceDescription)
            .IsRequired();

        builder.Property(e => e.Keywords)
            .IsRequired();

        builder.Property(e => e.RetentionPeriodYears)
            .IsRequired();

        builder.Property(e => e.SecurityClassification)
            .HasConversion(new SecurityClassificationStringConverter())
            .IsRequired();

        builder.Property(e => e.GeneratesInfoBusinessValue);

        builder.Property(e => e.ProjectTitle)
            .HasMaxLength(200);

        builder.Property(e => e.ProjectDescription);

        builder.Property(e => e.CBRName);

        builder.Property(e => e.CBRID)
            .IsRequired();

        builder.HasMany(e => e.WorkspacesInBudget)
            .WithOne(w => w.ParentGCHostingBudget)
            .HasForeignKey(w => w.ParentGCHostingBudgetId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}