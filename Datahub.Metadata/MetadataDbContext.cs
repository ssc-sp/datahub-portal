using Microsoft.EntityFrameworkCore;

namespace NRCan.Datahub.Metadata
{
    public class MetadataDbContext : DbContext
    {
        public MetadataDbContext(DbContextOptions<MetadataDbContext> options) : base(options)
        {
        }

        public DbSet<MetadataVersion> MetadataVersions { get; set; }
        public DbSet<FieldDefinition> FieldDefinitions { get; set; }
        public DbSet<FieldChoice> FieldChoices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MetadataVersion>(entity =>
            {
                entity.ToTable("MetadataVersion");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Source).HasMaxLength(32);

                entity.Property(e => e.VersionData).HasMaxLength(128);
            });

            modelBuilder.Entity<FieldChoice>(entity =>
            {
                entity.ToTable("FieldChoice");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(e => e.FieldDefinition)
                      .WithMany(e => e.Choices);                   
            });

            modelBuilder.Entity<FieldDefinition>(entity =>
            {
                entity.ToTable("FieldDefinition");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasOne(e => e.Version)
                      .WithMany(e => e.Definitions);  

                entity.Property(e => e.FieldName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasIndex(e => new { e.FieldName, e.VersionId })
                      .IsUnique();
            });
        }
    }
}
