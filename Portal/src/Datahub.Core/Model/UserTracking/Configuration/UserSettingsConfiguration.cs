using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Datahub.Core.Model.UserTracking.Configuration
{
    public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<UserSettings> builder)
        {
            var valueComparer = new ValueComparer<List<string>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList());

            builder.ToTable("UserSettings");
            builder.HasKey(e => e.PortalUserId);
            builder.Property(e => e.UserName)
                .HasMaxLength(128);
            builder.Property(e => e.AcceptedDate);
            builder.Property(e => e.Language)
                .HasMaxLength(5);
            builder.Property(e => e.NotificationsEnabled)
                .HasDefaultValue(true);
            builder.Property(e => e.HideAchievements)
                .HasDefaultValue(false);
            builder.Property(e => e.HideAlerts)
                .HasDefaultValue(false);
            builder.Property(e => e.HiddenAlerts)
                .HasConversion(
                    v => string.Join(',', v),
                    v => string.IsNullOrEmpty(v) ? new List<string>() : v.Split(',', System.StringSplitOptions.RemoveEmptyEntries).ToList());
            builder.Property(e => e.HiddenAlerts)
                .Metadata
                .SetValueComparer(valueComparer);
        }
    }
}