using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Users.Configuration
{
    internal class ExternalUserConfiguration : IEntityTypeConfiguration<ExternalUser>
    {
        public void Configure(EntityTypeBuilder<ExternalUser> builder)
        {
            builder.ToTable("ExternalUsers");

            // Configure key
            builder.HasKey(e => e.ExternalUserID);
            builder.Property(e => e.ExternalUserID)
                .ValueGeneratedOnAdd();

            // Configure alternate key for OID
            builder.HasIndex(e => e.OID)
                .IsUnique();

            builder.HasMany(e => e.Invitations)
                .WithOne(r => r.User);

            // Configure relationship with deactivated by user
            builder.HasOne(e => e.DeactivatedByUser)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}