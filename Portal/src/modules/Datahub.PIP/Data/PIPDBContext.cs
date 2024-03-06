using Microsoft.EntityFrameworkCore;

namespace Datahub.PIP.Data;

public class PIPDBContext : DbContext
{
    public PIPDBContext(DbContextOptions<PIPDBContext> options) : base(options)
    { }

    public DbSet<PIPTombstone> Tombstones { get; set; }
    public DbSet<PIPIndicatorAndResults> IndicatorAndResults { get; set; }

    public DbSet<PIPRisks> Risks { get; set; }

    public DbSet<PIPTombstoneRisks> TombstoneRisks { get; set; }
    public DbSet<PIPIndicatorRisks> IndicatorRisks { get; set; }

    public DbSet<PIPFiscalYears> FiscalYears { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.Entity<Datahub_Project>().HasOne(p => p.PBI_License_Request).WithOne(p => p.Project).HasForeignKey<PBI_License_Request>(l => l.Project_ID);

        //modelBuilder.Entity<Datahub_ProjectComment>().HasOne(c => c.Project).WithMany(p => p.Comments);

        //modelBuilder.Entity<PBI_User_License_Request>()
        //    .HasOne(p => p.LicenseRequest)
        //    .WithMany(b => b.User_License_Requests)
        //    .HasForeignKey(p => p.RequestID);

        modelBuilder.Entity<PIPFiscalYears>().HasData(
            new PIPFiscalYears { YearId = 2018, FiscalYear = "2017-18" },
            new PIPFiscalYears { YearId = 2019, FiscalYear = "2018-19" },
            new PIPFiscalYears { YearId = 2020, FiscalYear = "2019-20" },
            new PIPFiscalYears { YearId = 2021, FiscalYear = "2020-21" },
            new PIPFiscalYears { YearId = 2022, FiscalYear = "2021-22" }
        );
    }
}