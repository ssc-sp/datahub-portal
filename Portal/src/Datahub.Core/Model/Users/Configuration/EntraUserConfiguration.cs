using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Users.Configuration
{
    internal class EntraUserConfiguration : IEntityTypeConfiguration<EntraUser>
    {
        public void Configure(EntityTypeBuilder<EntraUser> builder)
        {
            builder.Property(e => e.GraphGuid)
                .IsRequired()
                .HasMaxLength(64);

            builder.HasIndex(e => e.GraphGuid)
                .IsUnique();

            builder.Property(r => r.Timestamp).IsRowVersion();
        }
    }
}
