using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;


namespace NRCan.Datahub.Portal.Data
{
    public class DatahubETLStatusContext : DbContext
    {
        
        public DatahubETLStatusContext(DbContextOptions<DatahubETLStatusContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ETL_CONTROL_TBL> ETL_CONTROL_TBL { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<ETL_CONTROL_TBL>(entity =>
            {
                entity.HasNoKey();

                //entity.Property(e => e.END_TS).HasColumnType("datetime");

                entity.Property(e => e.PROCESS_NM).HasMaxLength(100);

                //entity.Property(e => e.START_TS).HasColumnType("datetime");

                entity.Property(e => e.STATUS_FLAG)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength(true);
            });

            //modelBuilder.HasSequence<int>("run_id_seq");

            
        }

        //partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}