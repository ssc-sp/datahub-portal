
using Datahub.Metadata.Model;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Core.Model.Context
{
    public class SqlServerMetadataDbContext : MetadataDbContext
    {

#if MIGRATION
        public SqlServerMetadataDbContext()
        {
        }
#endif

        public SqlServerMetadataDbContext(DbContextOptions<SqlServerMetadataDbContext> options) : base(options)
        {
        }

#if MIGRATION
        protected override void OnConfiguring(DbContextOptionsBuilder options) {
            options.UseSqlServer("Server=(LocalDB);Integrated Security=True;MultipleActiveResultSets=True");
        }
#endif

    }
}
