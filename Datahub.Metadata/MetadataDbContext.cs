using Microsoft.EntityFrameworkCore;

namespace NRCan.Datahub.Metadata
{
    public class MetadataDbContext : DbContext
    {
        public MetadataDbContext(DbContextOptions<MetadataDbContext> options) : base(options)
        {
        }

        public DbSet<FieldDefinition> FieldDefinitions { get; set; }
        public DbSet<FieldChoice> FieldChoices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            

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

                entity.Property(e => e.FieldName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasIndex(e => e.FieldName)
                      .IsUnique();
            });
        }
    }
}
