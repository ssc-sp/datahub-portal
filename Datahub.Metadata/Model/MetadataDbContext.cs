using Microsoft.EntityFrameworkCore;

namespace NRCan.Datahub.Metadata.Model
{
    public class MetadataDbContext : DbContext
    {
        public MetadataDbContext(DbContextOptions<MetadataDbContext> options) : base(options)
        {
        }

        public DbSet<MetadataVersion> MetadataVersions { get; set; }
        public DbSet<FieldDefinition> FieldDefinitions { get; set; }
        public DbSet<FieldChoice> FieldChoices { get; set; }
        public DbSet<ObjectMetadata> ObjectMetadataSet { get; set; }
        public DbSet<ObjectFieldValue> ObjectFieldValues { get; set; }
        public DbSet<ApprovalForm> ApprovalForms { get; set; }
        public DbSet<Keyword> Keywords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MetadataVersion>(entity =>
            {
                entity.ToTable("MetadataVersions");

                entity.HasKey(e => e.MetadataVersionId);
                entity.Property(e => e.MetadataVersionId).ValueGeneratedOnAdd();

                entity.Property(e => e.Source_TXT).HasMaxLength(32);

                entity.Property(e => e.Version_Info_TXT).HasMaxLength(128);
            });

            modelBuilder.Entity<FieldDefinition>(entity =>
            {
                entity.ToTable("FieldDefinitions");

                entity.HasKey(e => e.FieldDefinitionId);
                entity.Property(e => e.FieldDefinitionId).ValueGeneratedOnAdd();

                entity.HasOne(e => e.MetadataVersion)
                      .WithMany(e => e.Definitions);

                entity.Property(e => e.Field_Name_TXT)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasIndex(e => new { e.Field_Name_TXT, e.MetadataVersionId })
                      .IsUnique();
            });

            modelBuilder.Entity<FieldChoice>(entity =>
            {
                entity.ToTable("FieldChoices");

                entity.HasKey(e => e.FieldChoiceId);
                entity.Property(e => e.FieldChoiceId).ValueGeneratedOnAdd();

                entity.Property(e => e.Value_TXT)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(e => e.FieldDefinition)
                      .WithMany(e => e.Choices);                   
            });

            modelBuilder.Entity<ObjectMetadata>(entity => 
            {
                entity.ToTable("ObjectMetadata");

                entity.HasKey(e => e.ObjectMetadataId);
                entity.Property(e => e.ObjectMetadataId).ValueGeneratedOnAdd();

                entity.Property(e => e.ObjectId_TXT)
                      .IsRequired()
                      .HasMaxLength(128);

                entity.HasOne(e => e.MetadataVersion)
                      .WithMany(e => e.Objects);

                entity.HasIndex(e => e.ObjectId_TXT)
                      .IsUnique();
            });

            modelBuilder.Entity<ObjectFieldValue>(entity =>
            {
                entity.ToTable("ObjectFieldValues");

                entity.HasKey(e => new { e.ObjectMetadataId, e.FieldDefinitionId });

                entity.HasOne(e => e.ObjectMetadata)
                      .WithMany(e => e.FieldValues)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.FieldDefinition)
                      .WithMany(e => e.FieldValues)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Keyword>(entity =>
            {
                entity.ToTable("Keywords");

                entity.HasKey(e => e.KeywordId);
                entity.Property(e => e.KeywordId).ValueGeneratedOnAdd();

                entity.Property(e => e.English_TXT).HasMaxLength(128);
                entity.HasIndex(e => e.English_TXT)
                      .IsUnique();

                entity.Property(e => e.French_TXT).HasMaxLength(128);
                entity.HasIndex(e => e.French_TXT)
                      .IsUnique();

                entity.Property(e => e.Source).HasMaxLength(64);
            });

            modelBuilder.Entity<ApprovalForm>(entity =>
            {
                entity.ToTable("ApprovalForms");

                entity.HasKey(e => e.ApprovalFormId);
                entity.Property(e => e.ApprovalFormId).ValueGeneratedOnAdd();

                entity.Property(e => e.Dataset_Title_TXT).HasMaxLength(256).IsRequired();
                entity.Property(e => e.Type_Of_Data_TXT).HasMaxLength(16).IsRequired();
            });
        }
    }
}
