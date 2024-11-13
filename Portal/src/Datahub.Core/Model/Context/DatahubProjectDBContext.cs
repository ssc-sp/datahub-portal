using Datahub.Core.Data;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Announcements;
using Datahub.Core.Model.Catalog;
using Datahub.Core.Model.CloudStorage;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Documentation;
using Datahub.Core.Model.Health;
using Datahub.Core.Model.Onboarding;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Repositories;
using Datahub.Core.Model.Subscriptions;
using Datahub.Core.Model.UserTracking;
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
    public DbSet<PBI_License_Request> PowerBI_License_Requests { get; set; }
    public DbSet<PBI_User_License_Request> PowerBI_License_User_Requests { get; set; }
    public DbSet<Datahub_ProjectComment> Project_Comments { get; set; }
    public DbSet<WebForm_Field> Fields { get; set; }
    public DbSet<WebForm> WebForms { get; set; }
    public DbSet<WebForm_DBCodes> DBCodes { get; set; }
    public DbSet<Datahub_Project_User> Project_Users { get; set; }
    public DbSet<Datahub_Project_User_Request> Project_Users_Requests { get; set; }
    public DbSet<Datahub_Project_Pipeline_Lnk> Project_Pipeline_Links { get; set; }
    public DbSet<Organization_Level> Organization_Levels { get; set; }
    public DbSet<OnboardingApp> OnboardingApps { get; set; }

    public DbSet<Project_Resources2> Project_Resources2 { get; set; }

    public DbSet<PublicDataFile> PublicDataFiles { get; set; }

    public DbSet<SharedDataFile> SharedDataFiles { get; set; }
    public DbSet<OpenDataSharedFile> OpenDataSharedFiles { get; set; }

    public DbSet<SystemNotification> SystemNotifications { get; set; }

    public DbSet<Datahub_Project_Costs> Project_Costs { get; set; }
    public DbSet<Project_Credits> Project_Credits { get; set; }
    public DbSet<Project_Whitelist> Project_Whitelists { get; set; }

    public DbSet<Project_Storage> Project_Storage_Avgs { get; set; }

    public DbSet<MiscStoredObject> MiscStoredObjects { get; set; }

    public DbSet<Datahub_ProjectApiUser> Project_ApiUsers { get; set; }
    public DbSet<PowerBi_Workspace> PowerBi_Workspaces { get; set; }
    public DbSet<PowerBi_Report> PowerBi_Reports { get; set; }
    public DbSet<PowerBi_DataSet> PowerBi_DataSets { get; set; }

    public DbSet<SpatialObjectShare> GeoObjectShares { get; set; }
    public DbSet<ExternalPowerBiReport> ExternalPowerBiReports { get; set; }

    public DbSet<Client_Engagement> Client_Engagements { get; set; }

    public DbSet<Achievements.Achievement> Achievements { get; set; }
    public DbSet<Achievements.PortalUser> PortalUsers { get; set; }
    public DbSet<Achievements.UserAchievement> UserAchievements { get; set; }
    public DbSet<Achievements.TelemetryEvent> TelemetryEvents { get; set; }

    public DbSet<UserTracking.UserSettings> UserSettings { get; set; }

    public DbSet<UserTracking.UserRecentLink> UserRecentLinks { get; set; }

    public DbSet<Announcement> Announcements { get; set; }

    public DbSet<ProjectRepository> ProjectRepositories { get; set; }

    public DbSet<Project_Role> Project_Roles { get; set; }

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

#pragma warning disable SX1309
    private readonly DbContextOptions<DatahubProjectDBContext> options;
#pragma warning restore SX1309

    // below are used for migrations
#if MIGRATION
    public DatahubProjectDBContext() { }
