using System;
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
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__datahub_mssql_project");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            options.UseSqlServer(connectionString);
            return;
        }

        options.UseSqlServer("Server=(LocalDB);Integrated Security=True;MultipleActiveResultSets=True");
    }

#endif

    }
}
