using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Projects.Configuration;

public class DatahubProjectConfiguration : IEntityTypeConfiguration<Datahub_Project>
{
    public void Configure(EntityTypeBuilder<Datahub_Project> builder)
    {
        builder.ToTable("Projects");
        builder.Property(e => e.SubscriptionId)
            .IsRequired()
            .HasMaxLength(36);
    }
}