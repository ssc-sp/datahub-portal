using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Subscriptions.Configuration;

public class DatahubAzureSubscriptionConfiguration : IEntityTypeConfiguration<DatahubAzureSubscription>
{
    public void Configure(EntityTypeBuilder<DatahubAzureSubscription> builder)
    {
        builder.ToTable("AzureSubscriptions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TenantId)
            .IsRequired()
            .HasMaxLength(36);
        builder.Property(e => e.SubscriptionId)
            .IsRequired()
            .HasMaxLength(36);
        builder.Property(e => e.SubscriptionName)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(e => e.Nickname)
            .HasMaxLength(100);

        builder.HasMany(e => e.Workspaces)
            .WithOne(w => w.DatahubAzureSubscription);
    }
}
