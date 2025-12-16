using Datahub.Core.Configuration;
using Datahub.Core.Model.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Users.Configuration;

public class PortalUserConfiguration : IEntityTypeConfiguration<PortalUser>
{
    public void Configure(EntityTypeBuilder<PortalUser> builder)
    {
        builder.ToTable("PortalUsers");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(ConfigurationConstants.EMAIL_MAX_LENGTH);

        builder.Property(e => e.DisplayName)
            .HasMaxLength(128);

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

        builder.HasOne(e => e.UserSettings)
            .WithOne(l => l.User)
            .HasForeignKey<UserSettings>(e => e.PortalUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(e => e.InactivityNotifications)
            .WithOne(e => e.User)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(u => u.ExternalUserHistory)
            .WithOne(r => r.PortalUser)
            .HasForeignKey(e => e.PortalUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(u => u.EntraUser)
            .WithOne(r => r.PortalUser)
            .HasForeignKey<EntraUser>(e => e.PortalUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
