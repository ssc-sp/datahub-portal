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
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            // Configure alternate key for OID
            builder.HasIndex(e => e.ExternalSubject)
                .IsUnique();

            builder.Property(e => e.ExternalSubject)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Organization)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(e => e.Affiliation)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(e => e.DeactivationReason)
                .HasMaxLength(500);

            builder.HasMany(e => e.Invitations)
                .WithOne(r => r.User);

            builder.HasOne(u => u.PortalUser)
                .WithOne(r => r.ExternalUser)
                .HasForeignKey<PortalUser>(u => u.ExternalUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure relationship with deactivated by user
            builder.HasOne(e => e.DeactivatedByUser)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}