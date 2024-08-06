using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Core.Model.Context
{
    public class SqliteDatahubContext : DatahubProjectDBContext
    {
#if MIGRATION
        public SqliteDatahubContext()
        {
        }
#endif

        public SqliteDatahubContext(DbContextOptions<DatahubProjectDBContext> options) : base(options)
        {
        }
#if MIGRATION
    protected override void OnConfiguring(DbContextOptionsBuilder options) {
        options.UseSqlite("DataSource=");
    }
#endif
    }
}
