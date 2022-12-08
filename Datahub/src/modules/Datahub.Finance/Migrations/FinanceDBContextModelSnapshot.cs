﻿// <auto-generated />
using System;
using Datahub.Finance.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Datahub.Finance.Migrations
{
    [DbContext(typeof(FinanceDBContext))]
    partial class FinanceDBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Datahub.Portal.Data.Finance.BranchAccess", b =>
                {
                    b.Property<int>("BranchAccess_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("BranchAccess_ID"), 1L, 1);

                    b.Property<string>("BranchFundCenter")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<bool>("IsInactive")
                        .HasColumnType("bit");

                    b.Property<string>("User")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("BranchAccess_ID");

                    b.ToTable("BranchAccess");
                });

            modelBuilder.Entity("Datahub.Portal.Data.Finance.FiscalYear", b =>
                {
                    b.Property<int>("YearId")
                        .HasColumnType("int");

                    b.Property<string>("Year")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("YearId");

                    b.ToTable("FiscalYears");
                });

            modelBuilder.Entity("Datahub.Portal.Data.Finance.Forecast", b =>
                {
                    b.Property<int>("Forecast_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Forecast_ID"), 1L, 1);

                    b.Property<string>("Classification")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<DateTime>("Created_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Created_UserId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Employee_First_Name")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Employee_Last_Name")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Employee_Planned_Staffing")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int?>("Employee_Position_Number")
                        .HasColumnType("int");

                    b.Property<DateTime>("End_Date")
                        .HasColumnType("datetime2");

                    b.Property<double?>("FTE")
                        .HasColumnType("float");

                    b.Property<int?>("FTE_Accomodations_Location")
                        .HasColumnType("int");

                    b.Property<int?>("FTE_Accomodations_Requirements")
                        .HasColumnType("int");

                    b.Property<string>("Fund")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<int?>("FundCenter_ID")
                        .HasColumnType("int");

                    b.Property<int?>("Incremental_Replacement")
                        .HasColumnType("int");

                    b.Property<bool>("Is_Deleted")
                        .HasColumnType("bit");

                    b.Property<bool>("Is_Indeterminate")
                        .HasColumnType("bit");

                    b.Property<DateTime>("Last_Updated_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Last_Updated_UserId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Location_Of_Hiring")
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<string>("Other_Locations")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int?>("Position_Workspace_Type")
                        .HasColumnType("int");

                    b.Property<int?>("Potential_Hiring_Process")
                        .HasColumnType("int");

                    b.Property<double?>("Salary")
                        .HasColumnType("float");

                    b.Property<DateTime>("Start_Date")
                        .HasColumnType("datetime2");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Forecast_ID");

                    b.HasIndex("FundCenter_ID");

                    b.ToTable("Forecasts");
                });

            modelBuilder.Entity("Datahub.Portal.Data.Finance.FundCenter", b =>
                {
                    b.Property<int>("FundCenter_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("FundCenter_ID"), 1L, 1);

                    b.Property<double?>("AttritionRate")
                        .HasColumnType("float");

                    b.Property<int>("BranchHierarchyLevelID")
                        .HasColumnType("int");

                    b.Property<DateTime>("Created_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Created_UserId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<int>("DivisionHierarchyLevelID")
                        .HasColumnType("int");

                    b.Property<int>("FiscalYearYearId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Last_Updated_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Last_Updated_UserId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SectorHierarchyLevelID")
                        .HasColumnType("int");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("FundCenter_ID");

                    b.HasIndex("BranchHierarchyLevelID");

                    b.HasIndex("DivisionHierarchyLevelID");

                    b.HasIndex("FiscalYearYearId");

                    b.HasIndex("SectorHierarchyLevelID");

                    b.ToTable("FundCenters");
                });

            modelBuilder.Entity("Datahub.Portal.Data.Finance.HierarchyLevel", b =>
                {
                    b.Property<int>("HierarchyLevelID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("HierarchyLevelID"), 1L, 1);

                    b.Property<DateTime>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("FCCode")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("FundCenterModifiedEnglish")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FundCenterModifiedFrench")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FundCenterNameEnglish")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FundCenterNameFrench")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<int>("Level")
                        .HasColumnType("int");

                    b.Property<string>("ParentCode")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("HierarchyLevelID");

                    b.ToTable("HierarchyLevels");
                });

            modelBuilder.Entity("Datahub.Portal.Data.Finance.SummaryForecast", b =>
                {
                    b.Property<int>("Forecast_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Forecast_ID"), 1L, 1);

                    b.Property<string>("AdditionalNotes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("Budget")
                        .HasColumnType("float");

                    b.Property<double?>("Contribution")
                        .HasColumnType("float");

                    b.Property<DateTime>("Created_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Created_UserId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("FTE_Sum")
                        .HasColumnType("float");

                    b.Property<string>("Fund")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<int?>("FundCenter_ID")
                        .HasColumnType("int");

                    b.Property<double?>("Grants")
                        .HasColumnType("float");

                    b.Property<bool>("Is_Deleted")
                        .HasColumnType("bit");

                    b.Property<int?>("Key_Activity")
                        .HasColumnType("int");

                    b.Property<string>("Key_Activity_Additional_Information")
                        .HasMaxLength(5000)
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Last_Updated_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Last_Updated_UserId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("Non_Personel")
                        .HasColumnType("float");

                    b.Property<double?>("Other_OnM")
                        .HasColumnType("float");

                    b.Property<double?>("Personel")
                        .HasColumnType("float");

                    b.Property<double?>("SFT_Forecast")
                        .HasColumnType("float");

                    b.Property<double?>("SFT_Forecast_Gross")
                        .HasColumnType("float");

                    b.Property<double?>("THC")
                        .HasColumnType("float");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Forecast_ID");

                    b.HasIndex("FundCenter_ID");

                    b.ToTable("SummaryForecasts");
                });

            modelBuilder.Entity("Datahub.Portal.Data.Finance.Forecast", b =>
                {
                    b.HasOne("Datahub.Portal.Data.Finance.FundCenter", "FundCenter")
                        .WithMany()
                        .HasForeignKey("FundCenter_ID");

                    b.Navigation("FundCenter");
                });

            modelBuilder.Entity("Datahub.Portal.Data.Finance.FundCenter", b =>
                {
                    b.HasOne("Datahub.Portal.Data.Finance.HierarchyLevel", "Branch")
                        .WithMany("BranchFundCenters")
                        .HasForeignKey("BranchHierarchyLevelID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Datahub.Portal.Data.Finance.HierarchyLevel", "Division")
                        .WithMany("DivisionFundCenters")
                        .HasForeignKey("DivisionHierarchyLevelID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Datahub.Portal.Data.Finance.FiscalYear", "FiscalYear")
                        .WithMany("FundCenters")
                        .HasForeignKey("FiscalYearYearId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Datahub.Portal.Data.Finance.HierarchyLevel", "Sector")
                        .WithMany("SectorFundCenters")
                        .HasForeignKey("SectorHierarchyLevelID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Branch");

                    b.Navigation("Division");

                    b.Navigation("FiscalYear");

                    b.Navigation("Sector");
                });

            modelBuilder.Entity("Datahub.Portal.Data.Finance.SummaryForecast", b =>
                {
                    b.HasOne("Datahub.Portal.Data.Finance.FundCenter", "FundCenter")
                        .WithMany()
                        .HasForeignKey("FundCenter_ID");

                    b.Navigation("FundCenter");
                });

            modelBuilder.Entity("Datahub.Portal.Data.Finance.FiscalYear", b =>
                {
                    b.Navigation("FundCenters");
                });

            modelBuilder.Entity("Datahub.Portal.Data.Finance.HierarchyLevel", b =>
                {
                    b.Navigation("BranchFundCenters");

                    b.Navigation("DivisionFundCenters");

                    b.Navigation("SectorFundCenters");
                });
#pragma warning restore 612, 618
        }
    }
}
