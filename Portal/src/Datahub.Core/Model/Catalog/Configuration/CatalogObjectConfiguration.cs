using Microsoft.EntityFrameworkCore;

namespace Datahub.Core.Model.Catalog.Configuration;

internal class CatalogObjectConfiguration : IEntityTypeConfiguration<CatalogObject>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<CatalogObject> builder)
    {
        builder.ToTable("CatalogObjects");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.HasKey(e => new { e.ObjectType, e.ObjectId });

        builder.Property(e => e.ObjectId)
               .HasMaxLength(64)
               .IsRequired();

        builder.Property(e => e.NameEnglish)
               .HasMaxLength(160);

        builder.Property(e => e.NameFrench)
               .HasMaxLength(160);

        builder.Property(e => e.DescEnglish);

        builder.Property(e => e.DescFrench);
    }
}
