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
            builder.Property(e => e.Language)
                   .HasMaxLength(5);
        }
    }
}