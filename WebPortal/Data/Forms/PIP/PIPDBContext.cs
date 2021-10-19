using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.ProjectForms.Data.PIP
{
    public class PIPDBContext : DbContext
    {
        public PIPDBContext(DbContextOptions<PIPDBContext> options) : base(options)
        { }

        public DbSet<PIP_Tombstone> Tombstones { get; set; }
        public DbSet<PIP_IndicatorAndResults> IndicatorAndResults { get; set; }

        public DbSet<PIP_Risks> Risks { get; set; }

        public DbSet<PIP_TombstoneRisks> TombstoneRisks { get; set; }
        public DbSet<PIP_IndicatorRisks> IndicatorRisks { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Datahub_Project>().HasOne(p => p.PBI_License_Request).WithOne(p => p.Project).HasForeignKey<PBI_License_Request>(l => l.Project_ID);

            //modelBuilder.Entity<Datahub_ProjectComment>().HasOne(c => c.Project).WithMany(p => p.Comments);

            //modelBuilder.Entity<PBI_User_License_Request>()
            //    .HasOne(p => p.LicenseRequest)
            //    .WithMany(b => b.User_License_Requests)
            //    .HasForeignKey(p => p.RequestID);
        }
    }
}
