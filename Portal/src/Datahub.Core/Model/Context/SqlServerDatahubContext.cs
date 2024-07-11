using Datahub.Core.Model.Datahub;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Core.Model.Context
{
    public class SqlServerDatahubContext : DatahubProjectDBContext
    {
#if MIGRATION
        public SqlServerDatahubContext()
        {
        }
#endif

        public SqlServerDatahubContext(DbContextOptions<SqlServerDatahubContext> options) : base(options)
        {
        }
#if MIGRATION
    protected override void OnConfiguring(DbContextOptionsBuilder options) {
        options.UseSqlServer("Server=(LocalDB);Integrated Security=True;MultipleActiveResultSets=True");
    }

#endif

    }
}
