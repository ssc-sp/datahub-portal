using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.UserTracking.Configuration
{
    public class UserInactivityNotificationsConfiguration : IEntityTypeConfiguration<UserInactivityNotifications>
    {
        public void Configure(EntityTypeBuilder<UserInactivityNotifications> builder)
        {
            builder.ToTable("UserInactivityNotifications");

            builder.HasKey(e => e.UserID);

            builder.Property(e => e.NotificationDate)
                .IsRequired();

            builder.Property(e => e.DaysBeforeLocked)
                .IsRequired();

            builder.Property(e => e.DaysBeforeDeleted)
                .IsRequired();
        }
    }
}