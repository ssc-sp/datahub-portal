using Microsoft.EntityFrameworkCore;

namespace Datahub.Portal.Data.M365Forms
{
    public class M365FormsDBContext : DbContext
    {
        public M365FormsDBContext(DbContextOptions<M365FormsDBContext> options) : base(options)
        { }

        public DbSet<M365FormsApplication> M365FormsApplications { get; set; }
    }
}
