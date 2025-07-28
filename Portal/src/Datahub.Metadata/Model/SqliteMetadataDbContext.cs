using Datahub.Metadata.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Model.Context
{
    public class SqliteMetadataDbContext : MetadataDbContext
    {
#if MIGRATION
        public SqliteMetadataDbContext()
        {
        }
#endif

        public SqliteMetadataDbContext(DbContextOptions<MetadataDbContext> options) : base(options)
        {
        }

#if MIGRATION
        protected override void OnConfiguring(DbContextOptionsBuilder options) {
            options.UseSqlite("DataSource=");
        }
#endif
    }
}
