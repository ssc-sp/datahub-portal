using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Datahub.Core.Model.Projects;

namespace Datahub.Core.Model.Users.Configuration
{
    internal class ExternalUserInviteConfiguration : IEntityTypeConfiguration<ExternalUserInvite>
    {
        public void Configure(EntityTypeBuilder<ExternalUserInvite> builder)
        {
            builder.ToTable("ExternalUserInvites");

            // Key
            builder.HasKey(i => i.RequestID);
            builder.Property(i => i.RequestID)
                .ValueGeneratedOnAdd();

            builder.Property(i => i.Request_DT)
                .HasConversion(
                    v => v,
                    v => v)
                .HasColumnType("datetimeoffset");

            // Relationships
            builder.HasOne(i => i.Project)
                .WithMany() // avoid relying on a project-side navigation
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
