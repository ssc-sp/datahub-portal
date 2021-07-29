using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NRCan.Datahub.Data.Projects
{
    public class DatahubProjectDBContext : DbContext
    {
        public DatahubProjectDBContext(DbContextOptions<DatahubProjectDBContext> options) : base(options)
        { }

        public DbSet<Datahub_Project> Projects { get; set; }

        public DbSet<PBI_License_Request> PowerBI_License_Requests { get; set; }

        public DbSet<PBI_User_License_Request> PowerBI_License_User_Requests { get; set; }

        public DbSet<Datahub_ProjectComment> Project_Comments { get; set; }

        public DbSet<Datahub_Project_Access_Request> Access_Requests { get; set; }

        public DbSet<WebForm_Field> Fields { get; set; }

        public DbSet<WebForm> WebForms { get; set; }

        public DbSet<WebForm_DBCodes> DBCodes { get; set; }

        public DbSet<Datahub_Project_User> Project_Users { get; set; }

        public DbSet<Datahub_ProjectServiceRequests> Project_Requests { get; set; }

        public DbSet<Datahub_Project_Pipeline_Lnk> Project_Pipeline_Links { get; set; }

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

            modelBuilder.Entity<Datahub_ProjectComment>().HasOne(c => c.Project).WithMany(p => p.Comments);

            modelBuilder.Entity<Datahub_Project_Pipeline_Lnk>().HasKey(t => new { t.Project_ID, t.Process_Nm });

            //modelBuilder.Entity<Datahub_ProjectServiceRequests>().HasOne(c => c.Project).WithOne(p => p.Branch_Name);

            modelBuilder.Entity<PBI_User_License_Request>()
                .HasOne(p => p.LicenseRequest)
                .WithMany(b => b.User_License_Requests)
                .HasForeignKey(p => p.RequestID);

            modelBuilder.Entity<Datahub_Project>().HasData(new Datahub_Project() { Project_ID = 1, 
                Project_Acronym_CD = "DHTRK", 
                Project_Status_Desc = Datahub_Project.ONGOING, 
                Project_Name = "Datahub Projects", 
                Is_Private = false,
                Project_Icon = "database",
                Project_Summary_Desc = "Datahub Project Tracker",
                Sector_Name = "CIOSB"});
            }
    }
}
