using Datahub.Core.Model.Subscriptions;
using Datahub.Metadata.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Projects.Configuration;

public class DatahubProjectConfiguration : IEntityTypeConfiguration<Datahub_Project>
{
    public void Configure(EntityTypeBuilder<Datahub_Project> builder)
    {
        builder
            .ToTable("Projects");
        builder
            .Property(p => p.Created_DT)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("GETUTCDATE()");
        builder.Property(p => p.Data_Sensitivity)
            .IsRequired()
            .HasConversion(
                v => v == ClassificationType.ProtectedA ? "Protected A"
                   : v == ClassificationType.ProtectedB ? "Protected B"
                   : "Unclassified",
                v => v == "Protected A" ? ClassificationType.ProtectedA
                   : v == "Protected B" ? ClassificationType.ProtectedB
                   : ClassificationType.Unclassified);
        // builder.HasOne(e => e.DatahubAzureSubscription)
        //     .WithMany(s => s.Workspaces)
        //     .HasForeignKey(e => e.DatahubAzureSubscriptionId)
        //     .IsRequired(false);
    }
}
