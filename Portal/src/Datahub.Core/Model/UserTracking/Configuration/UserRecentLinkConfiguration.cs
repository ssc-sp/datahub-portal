using Microsoft.EntityFrameworkCore;

namespace Datahub.Core.Model.UserTracking.Configuration
{
    public class UserRecentLinkConfiguration : IEntityTypeConfiguration<UserRecentLink>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<UserRecentLink> builder)
        {
            builder.ToTable("UserRecentLink");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.HasOne(e => e.User)
                .WithMany(e => e.RecentLinks)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}