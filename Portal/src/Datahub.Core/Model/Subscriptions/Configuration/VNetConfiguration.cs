using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Subscriptions.Configuration;

public class VNetConfiguration : IEntityTypeConfiguration<VNet>
{
    public void Configure(EntityTypeBuilder<VNet> builder)
    {
        builder.ToTable("VNets");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.VNetId)
            .IsRequired()
            .HasMaxLength(500);
        builder.Property(e => e.VNetName)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(e => e.Subscription)
            .WithMany(s => s.VNets)
            .HasForeignKey(e => e.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
