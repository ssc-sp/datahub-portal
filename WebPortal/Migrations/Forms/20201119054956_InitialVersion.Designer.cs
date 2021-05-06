﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NRCan.Datahub.ProjectForms.Data.PIP;

namespace NRCan.Datahub.ProjectForms.Migrations
{
    [DbContext(typeof(PIPDBContext))]
    [Migration("20201119054956_InitialVersion")]
    partial class InitialVersion
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.0-rc.2.20475.6");

            modelBuilder.Entity("NRCan.Datahub.ProjectForms.Data.PIP.PIP_Tombstone", b =>
                {
                    b.Property<int>("Tombstone_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<decimal?>("Actual_Spending_AMTL")
                        .HasColumnType("Money");

                    b.Property<DateTime?>("Approval_By_Program_Offical_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Canadian_Classification_Of_Functions_Of_Government_DESC")
                        .IsRequired()
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<DateTime?>("Consultation_With_The_Head_Of_Evaluation_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Core_Responsbility_1_DESC")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Core_Responsbility_2_DESC")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Core_Responsbility_3_DESC")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("Date_Updated_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Departmental_Result_1_CD")
                        .IsRequired()
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Departmental_Result_2_CD")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<DateTime?>("Functional_SignOff_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Government_Of_Canada_Activity_Tags_DESC")
                        .IsRequired()
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Government_Of_Canada_Outcome_Areas_1_DESC")
                        .IsRequired()
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Government_Of_Canada_Outcome_Areas_2_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Horizontal_Initiative_1_DESC")
                        .IsRequired()
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Horizontal_Initiative_2_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Intermediate_Outcome")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Internal_Services_DESC")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("Last_Updated_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Mandate_Letter_Commitment_1_DESC")
                        .IsRequired()
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Mandate_Letter_Commitment_2_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Method_Of_Intervention_1_DESC")
                        .IsRequired()
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Method_Of_Intervention_2_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<decimal?>("Planned_Spending_AMTL")
                        .HasColumnType("Money");

                    b.Property<string>("Program_Inventory_Program_Description_URL")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Program_Official_Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Program_Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Strategic_Priorities_1_DESC")
                        .IsRequired()
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Strategic_Priorities_2_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Target_Group_1_DESC")
                        .IsRequired()
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Target_Group_2_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Target_Group_3_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Target_Group_4_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Target_Group_5_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("Transfer_Payment_Programs_1_DESC")
                        .IsRequired()
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Transfer_Payment_Programs_2_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Ultimate_Outcome_1_DESC")
                        .IsRequired()
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Ultimate_Outcome_2_DESC")
                        .IsRequired()
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.HasKey("Tombstone_ID");

                    b.ToTable("Tombstones");
                });
#pragma warning restore 612, 618
        }
    }
}
