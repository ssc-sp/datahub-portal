using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Projects.Configuration
{
    public class
        ProjectInactivityNotificationsConfiguration : IEntityTypeConfiguration<Project_Inactivity_Notifications>
    {
        public void Configure(EntityTypeBuilder<Project_Inactivity_Notifications> builder)
        {
            builder.ToTable("Project_Inactivity_Notifications");

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