using Microsoft.EntityFrameworkCore;

namespace Datahub.Portal.Data.Pipelines;

public class DatahubETLStatusContext : DbContext
{

    public DatahubETLStatusContext(DbContextOptions<DatahubETLStatusContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ETLCONTROLTBL> ETLCONTROLTBL { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<ETLCONTROLTBL>(entity =>
        {
            entity.HasNoKey();

            //entity.Property(e => e.END_TS).HasColumnType("datetime");

            entity.Property(e => e.PROCESSNM).HasMaxLength(100);

            //entity.Property(e => e.START_TS).HasColumnType("datetime");

            entity.Property(e => e.STATUSFLAG)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength(true);
        });

        //modelBuilder.HasSequence<int>("run_id_seq");


    }

    //partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}