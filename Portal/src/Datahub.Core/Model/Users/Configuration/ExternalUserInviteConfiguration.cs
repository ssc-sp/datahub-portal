using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

            // Relationships - configured in ExternalUserConfiguration
        }
    }
}
