using Datahub.Core.Data;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Announcements;
using Datahub.Core.Model.Catalog;
using Datahub.Core.Model.CloudStorage;
using Datahub.Core.Model.Documentation;
using Datahub.Core.Model.Health;
using Datahub.Core.Model.Onboarding;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Repositories;
using Datahub.Core.Model.UserTracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

namespace Datahub.Core.Model.Datahub;

/// <summary>
/// Datahub Main DbContext
/// Add a migration using PS command: Add-Migration MIGRATION_NAME -Context DatahubProjectDBContext
/// </summary>
public class DatahubProjectDBContext : DbContext //, ISeedable<DatahubProjectDBContext>
{
    public DatahubProjectDBContext(DbContextOptions<DatahubProjectDBContext> options) : base(options)
    {
    }

    public DbSet<DatahubProject> Projects { get; set; }
    public DbSet<PBILicenseRequest> PowerBILicenseRequests { get; set; }
    public DbSet<PBIUserLicenseRequest> PowerBILicenseUserRequests { get; set; }
    public DbSet<DatahubProjectComment> ProjectComments { get; set; }
    public DbSet<WebFormField> Fields { get; set; }
    public DbSet<WebForm> WebForms { get; set; }
    public DbSet<WebFormDBCodes> DBCodes { get; set; }
    public DbSet<DatahubProjectUser> ProjectUsers { get; set; }
    public DbSet<DatahubProjectUserRequest> ProjectUsersRequests { get; set; }
    public DbSet<DatahubProjectPipelineLnk> ProjectPipelineLinks { get; set; }
    public DbSet<OrganizationLevel> OrganizationLevels { get; set; }
    public DbSet<OnboardingApp> OnboardingApps { get; set; }

    public DbSet<ProjectResources2> ProjectResources2 { get; set; }

    public DbSet<PublicDataFile> PublicDataFiles { get; set; }

    public DbSet<SharedDataFile> SharedDataFiles { get; set; }
    public DbSet<OpenDataSharedFile> OpenDataSharedFiles { get; set; }

    public DbSet<SystemNotification> SystemNotifications { get; set; }

    public DbSet<DatahubProjectCosts> ProjectCosts { get; set; }
    public DbSet<ProjectCredits> ProjectCredits { get; set; }
    public DbSet<ProjectWhitelist> ProjectWhitelists { get; set; }

    public DbSet<ProjectStorage> ProjectStorageAvgs { get; set; }

    public DbSet<MiscStoredObject> MiscStoredObjects { get; set; }

    public DbSet<DatahubProjectApiUser> ProjectApiUsers { get; set; }
    public DbSet<PowerBiWorkspace> PowerBiWorkspaces { get; set; }
    public DbSet<PowerBiReport> PowerBiReports { get; set; }
    public DbSet<PowerBiDataSet> PowerBiDataSets { get; set; }

    public DbSet<SpatialObjectShare> GeoObjectShares { get; set; }
    public DbSet<ExternalPowerBiReport> ExternalPowerBiReports { get; set; }

    public DbSet<ClientEngagement> ClientEngagements { get; set; }

    public DbSet<Achievements.Achievement> Achievements { get; set; }
    public DbSet<Achievements.PortalUser> PortalUsers { get; set; }
    public DbSet<Achievements.UserAchievement> UserAchievements { get; set; }
    public DbSet<Achievements.TelemetryEvent> TelemetryEvents { get; set; }

    public DbSet<UserTracking.UserSettings> UserSettings { get; set; }

    public DbSet<UserTracking.UserRecentLink> UserRecentLinks { get; set; }

    public DbSet<Announcement> Announcements { get; set; }

    public DbSet<ProjectRepository> ProjectRepositories { get; set; }

    public DbSet<ProjectRole> ProjectRoles { get; set; }

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

    /// <summary>
    /// Gets or sets table for storing the infrastructure health checks
    /// </summary>
    public DbSet<InfrastructureHealthCheck> InfrastructureHealthChecks { get; set; }

