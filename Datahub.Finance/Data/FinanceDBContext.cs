using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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
            modelBuilder.Entity<FundCenter>()
                .HasOne(e => e.Sector)
                .WithMany(e => e.SectorFundCenters)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FundCenter>()
                .HasOne(e => e.Branch)
                .WithMany(e => e.BranchFundCenters)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FundCenter>()
                .HasOne(e => e.Division)
                .WithMany(e => e.DivisionFundCenters)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FundCenter>()
                .HasOne(e => e.FiscalYear)
                .WithMany(e => e.FundCenters)
                .OnDelete(DeleteBehavior.NoAction);

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
