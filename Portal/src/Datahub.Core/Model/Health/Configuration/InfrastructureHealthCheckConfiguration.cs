using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Health.Configuration;

public class InfrastructureHealthCheckConfiguration : IEntityTypeConfiguration<InfrastructureHealthCheck>
{
    public void Configure(EntityTypeBuilder<InfrastructureHealthCheck> builder)
    {
        builder.ToTable("InfrastructureHealthChecks");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
               .ValueGeneratedOnAdd();

        builder.Property(e => e.Group)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(e => e.Url)
            .HasMaxLength(256);

        builder.Property(e => e.ResourceType)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.HealthCheckTimeUtc)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("GETUTCDATE()");
    }
}