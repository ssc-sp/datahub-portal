using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Subscriptions.Configuration;

public class SubnetConfiguration : IEntityTypeConfiguration<Subnet>
{
    public void Configure(EntityTypeBuilder<Subnet> builder)
    {
        builder.ToTable("Subnets");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.SubnetName)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(e => e.AddressPrefix)
            .HasMaxLength(50);

        builder.HasOne(e => e.VNet)
            .WithMany(v => v.Subnets)
            .HasForeignKey(e => e.VNetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
