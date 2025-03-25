using Datahub.Core.Model.Subscriptions;
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
            .HasDefaultValue(new DateTime(2025, 3, 25));
        // builder.HasOne(e => e.DatahubAzureSubscription)
        //     .WithMany(s => s.Workspaces)
        //     .HasForeignKey(e => e.DatahubAzureSubscriptionId)
        //     .IsRequired(false);
    }
}