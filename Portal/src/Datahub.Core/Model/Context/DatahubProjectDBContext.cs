using Datahub.Core.Data;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Announcements;
using Datahub.Core.Model.Catalog;
using Datahub.Core.Model.CloudStorage;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Documentation;
using Datahub.Core.Model.Onboarding;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Repositories;
using Datahub.Core.Model.Subscriptions;
using Datahub.Core.Model.UserTracking;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

namespace Datahub.Core.Model.Context;

/// <summary>
/// Datahub Main DbContext
/// Add a migration using PS command: Add-Migration MIGRATION_NAME -Context DatahubProjectDBContext
/// </summary>
public class DatahubProjectDBContext : DbContext //, ISeedable<DatahubProjectDBContext>
{
    public DbSet<Datahub_Project> Projects { get; set; }
    public DbSet<Datahub_Project_User> Project_Users { get; set; }

    public DbSet<Project_Resources2> Project_Resources2 { get; set; }

    public DbSet<SharedDataFile> SharedDataFiles { get; set; }
    public DbSet<OpenDataSharedFile> OpenDataSharedFiles { get; set; }

    public DbSet<SystemNotification> SystemNotifications { get; set; }

    public DbSet<Datahub_Project_Costs> Project_Costs { get; set; }
    public DbSet<Project_Credits> Project_Credits { get; set; }
    public DbSet<Project_Whitelist> Project_Whitelists { get; set; }

    public DbSet<Project_Storage> Project_Storage_Avgs { get; set; }

    public DbSet<MiscStoredObject> MiscStoredObjects { get; set; }

    public DbSet<Datahub_ProjectApiUser> Project_ApiUsers { get; set; }
    public DbSet<Achievements.Achievement> Achievements { get; set; }
    public DbSet<Achievements.PortalUser> PortalUsers { get; set; }
    public DbSet<PortalUserRoleChange> PortalUserStatusChanges { get; set; }

    public DbSet<Achievements.UserAchievement> UserAchievements { get; set; }
    public DbSet<Achievements.TelemetryEvent> TelemetryEvents { get; set; }

    public DbSet<UserTracking.UserSettings> UserSettings { get; set; }

    public DbSet<UserTracking.UserRecentLink> UserRecentLinks { get; set; }

    public DbSet<Announcement> Announcements { get; set; }

    public DbSet<ProjectRepository> ProjectRepositories { get; set; }

    public DbSet<Project_Role> Project_Roles { get; set; }

    public DbSet<PortalUserRoleChange> PortalUserRoleChanges { get; set; }

    public DbSet<ProjectInactivityNotifications> ProjectInactivityNotifications { get; set; }

    public DbSet<UserInactivityNotifications> UserInactivityNotifications { get; set; }

    public DbSet<DocumentationResource> DocumentationResources { get; set; }

    /// <summary>
    /// Gets or sets table for storing any additional details when users go through the self registration process
    /// </summary>
    public DbSet<SelfRegistrationDetails> SelfRegistrationDetails { get; set; }

    /// <summary>
    /// Gets or sets table for storing any additional details when users go through the project creation process
    /// </summary>
    public DbSet<ProjectCreationDetails> ProjectCreationDetails { get; set; }

    /// <summary>
    /// Gets or sets cataloged objects
    /// </summary>
    public DbSet<CatalogObject> CatalogObjects { get; set; }

    /// <summary>
    /// Gets or sets table for storing the cloud storage associcated to a project
    /// </summary>
    public DbSet<ProjectCloudStorage> ProjectCloudStorages { get; set; }

    public DbSet<OpenDataSubmission> OpenDataSubmissions { get; set; }

    public DbSet<OpenDataPublishFile> OpenDataPublishFiles { get; set; }

    public DbSet<TbsOpenGovSubmission> TbsOpenGovSubmissions { get; set; }

    /// <summary>
    /// Gets or sets the table for storing the GC hosting info
    /// </summary>
    public DbSet<GCHostingWorkspaceDetails> GCHostingWorkspaceDetails { get; set; }

    /// <summary>
    /// Gets or sets the table for storing the Azure subscriptions
    /// </summary>
    public DbSet<DatahubAzureSubscription> AzureSubscriptions { get; set; }

    /// <summary>
    /// Gets or sets table for storing the infrastructure health checks
    /// </summary>
    public DbSet<InfrastructureHealthCheck> InfrastructureHealthChecks { get; set; }

    /// <summary>
    /// Gets or sets table for storing the history of infrastructure health checks
    /// </summary>
    public DbSet<InfrastructureHealthCheck> InfrastructureHealthCheckRuns { get; set; }

    /// <summary>
    /// Gets or sets workspace lead confirmations for deleted projects
    /// </summary>
    public DbSet<Project_Delete_Questionnaire> Project_Delete_Questionnaires { get; set; }

    /// <summary>
    /// Gets or sets datahub versions
    /// </summary>
    public DbSet<VersionTag> VersionTags { get; set; }

    private DbContextOptions<DatahubProjectDBContext> _options;
    // below are used for migrations
#if MIGRATION

    public DatahubProjectDBContext() { }
#endif

    public DatahubProjectDBContext(DbContextOptions<DatahubProjectDBContext> options) : base(options)
    {
        this._options = options;
    }

    protected DatahubProjectDBContext(DbContextOptions options) : base(options)
    {
    }

