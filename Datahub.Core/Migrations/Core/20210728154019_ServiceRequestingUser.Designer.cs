﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Datahub.Shared.EFCore;

namespace Datahub.Portal.Migrations.Forms.DatahubProjectDB
{
    [DbContext(typeof(DatahubProjectDBContext))]
    [Migration("20210728154019_ServiceRequestingUser")]
    partial class ServiceRequestingUser
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("Datahub.Shared.EFCore.Datahub_Project", b =>
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

                    b.Property<string>("Databricks_URL")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<DateTime?>("Deleted_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Division_Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GC_Docs_URL")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Initial_Meeting_DT")
                        .HasColumnType("datetime2");

                    b.Property<bool>("Is_Private")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("Last_Contact_DT")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("Last_Updated_DT")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Next_Meeting_DT")
                        .HasColumnType("datetime2");

                    b.Property<int?>("Number_Of_Users_Involved")
                        .HasColumnType("int");

                    b.Property<string>("PowerBI_URL")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Project_Acronym_CD")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("Project_Admin")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Project_Category")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Project_Icon")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Project_Name")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Project_Name_Fr")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Project_Phase")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Project_Status_Desc")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Project_Summary_Desc")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Project_Summary_Desc_Fr")
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

                    b.Property<string>("WebForms_URL")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.HasKey("Project_ID");

                    b.ToTable("Projects");

                    // b.HasData(
                    //     new
                    //     {
                    //         Project_ID = 1,
                    //         Initial_Meeting_DT = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                    //         Is_Private = false,
                    //         Last_Updated_DT = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                    //         Project_Acronym_CD = "DHTRK",
                    //         Project_Icon = "database",
                    //         Project_Name = "Datahub Projects",
                    //         Project_Status_Desc = "Ongoing",
                    //         Project_Summary_Desc = "Datahub Project Tracker",
                    //         Sector_Name = "CIOSB"
                    //     });
                });

            modelBuilder.Entity("Datahub.Shared.EFCore.Datahub_ProjectComment", b =>
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

            modelBuilder.Entity("Datahub.Shared.EFCore.Datahub_ProjectServiceRequests", b =>
                {
                    b.Property<int>("ServiceRequests_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<DateTime?>("Is_Completed")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Notification_Sent")
                        .HasColumnType("datetime2");

                    b.Property<int?>("Project_ID")
                        .HasColumnType("int");

                    b.Property<DateTime>("ServiceRequests_Date_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("ServiceType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("User_ID")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("User_Name")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.HasKey("ServiceRequests_ID");

                    b.HasIndex("Project_ID");

                    b.ToTable("Project_Requests");
                });

            modelBuilder.Entity("Datahub.Shared.EFCore.Datahub_Project_Access_Request", b =>
                {
                    b.Property<int>("Request_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<DateTime?>("Completion_DT")
                        .HasColumnType("datetime2");

                    b.Property<bool>("Databricks")
                        .HasColumnType("bit");

                    b.Property<bool>("PowerBI")
                        .HasColumnType("bit");

                    b.Property<int?>("Project_ID")
                        .HasColumnType("int");

                    b.Property<DateTime>("Request_DT")
                        .HasColumnType("datetime2");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("User_ID")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("User_Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<bool>("WebForms")
                        .HasColumnType("bit");

                    b.HasKey("Request_ID");

                    b.HasIndex("Project_ID");

                    b.ToTable("Access_Requests");
                });

            modelBuilder.Entity("Datahub.Shared.EFCore.Datahub_Project_Pipeline_Lnk", b =>
                {
                    b.Property<int>("Project_ID")
                        .HasColumnType("int");

                    b.Property<string>("Process_Nm")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Project_ID", "Process_Nm");

                    b.ToTable("Project_Pipeline_Links");
                });

            modelBuilder.Entity("Datahub.Shared.EFCore.Datahub_Project_User", b =>
                {
                    b.Property<int>("ProjectUser_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<bool?>("Databricks")
                        .HasColumnType("bit");

                    b.Property<bool?>("PowerBI")
                        .HasColumnType("bit");

                    b.Property<int?>("Project_ID")
                        .HasColumnType("int");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("User_ID")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("User_Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<bool?>("WebForms")
                        .HasColumnType("bit");

                    b.HasKey("ProjectUser_ID");

                    b.HasIndex("Project_ID");

                    b.ToTable("Project_Users");
                });

            modelBuilder.Entity("Datahub.Shared.EFCore.PBI_License_Request", b =>
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

            modelBuilder.Entity("Datahub.Shared.EFCore.PBI_User_License_Request", b =>
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

            modelBuilder.Entity("Datahub.Shared.EFCore.WebForm", b =>
                {
                    b.Property<int>("WebForm_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Description_DESC")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Project_ID")
                        .HasColumnType("int");

                    b.Property<string>("Title_DESC")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("WebForm_ID");

                    b.HasIndex("Project_ID");

                    b.ToTable("WebForms");
                });

            modelBuilder.Entity("Datahub.Shared.EFCore.WebForm_DBCodes", b =>
                {
                    b.Property<string>("DBCode")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("ClassWord_DEF")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClassWord_DESC")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("DBCode");

                    b.ToTable("DBCodes");
                });

            modelBuilder.Entity("Datahub.Shared.EFCore.WebForm_Field", b =>
                {
                    b.Property<int>("FieldID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<DateTime>("Date_Updated_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description_DESC")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Extension_CD")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(8)
                        .HasColumnType("nvarchar(8)")
                        .HasDefaultValue("NONE");

                    b.Property<string>("Field_DESC")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<bool>("Mandatory_FLAG")
                        .HasColumnType("bit");

                    b.Property<int?>("Max_Length_NUM")
                        .HasColumnType("int");

                    b.Property<string>("Notes_TXT")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Section_DESC")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Type_CD")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(8)
                        .HasColumnType("nvarchar(8)")
                        .HasDefaultValue("Text");

                    b.Property<int>("WebForm_ID")
                        .HasColumnType("int");

                    b.HasKey("FieldID");

                    b.HasIndex("WebForm_ID");

                    b.ToTable("Fields");
                });

            modelBuilder.Entity("Datahub.Shared.EFCore.Datahub_ProjectComment", b =>
                {
                    b.HasOne("Datahub.Shared.EFCore.Datahub_Project", "Project")
                        .WithMany("Comments")
                        .HasForeignKey("Project_ID");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Datahub.Shared.EFCore.Datahub_ProjectServiceRequests", b =>
                {
                    b.HasOne("Datahub.Shared.EFCore.Datahub_Project", "Project")
                        .WithMany()
                        .HasForeignKey("Project_ID");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Datahub.Shared.EFCore.Datahub_Project_Access_Request", b =>
                {
                    b.HasOne("Datahub.Shared.EFCore.Datahub_Project", "Project")
                        .WithMany("Requests")
                        .HasForeignKey("Project_ID");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Datahub.Shared.EFCore.Datahub_Project_Pipeline_Lnk", b =>
                {
                    b.HasOne("Datahub.Shared.EFCore.Datahub_Project", "Project")
                        .WithMany("Pipelines")
                        .HasForeignKey("Project_ID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Datahub.Shared.EFCore.Datahub_Project_User", b =>
                {
                    b.HasOne("Datahub.Shared.EFCore.Datahub_Project", "Project")
                        .WithMany("Users")
                        .HasForeignKey("Project_ID");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Datahub.Shared.EFCore.PBI_License_Request", b =>
                {
                    b.HasOne("Datahub.Shared.EFCore.Datahub_Project", "Project")
                        .WithOne("PBI_License_Request")
                        .HasForeignKey("Datahub.Shared.EFCore.PBI_License_Request", "Project_ID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Datahub.Shared.EFCore.PBI_User_License_Request", b =>
                {
                    b.HasOne("Datahub.Shared.EFCore.PBI_License_Request", "LicenseRequest")
                        .WithMany("User_License_Requests")
                        .HasForeignKey("RequestID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("LicenseRequest");
                });

            modelBuilder.Entity("Datahub.Shared.EFCore.WebForm", b =>
                {
                    b.HasOne("Datahub.Shared.EFCore.Datahub_Project", "Project")
                        .WithMany("WebForms")
                        .HasForeignKey("Project_ID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Datahub.Shared.EFCore.WebForm_Field", b =>
                {
                    b.HasOne("Datahub.Shared.EFCore.WebForm", "WebForm")
                        .WithMany("Fields")
                        .HasForeignKey("WebForm_ID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("WebForm");
                });

            modelBuilder.Entity("Datahub.Shared.EFCore.Datahub_Project", b =>
                {
                    b.Navigation("Comments");

                    b.Navigation("PBI_License_Request");

                    b.Navigation("Pipelines");

                    b.Navigation("Requests");

                    b.Navigation("Users");

                    b.Navigation("WebForms");
                });

            modelBuilder.Entity("Datahub.Shared.EFCore.PBI_License_Request", b =>
                {
                    b.Navigation("User_License_Requests");
                });

            modelBuilder.Entity("Datahub.Shared.EFCore.WebForm", b =>
                {
                    b.Navigation("Fields");
                });
#pragma warning restore 612, 618
        }
    }
}
