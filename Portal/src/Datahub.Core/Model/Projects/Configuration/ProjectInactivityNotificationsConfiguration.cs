using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Projects.Configuration
{
    public class
        ProjectInactivityNotificationsConfiguration : IEntityTypeConfiguration<ProjectInactivityNotifications>
    {
        public void Configure(EntityTypeBuilder<ProjectInactivityNotifications> builder)
        {
            builder.ToTable("ProjectInactivityNotifications");

            builder.HasKey(e => e.Project_ID);

            builder.Property(e => e.NotificationDate)
                .IsRequired();

            builder.Property(e => e.DaysBeforeDeletion)
                .IsRequired();

            builder.Property(e => e.SentTo)
                .IsRequired();
        }
    }
}