﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Datahub.LanguageTraining.Data;

namespace Datahub.Portal.Migrations
{
    [DbContext(typeof(LanguageTrainingDBContext))]
    partial class LanguageTrainingDBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("Datahub.Portal.Data.LanguageTraining.LanguageTrainingApplication", b =>
                {
                    b.Property<int>("Application_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<bool>("ApplicationCompleteEmailSent")
                        .HasColumnType("bit");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Class_For_Language_Training")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Completed_LETP_Assessment")
                        .HasColumnType("bit");

                    b.Property<string>("Completed_Training_Session")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Completed_Training_Year")
                        .HasColumnType("int");

                    b.Property<string>("Decision")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Delegate_Manager_First_Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Delegated_Manager_Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Delegated_Manager_Last_Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email_Address_EMAIL")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Employee_Appointed_NonImperative_Basis")
                        .HasColumnType("bit");

                    b.Property<bool>("Employee_equity_group")
                        .HasColumnType("bit");

                    b.Property<bool>("Employee_language_profile_raised")
                        .HasColumnType("bit");

                    b.Property<bool>("Employee_professional_dev_program")
                        .HasColumnType("bit");

                    b.Property<bool>("Employee_talent_management_exercise")
                        .HasColumnType("bit");

                    b.Property<string>("Employment_Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("First_Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("I_am_seeking")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("LSUDecisionSent")
                        .HasColumnType("bit");

                    b.Property<string>("Language_Training_Provided_By")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Language_Training_Since_LETP_Assessment")
                        .HasColumnType("bit");

                    b.Property<string>("Last_Course_Successfully_Completed")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Last_Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Last_Updated_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Last_Updated_UserId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("ManagerDecisionSent")
                        .HasColumnType("bit");

                    b.Property<string>("Manager_Decision")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Manager_Email_Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Manager_First_Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Manager_Last_Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NRCan_Username")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Province_Territory")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Report_Sent_To_NRCan_Language_School")
                        .HasColumnType("bit");

                    b.Property<string>("SLE_Results_Oral")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SLE_Results_Reading")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SLE_Results_Writing")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("SLE_Test_Date")
                        .HasColumnType("datetime2");

                    b.Property<bool>("Second_Language_Evaluation_Results")
                        .HasColumnType("bit");

                    b.Property<string>("Sector_Branch")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Session_For_Language_Training")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("Training_Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Application_ID");

                    b.ToTable("LanguageTrainingApplications");
                });
#pragma warning restore 612, 618
        }
    }
}
