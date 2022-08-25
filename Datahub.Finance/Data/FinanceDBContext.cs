using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Portal.Data.Finance
{
    public class FinanceDBContext : DbContext
    {
        public FinanceDBContext(DbContextOptions<FinanceDBContext> options) : base(options)
        { }

        public DbSet<HierarchyLevel> HierarchyLevels { get; set; }
        public DbSet<FundCenter> FundCenters  { get; set; }
        public DbSet<FiscalYear> FiscalYears { get; set; }
        public DbSet<Forecast> Forecasts { get; set; }
        public DbSet<SummaryForecast> SummaryForecasts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //BuildSectors(modelBuilder);
            //BuildBranches(modelBuilder);

            //modelBuilder.Entity<SectorAndBranch>()
            //.HasOne(a => a.Sector)
            //.WithOne(b => b.SectorAndBranch)
            //.HasForeignKey<Sector>(b => b.SectorAndBranchForSectorId);

            //modelBuilder.Entity<SectorAndBranch>()
            //.HasOne(a => a.Branch)
            //.WithOne(b => b.SectorAndBranch)
            //.HasForeignKey<Branch>(b => b.SectorAndBranchForBranchId);
        }

      
    }
}
