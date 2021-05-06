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
    [Migration("20201224011834_ProjectUser2")]
    partial class ProjectUser2
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.1");

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

                    b.Property<DateTime?>("Last_Contact_DT")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("Last_Updated_DT")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Next_Meeting_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("PowerBI_URL")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

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

                    b.Property<string>("WebForms_URL")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

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

            modelBuilder.Entity("NRCan.Datahub.Data.Projects.Datahub_Project_User", b =>
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

            modelBuilder.Entity("NRCan.Datahub.Data.Projects.WebForm", b =>
                {
                    b.Property<int>("WebForm_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Project_Name")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Title_DESC")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("WebForm_ID");

                    b.ToTable("WebForms");
                });

            modelBuilder.Entity("NRCan.Datahub.Data.Projects.WebForm_DBCodes", b =>
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

            modelBuilder.Entity("NRCan.Datahub.Data.Projects.WebForm_Field", b =>
                {
                    b.Property<int>("FieldID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Class")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("Date_Updated_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Field_DESC")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<bool>("Mandatory_FLAG")
                        .HasMaxLength(100)
                        .HasColumnType("bit");

                    b.Property<string>("Max_Length_NUM")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Section_DESC")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int?>("WebForm_ID")
                        .HasColumnType("int");

                    b.HasKey("FieldID");

                    b.HasIndex("WebForm_ID");

                    b.ToTable("Fields");
                });

            modelBuilder.Entity("NRCan.Datahub.Data.Projects.Datahub_ProjectComment", b =>
                {
                    b.HasOne("NRCan.Datahub.Data.Projects.Datahub_Project", "Project")
                        .WithMany("Comments")
                        .HasForeignKey("Project_ID");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("NRCan.Datahub.Data.Projects.Datahub_Project_User", b =>
                {
                    b.HasOne("NRCan.Datahub.Data.Projects.Datahub_Project", "Project")
                        .WithMany()
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

            modelBuilder.Entity("NRCan.Datahub.Data.Projects.WebForm_Field", b =>
                {
                    b.HasOne("NRCan.Datahub.Data.Projects.WebForm", "WebForm")
                        .WithMany("Fields")
                        .HasForeignKey("WebForm_ID");

                    b.Navigation("WebForm");
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

            modelBuilder.Entity("NRCan.Datahub.Data.Projects.WebForm", b =>
                {
                    b.Navigation("Fields");
                });
#pragma warning restore 612, 618
        }
    }
}