#endif

    public DatahubProjectDBContext(DbContextOptions<DatahubProjectDBContext> options) : base(options)
    {
        this.options = options;
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
            Sector_Name = "CIOSB"
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
                Project_Summary_Desc = "Test Project 1 for CFS",
                Sector_Name = "CFS"
            });
        context.Projects.Add(new Datahub_Project()
        {
            Project_ID = 3,
            Project_Acronym_CD = "TEST2",
            Project_Status_Desc = ProjectStatus.InProgress.ToString(),
            Project_Name = "Test Project 2",
            Is_Private = false,
            Project_Icon = "database",
            Project_Summary_Desc = "Test Project 2 for CFS",
            Sector_Name = "CFS"
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

        modelBuilder.Entity<WebForm_Field>()
            .HasOne(p => p.WebForm)
            .WithMany(p => p.Fields);

        modelBuilder.Entity<WebForm_Field>()
            .Property(e => e.Extension_CD)
            .HasDefaultValue("NONE");

        modelBuilder.Entity<WebForm_Field>()
            .Property(e => e.Type_CD)
            .HasDefaultValue("Text");

        modelBuilder.Entity<WebForm>().HasOne(e => e.Project).WithMany(e => e.WebForms);

        modelBuilder.Entity<Datahub_Project>().HasOne(p => p.PBI_License_Request).WithOne(p => p.Project).HasForeignKey<PBI_License_Request>(l => l.Project_ID);
        modelBuilder.Entity<Datahub_Project>().HasIndex(p => p.Project_Acronym_CD).IsUnique();
        modelBuilder.Entity<Datahub_Project>().Property(p => p.WebAppUrlRewritingEnabled).HasDefaultValue(true);

        modelBuilder.Entity<Datahub_ProjectComment>().HasOne(c => c.Project).WithMany(p => p.Comments);

        modelBuilder.Entity<Datahub_Project_Pipeline_Lnk>().HasKey(t => new { t.Project_ID, t.Process_Nm });

        modelBuilder.Entity<PowerBi_Workspace>()
            .HasOne<Datahub_Project>(w => w.Project)
            .WithMany(p => p.PowerBi_Workspaces)
            .HasForeignKey(w => w.Project_Id);

        modelBuilder.Entity<PowerBi_Report>()
            .HasOne<PowerBi_Workspace>(r => r.Workspace)
            .WithMany(w => w.Reports)
            .HasForeignKey(r => r.Workspace_Id);

        modelBuilder.Entity<PowerBi_DataSet>()
            .HasOne<PowerBi_Workspace>(d => d.Workspace)
            .WithMany(w => w.Datasets)
            .HasForeignKey(d => d.Workspace_Id);

        modelBuilder.Entity<Datahub_Project>()
            .HasOne(w => w.Sector)
            .WithMany(p => p.Sectors)
            .HasForeignKey(f => f.SectorId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Datahub_Project>()
            .HasOne(w => w.Branch)
            .WithMany(p => p.Branches)
            .HasForeignKey(f => f.BranchId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Datahub_Project>()
            .HasOne(w => w.Division)
            .WithMany(p => p.Divisions)
            .HasForeignKey(f => f.DivisionId)
            .OnDelete(DeleteBehavior.NoAction);

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

        modelBuilder.Entity<PBI_User_License_Request>()
            .HasOne(p => p.LicenseRequest)
            .WithMany(b => b.User_License_Requests)
            .HasForeignKey(p => p.RequestID);

        modelBuilder.Entity<PublicDataFile>()
            .HasIndex(e => e.File_ID)
            .IsUnique();

        modelBuilder.Entity<SharedDataFile>()
            .HasIndex(e => e.File_ID)
            .IsUnique();

        modelBuilder.Entity<MiscStoredObject>()
            .HasAlternateKey(e => new { e.TypeName, e.Id });

        modelBuilder.Entity<SpatialObjectShare>()
            .ToTable("SpatialObjectShares");

        modelBuilder.Entity<Datahub_Project_User>()
            .Property(u => u.ProjectUser_ID);

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