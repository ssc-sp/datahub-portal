using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Core.Model.UserTracking.Configuration
{
    public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<UserSettings> builder)
        {
            builder.ToTable("UserSettings");
            builder.HasKey(e => e.UserId);
            builder.Property(e => e.UserId)
                .HasMaxLength(64);
            builder.Property(e => e.UserName)
                .HasMaxLength(128);
            builder.Property(e => e.AcceptedDate);
            builder.Property(e => e.Language)
                .HasMaxLength(5);
            builder.Property(e => e.NotificationsEnabled);
            builder.Property(e => e.HideAchievements);
            builder.Property(e => e.HideAlerts);
            builder.Property(e => e.HiddenAlerts)
                .HasConversion(
                    v => string.Join(',', v),
                    v => string.IsNullOrEmpty(v) ? new List<string>() : v.Split(',', System.StringSplitOptions.RemoveEmptyEntries).ToList());
            
        }
    }
}