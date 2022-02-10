using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Datahub.Core.Data;
using Datahub.Core.EFCore;
using Datahub.Core.Model.Onboarding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Core.EFCore
{
    public class DatahubProjectDBContext : DbContext //, ISeedable<DatahubProjectDBContext>
    {
        public DatahubProjectDBContext(DbContextOptions<DatahubProjectDBContext> options) : base(options)
        {
        }

        public DbSet<Datahub_Project> Projects { get; set; }
        public DbSet<PBI_License_Request> PowerBI_License_Requests { get; set; }
        public DbSet<PBI_User_License_Request> PowerBI_License_User_Requests { get; set; }
        public DbSet<Datahub_ProjectComment> Project_Comments { get; set; }
        public DbSet<Datahub_Project_Access_Request> Access_Requests { get; set; }
        public DbSet<WebForm_Field> Fields { get; set; }
        public DbSet<WebForm> WebForms { get; set; }
        public DbSet<WebForm_DBCodes> DBCodes { get; set; }
        public DbSet<Datahub_Project_User> Project_Users { get; set; }
        public DbSet<Datahub_Project_User_Request> Project_Users_Requests { get; set; }
        public DbSet<Datahub_ProjectServiceRequests> Project_Requests { get; set; }
        public DbSet<Datahub_Project_Pipeline_Lnk> Project_Pipeline_Links { get; set; }
        public DbSet<Project_Database> Project_Databases { get; set; }
        public DbSet<Datahub_Project_Sectors_And_Branches> Organization_Levels { get; set; }
        public DbSet<OnboardingApp> OnboardingApps {  get; set; }
        public DbSet<Project_Resources> Project_Resources { get; set; }
        public DbSet<Project_PBI_DataSet> Project_PBI_DataSets { get; set; }
        public DbSet<Project_PBI_Report> Project_PBI_Reports { get; set; }
        public DbSet<Project_PBI_Workspace> Project_PBI_Workspaces { get; set; }
        public DbSet<PublicDataFile> PublicDataFiles { get; set; }

        public DbSet<SharedDataFile> SharedDataFiles { get; set; }
        public DbSet<OpenDataSharedFile> OpenDataSharedFiles { get; set; }

        public DbSet<SystemNotification> SystemNotifications { get; set; }

        public DbSet<Datahub_Project_Costs> Project_Costs { get; set; }

        public DbSet<MiscStoredObject> MiscStoredObjects { get; set; }

        public DbSet<Datahub_ProjectApiUser> Project_ApiUsers { get; set; }

        public void Seed(DatahubProjectDBContext context, IConfiguration configuration)
        {
            var p1 = context.Projects.Add(new Datahub_Project()
            {
                Project_ID = 1,
                Project_Acronym_CD = RoleConstants.DATAHUB_ADMIN_PROJECT,
                Project_Status_Desc = Datahub_Project.ONGOING,
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
                    Project_Status_Desc = Datahub_Project.ONGOING,
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
                    Project_Status_Desc = Datahub_Project.ONGOING,
                    Project_Name = "Test Project 2",
                    Is_Private = false,
                    Project_Icon = "database",
                    Project_Summary_Desc = "Test Project 2 for CFS",
                    Sector_Name = "CFS"
                });
            var initialSetup = configuration.GetSection("InitialSetup");
            if (initialSetup?.GetValue<string>("AdminGUID") != null)
            {
                var user = context.Project_Users.Add(new Datahub_Project_User() { User_ID = initialSetup.GetValue<string>("AdminGUID"), IsAdmin = true, ProjectUser_ID = 1, Project = p1 });
                //var permissions = context.Project_Users_Requests.Add(new Datahub_)
            }

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

            modelBuilder.Entity<Datahub_Project>()
                .HasCheckConstraint("CHK_DB_Type", "DB_Type in ('SQL Server', 'Postgres')");

            modelBuilder.Entity<Datahub_ProjectComment>().HasOne(c => c.Project).WithMany(p => p.Comments);

            modelBuilder.Entity<Datahub_Project_Pipeline_Lnk>().HasKey(t => new { t.Project_ID, t.Process_Nm });

            //modelBuilder.Entity<Datahub_ProjectServiceRequests>().HasOne(c => c.Project).WithOne(p => p.Branch_Name);

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

        }
    }
}
