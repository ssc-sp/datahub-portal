using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Achievements.Configuration;

public class PortalUserConfiguration : IEntityTypeConfiguration<PortalUser>
{
    public void Configure(EntityTypeBuilder<PortalUser> builder)
    {
        builder.ToTable("PortalUsers");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
               .ValueGeneratedOnAdd();

        builder.Property(e => e.GraphGuid)
               .IsRequired()
               .HasMaxLength(64);

        builder.HasIndex(e => e.GraphGuid)
               .IsUnique();

        builder.Property(e => e.Email)
               .HasMaxLength(64);

        builder.Property(e => e.Language)
               .HasMaxLength(5);

        builder.HasMany(e => e.Achievements)
               .WithOne(e => e.PortalUser)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(e => e.TelemetryEvents)
               .WithOne(e => e.PortalUser)
               .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasMany(e => e.RecentLinks)
            .WithOne(l => l.User)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(e => e.InactivityNotifications)
            .WithOne(e => e.User)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
