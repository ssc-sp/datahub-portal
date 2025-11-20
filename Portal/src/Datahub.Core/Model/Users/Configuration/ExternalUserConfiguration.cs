using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Users.Configuration
{
    internal class ExternalUserConfiguration : IEntityTypeConfiguration<ExternalUser>
    {
        public void Configure(EntityTypeBuilder<ExternalUser> builder)
        {
            builder.ToTable("ExternalUsers");

            builder.HasMany(e => e.Requests)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserOID);
        }
    }
}