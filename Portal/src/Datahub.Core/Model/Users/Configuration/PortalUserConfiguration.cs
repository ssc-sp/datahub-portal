using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Users.Configuration
{
    public class PortalUserConfiguration : IEntityTypeConfiguration<PortalUser>
    {
        public void Configure(EntityTypeBuilder<PortalUser> builder)
        {
            builder.HasOne(u => u.ExternalUser)
                .WithOne(r => r.PortalUser);

            builder.HasOne(u => u.EntraUser)
                .WithOne(r => r.PortalUser);
        }
    }
}