    public void Seed(DatahubProjectDBContext context, IConfiguration configuration)
    {
        var p1 = context.Projects.Add(new DatahubProject()
        {
            ProjectID = 1,
            ProjectAcronymCD = RoleConstants.DATAHUBADMINPROJECT,
            ProjectStatusDesc = ProjectStatus.InProgress.ToString(),
            Project_Name = "Datahub Tracker",
            IsPrivate = false,
            ProjectIcon = "database",
            ProjectSummaryDesc = "Datahub Project Tracker",
            SectorName = "CIOSB"
        }).Entity;
        context.Projects.Add(
            new DatahubProject()
            {
                ProjectID = 2,
                ProjectAcronymCD = "TEST1",
                ProjectStatusDesc = ProjectStatus.InProgress.ToString(),
                Project_Name = "Test Project 1",
                IsPrivate = false,
                ProjectIcon = "database",
                ProjectSummaryDesc = "Test Project 1 for CFS",
                SectorName = "CFS"
            });
        context.Projects.Add(new DatahubProject()
        {
            ProjectID = 3,
            ProjectAcronymCD = "TEST2",
            ProjectStatusDesc = ProjectStatus.InProgress.ToString(),
            Project_Name = "Test Project 2",
            IsPrivate = false,
            ProjectIcon = "database",
            ProjectSummaryDesc = "Test Project 2 for CFS",
            SectorName = "CFS"
        });
        var initialSetup = configuration.GetSection("InitialSetup");
        if (initialSetup?.GetValue<string>("AdminGUID") != null)
        {
            var user = context.ProjectUsers.Add(new DatahubProjectUser()
            {
                PortalUser = new PortalUser()
                {
                    GraphGuid = initialSetup.GetValue<string>("AdminGUID"),
                },
                Project = p1,
                RoleId = (int)ProjectRole.RoleNames.Admin
            });
            //var permissions = context.Project_Users_Requests.Add(new Datahub_)
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatahubProjectDBContext).Assembly);

        modelBuilder.Entity<WebFormField>()
            .HasOne(p => p.WebForm)
            .WithMany(p => p.Fields);

        modelBuilder.Entity<WebFormField>()
            .Property(e => e.ExtensionCD)
            .HasDefaultValue("NONE");

        modelBuilder.Entity<WebFormField>()
            .Property(e => e.TypeCD)
            .HasDefaultValue("Text");

        modelBuilder.Entity<WebForm>().HasOne(e => e.Project).WithMany(e => e.WebForms);

        modelBuilder.Entity<DatahubProject>().HasOne(p => p.PBILicenseRequest).WithOne(p => p.Project).HasForeignKey<PBILicenseRequest>(l => l.ProjectID);
        modelBuilder.Entity<DatahubProject>().HasIndex(p => p.ProjectAcronymCD).IsUnique();

        modelBuilder.Entity<DatahubProjectComment>().HasOne(c => c.Project).WithMany(p => p.Comments);

        modelBuilder.Entity<DatahubProjectPipelineLnk>().HasKey(t => new { t.ProjectID, t.ProcessNm });

        modelBuilder.Entity<PowerBiWorkspace>()
            .HasOne<DatahubProject>(w => w.Project)
            .WithMany(p => p.PowerBiWorkspaces)
            .HasForeignKey(w => w.ProjectId);

        modelBuilder.Entity<PowerBiReport>()
            .HasOne<PowerBiWorkspace>(r => r.Workspace)
            .WithMany(w => w.Reports)
            .HasForeignKey(r => r.WorkspaceId);

        modelBuilder.Entity<PowerBiDataSet>()
            .HasOne<PowerBiWorkspace>(d => d.Workspace)
            .WithMany(w => w.Datasets)
            .HasForeignKey(d => d.WorkspaceId);

        modelBuilder.Entity<DatahubProject>()
            .HasOne(w => w.Sector)
            .WithMany(p => p.Sectors)
            .HasForeignKey(f => f.SectorId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<DatahubProject>()
            .HasOne(w => w.Branch)
            .WithMany(p => p.Branches)
            .HasForeignKey(f => f.BranchId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<DatahubProject>()
            .HasOne(w => w.Division)
            .WithMany(p => p.Divisions)
            .HasForeignKey(f => f.DivisionId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<DatahubProject>()
            .HasMany(w => w.ProjectInactivityNotifications)
            .WithOne(p => p.Project)
            .HasForeignKey(p => p.ProjectID)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<ProjectWhitelist>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Project)
                .WithOne(e => e.Whitelist)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<DatahubProjectCosts>(entity =>
        {
            entity.HasIndex(e => new { e.ProjectID, e.Date });
        });

        modelBuilder.Entity<ProjectCredits>(entity =>
        {
            entity.ToTable("Project_Credits");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Project)
                  .WithOne(e => e.Credits)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<PBIUserLicenseRequest>()
            .HasOne(p => p.LicenseRequest)
            .WithMany(b => b.UserLicenseRequests)
            .HasForeignKey(p => p.RequestID);

        modelBuilder.Entity<PublicDataFile>()
            .HasIndex(e => e.FileID)
            .IsUnique();

        modelBuilder.Entity<SharedDataFile>()
            .HasIndex(e => e.FileID)
            .IsUnique();

        modelBuilder.Entity<MiscStoredObject>()
            .HasAlternateKey(e => new { e.TypeName, e.Id });

        modelBuilder.Entity<SpatialObjectShare>()
            .ToTable("SpatialObjectShares");

        modelBuilder.Entity<DatahubProjectUser>()
            .Property(u => u.ProjectUserID);

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