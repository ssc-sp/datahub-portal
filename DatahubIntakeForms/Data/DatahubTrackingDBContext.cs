using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Portal.Data
{
    public class DatahubTrackingDBContext : DbContext
    {
        public DatahubTrackingDBContext(DbContextOptions<DatahubTrackingDBContext> options) : base(options)
        { }

        public DbSet<DatahubProject> Projects { get; set; }
    }
}
