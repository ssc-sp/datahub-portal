using Microsoft.EntityFrameworkCore;

namespace Datahub.Metadata.Model;

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
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<SubSubject> SubSubjects { get; set; }
    public DbSet<MetadataProfile> Profiles { get; set; }
    public DbSet<MetadataSection> Sections { get; set; }
    public DbSet<SectionField> SectionFields { get; set; }
    public DbSet<CatalogObject> CatalogObjects { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MetadataVersion>(entity =>
        {
            entity.ToTable("MetadataVersions");

            entity.HasKey(e => e.MetadataVersionId);
            entity.Property(e => e.MetadataVersionId).ValueGeneratedOnAdd();

            entity.Property(e => e.SourceTXT).HasMaxLength(32);

            entity.Property(e => e.VersionInfoTXT).HasMaxLength(128);
        });

        modelBuilder.Entity<FieldDefinition>(entity =>
        {
            entity.ToTable("FieldDefinitions");

            entity.HasKey(e => e.FieldDefinitionId);
            entity.Property(e => e.FieldDefinitionId).ValueGeneratedOnAdd();

            entity.HasOne(e => e.MetadataVersion)
                .WithMany(e => e.Definitions);

            entity.Property(e => e.FieldNameTXT)
                .IsRequired()
                .HasMaxLength(128);

            entity.HasIndex(e => new { e.FieldNameTXT, e.MetadataVersionId })
                .IsUnique();
        });

        modelBuilder.Entity<FieldChoice>(entity =>
        {
            entity.ToTable("FieldChoices");

            entity.HasKey(e => e.FieldChoiceId);
            entity.Property(e => e.FieldChoiceId).ValueGeneratedOnAdd();

            entity.Property(e => e.ValueTXT)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(e => e.CascadingValueTXT)
                .HasMaxLength(128);

            entity.HasOne(e => e.FieldDefinition)
                .WithMany(e => e.Choices);
        });

        modelBuilder.Entity<ObjectMetadata>(entity =>
        {
            entity.ToTable("ObjectMetadata");

            entity.HasKey(e => e.ObjectMetadataId);
            entity.Property(e => e.ObjectMetadataId).ValueGeneratedOnAdd();

            entity.Property(e => e.ObjectIdTXT)
                .IsRequired()
                .HasMaxLength(128);

            entity.HasOne(e => e.MetadataVersion)
                .WithMany(e => e.Objects);

            entity.HasIndex(e => e.ObjectIdTXT)
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

            entity.Property(e => e.EnglishTXT).HasMaxLength(128);
            entity.HasIndex(e => e.EnglishTXT)
                .IsUnique();

            entity.Property(e => e.FrenchTXT).HasMaxLength(128);
            entity.HasIndex(e => e.FrenchTXT)
                .IsUnique();

            entity.Property(e => e.Source).HasMaxLength(64);
        });

        modelBuilder.Entity<ApprovalForm>(entity =>
        {
            entity.ToTable("ApprovalForms");

            entity.HasKey(e => e.ApprovalFormId);
            entity.Property(e => e.ApprovalFormId).ValueGeneratedOnAdd();

            entity.Property(e => e.DepartmentNAME).HasMaxLength(256);
            entity.Property(e => e.SectorNAME).HasMaxLength(256);
            entity.Property(e => e.BranchNAME).HasMaxLength(256);
            entity.Property(e => e.DivisionNAME).HasMaxLength(256);
            entity.Property(e => e.SectionNAME).HasMaxLength(256);

            entity.Property(e => e.NameNAME).HasMaxLength(256).IsRequired();
            entity.Property(e => e.PhoneTXT).HasMaxLength(32);

            entity.Property(e => e.EmailEMAIL).HasMaxLength(128).IsRequired();

            entity.Property(e => e.DatasetTitleTXT).HasMaxLength(256).IsRequired();
            entity.Property(e => e.TypeOfDataTXT).HasMaxLength(16).IsRequired();
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("Subjects");
            entity.HasKey(e => e.SubjectId);

            entity.Property(e => e.SubjectTXT).HasMaxLength(64);
            entity.HasIndex(e => e.SubjectTXT).IsUnique();
        });

        modelBuilder.Entity<SubSubject>(entity =>
        {
            entity.ToTable("SubSubjects");
            entity.HasKey(e => e.SubSubjectId);
        });

        modelBuilder.Entity<MetadataProfile>(entity =>
        {
            entity.ToTable("Profiles");

            entity.HasKey(e => e.ProfileId);
            entity.Property(e => e.ProfileId).ValueGeneratedOnAdd();

            entity.Property(e => e.Name).HasMaxLength(32);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<MetadataSection>(entity =>
        {
            entity.ToTable("Sections");

            entity.HasKey(e => e.SectionId);
            entity.Property(e => e.SectionId).ValueGeneratedOnAdd();

            entity.Property(e => e.NameEnglishTXT).HasMaxLength(256);
            entity.Property(e => e.NameFrenchTXT).HasMaxLength(256);

            entity.HasOne(e => e.Profile)
                .WithMany(e => e.Sections);
        });

        modelBuilder.Entity<SectionField>(entity =>
        {
            entity.ToTable("SectionFields");

            entity.HasKey(e => new { e.SectionId, e.FieldDefinitionId });

            entity.HasOne(e => e.Section)
                .WithMany(e => e.Fields);

            entity.HasOne(e => e.FieldDefinition)
                .WithMany(e => e.SectionFields);
        });

        modelBuilder.Entity<CatalogObject>(entity =>
        {
            entity.ToTable("CatalogObjects");

            entity.HasKey(e => e.CatalogObjectId);
            entity.Property(e => e.CatalogObjectId).ValueGeneratedOnAdd();

            entity.HasOne(e => e.ObjectMetadata)
                .WithMany(e => e.CatalogObjects);

            entity.Property(e => e.NameTXT).IsRequired();
            entity.Property(e => e.SecurityClassTXT).HasDefaultValue("Unclassified").IsRequired();

            entity.HasIndex(e => e.GroupId);
        });
    }
}