    public void Seed(DatahubProjectDBContext context, IConfiguration configuration)
    {
        var p1 = context.Projects.Add(new Datahub_Project()
        {
            Project_ID = 1,
            Project_Acronym_CD = RoleConstants.DATAHUB_ADMIN_PROJECT,
            Project_Status_Desc = ProjectStatus.InProgress.ToString(),
            Project_Name = "Datahub Tracker",
            Is_Private = false,
            Project_Icon = "database",
            Project_Summary_Desc = "Datahub Project Tracker",
        }).Entity;
        context.Projects.Add(
            new Datahub_Project()
            {
                Project_ID = 2,
                Project_Acronym_CD = "TEST1",
                Project_Status_Desc = ProjectStatus.InProgress.ToString(),
                Project_Name = "Test Project 1",
                Is_Private = false,
                Project_Icon = "database",
                Project_Summary_Desc = "Test Project 1 for CFS"
            });
        context.Projects.Add(new Datahub_Project()
        {
            Project_ID = 3,
            Project_Acronym_CD = "TEST2",
            Project_Status_Desc = ProjectStatus.InProgress.ToString(),
            Project_Name = "Test Project 2",
            Is_Private = false,
            Project_Icon = "database",
            Project_Summary_Desc = "Test Project 2 for CFS"
        });
        var initialSetup = configuration.GetSection("InitialSetup");
        if (initialSetup?.GetValue<string>("AdminGUID") != null)
        {
            var user = context.Project_Users.Add(new Datahub_Project_User()
            {
                PortalUser = new PortalUser()
                {
                    GraphGuid = initialSetup.GetValue<string>("AdminGUID"),
                },
                Project = p1,
                RoleId = (int)Project_Role.RoleNames.Admin
            });
            //var permissions = context.Project_Users_Requests.Add(new Datahub_)
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatahubProjectDBContext).Assembly);

        modelBuilder.Entity<Datahub_Project>().HasIndex(p => p.Project_Acronym_CD).IsUnique();
        modelBuilder.Entity<Datahub_Project>().Property(p => p.WebAppUrlRewritingEnabled).HasDefaultValue(true);

        modelBuilder.Entity<Datahub_Project>()
            .HasMany(w => w.ProjectInactivityNotifications)
            .WithOne(p => p.Project)
            .HasForeignKey(p => p.Project_ID)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Project_Whitelist>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Project)
                .WithOne(e => e.Whitelist)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Datahub_Project_Costs>(entity =>
        {
            entity.HasIndex(e => new { e.Project_ID, e.Date });
        });

        modelBuilder.Entity<Project_Credits>(entity =>
        {
            entity.ToTable("Project_Credits");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Project)
                  .WithOne(e => e.Credits)
                  .OnDelete(DeleteBehavior.NoAction);
        });
        modelBuilder.Entity<PortalUserRoleChange>()
            .Property(p => p.RoleId)
            .HasConversion<int>();

        modelBuilder.Entity<PortalUserRoleChange>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChangeDate).IsRequired();
        });

        modelBuilder.Entity<SharedDataFile>()
            .HasIndex(e => e.File_ID)
            .IsUnique();

        modelBuilder.Entity<MiscStoredObject>()
            .HasAlternateKey(e => new { e.TypeName, e.Id });

        modelBuilder.Entity<Datahub_Project_User>()
            .HasKey(u => u.ProjectUser_ID);

        modelBuilder.Entity<Datahub_Project_User>()
            .Property(u => u.ProjectUser_ID);

        modelBuilder.Entity<Datahub_Project_User>()
            .HasIndex(u => new { u.Project_ID, u.PortalUserId })
            .IsUnique();

        modelBuilder.Entity<OpenDataSubmission>()
            .HasMany<OpenDataPublishFile>(p => p.Files)
            .WithOne(f => f.Submission)
            .HasForeignKey(f => f.SubmissionId);

        modelBuilder.Entity<OpenDataSubmission>()
            .HasOne<Datahub_Project>(p => p.Project)
            .WithMany(p => p.PublishingSubmissions)
            .HasForeignKey(p => p.ProjectId);

        modelBuilder.Entity<OpenDataSubmission>()
            .HasOne<PortalUser>(p => p.RequestingUser)
            .WithMany(p => p.OpenDataSubmissions)
            .HasForeignKey(p => p.RequestingUserId);

        modelBuilder.Entity<OpenDataSubmission>()
            .Property(s => s.UniqueId)
            .IsRequired();

        modelBuilder.Entity<OpenDataSubmission>()
            .HasIndex(s => s.UniqueId)
            .IsUnique();

        modelBuilder.Entity<OpenDataSubmission>()
            .UseTptMappingStrategy();

        modelBuilder.Entity<OpenDataPublishFile>()
            .HasOne<ProjectCloudStorage>(f => f.Storage)
            .WithMany(s => s.PublishingSubmissionFiles)
            .HasForeignKey(f => f.ProjectStorageId);

        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
            // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
            // To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
            // use the DateTimeOffsetToBinaryConverter
            // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
            // This only supports millisecond precision, but should be sufficient for most use cases.
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(DateTimeOffset)
                                                                               || p.PropertyType == typeof(DateTimeOffset?));
                foreach (var property in properties)
                {
                    modelBuilder
                        .Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(new DateTimeOffsetToBinaryConverter());
                }
            }
        }
    }
}