﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NRCan.Datahub.Data.Projects;

namespace NRCan.Datahub.ProjectForms.Migrations.DatahubProjectDB
{
    [DbContext(typeof(DatahubProjectDBContext))]
    [Migration("20201126012432_SoftDeleteTS2")]
    partial class SoftDeleteTS2
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("NRCan.Datahub.Data.Projects.Datahub_Project", b =>
                {
                    b.Property<int>("Project_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Branch_Name")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Comments_NT")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Contact_List")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("Deleted_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Division_Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GC_Docs_URL")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Initial_Meeting_DT")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Last_Contact_DT")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("Last_Updated_DT")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Next_Meeting_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Project_Acronym_CD")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("Project_Name")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Project_Status_Desc")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Project_Summary_Desc")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Sector_Name")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("Stage_Desc")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Project_ID");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("NRCan.Datahub.Data.Projects.Datahub_ProjectComment", b =>
                {
                    b.Property<int>("Comment_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<DateTime>("Comment_Date_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Comment_NT")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Project_ID")
                        .HasColumnType("int");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Comment_ID");

                    b.HasIndex("Project_ID");

                    b.ToTable("Project_Comments");
                });

            modelBuilder.Entity("NRCan.Datahub.Data.Projects.PBI_License_Request", b =>
                {
                    b.Property<int>("Request_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Contact_Email")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Contact_Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<bool>("Desktop_Usage_Flag")
                        .HasColumnType("bit");

                    b.Property<bool>("Premium_License_Flag")
                        .HasColumnType("bit");

                    b.Property<int>("Project_ID")
                        .HasColumnType("int");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("User_ID")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Request_ID");

                    b.HasIndex("Project_ID")
                        .IsUnique();

                    b.ToTable("PowerBI_License_Requests");
                });

            modelBuilder.Entity("NRCan.Datahub.Data.Projects.PBI_User_License_Request", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("LicenseType")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<int>("RequestID")
                        .HasColumnType("int");

                    b.Property<string>("UserEmail")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.HasKey("ID");

                    b.HasIndex("RequestID");

                    b.ToTable("PowerBI_License_User_Requests");
                });

            modelBuilder.Entity("NRCan.Datahub.Data.Projects.Datahub_ProjectComment", b =>
                {
                    b.HasOne("NRCan.Datahub.Data.Projects.Datahub_Project", "Project")
                        .WithMany("Comments")
                        .HasForeignKey("Project_ID");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("NRCan.Datahub.Data.Projects.PBI_License_Request", b =>
                {
                    b.HasOne("NRCan.Datahub.Data.Projects.Datahub_Project", "Project")
                        .WithOne("PBI_License_Request")
                        .HasForeignKey("NRCan.Datahub.Data.Projects.PBI_License_Request", "Project_ID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");
                });

            modelBuilder.Entity("NRCan.Datahub.Data.Projects.PBI_User_License_Request", b =>
                {
                    b.HasOne("NRCan.Datahub.Data.Projects.PBI_License_Request", "LicenseRequest")
                        .WithMany("User_License_Requests")
                        .HasForeignKey("RequestID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("LicenseRequest");
                });

            modelBuilder.Entity("NRCan.Datahub.Data.Projects.Datahub_Project", b =>
                {
                    b.Navigation("Comments");

                    b.Navigation("PBI_License_Request");
                });

            modelBuilder.Entity("NRCan.Datahub.Data.Projects.PBI_License_Request", b =>
                {
                    b.Navigation("User_License_Requests");
                });
#pragma warning restore 612, 618
        }
    }
}
