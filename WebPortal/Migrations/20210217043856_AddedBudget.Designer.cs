﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NRCan.Datahub.Portal.Data.Finance;

namespace NRCan.Datahub.Portal.Migrations
{
    [DbContext(typeof(FinanceDBContext))]
    [Migration("20210217043856_AddedBudget")]
    partial class AddedBudget
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("NRCan.Datahub.Portal.Data.Finance.Budget", b =>
                {
                    b.Property<int>("Budget_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<double?>("Adjustments_To_Forecast_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Allocation_Percentage_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Anticipated_Transfers_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Attrition_Percentage_PCT")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Budget_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<string>("Comments_Notes_For_Financial_Information_TXT")
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Comments_Notes_For_NonFinancial_Information_TXT")
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("Contributions_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<string>("Departmental_Priorities_TXT")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("Determinate_Fte_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Determinate__Amount_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Division_NUM")
                        .HasMaxLength(15)
                        .HasColumnType("float");

                    b.Property<double?>("Forecast_Adjustment_For_Risk_Management_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Forecast_Adjustment_For_Salary_Attrition_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<int?>("Fund_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("int");

                    b.Property<string>("Funding_Type_TXT")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<double?>("Grants_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Indeterminate_Fte_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Indeterminate__Amount_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Information_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Information_Three_Year_Average_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<string>("Involves_An_It_Or_Real_Property_Component_TXT")
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Key_Activity_TXT")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Last_Updated_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Last_Updated_UserId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("Machine_and_Equipment_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Machine_and_Equipment_Three_Year_Average_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("NonPersonnel_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Other_Payments_and_Ogd_Recoveries_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Other_Payments_and_Ogd_Recoveries_Three_Year_Average_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Percent_Of_Forecast_To_Budget_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Personnel_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Planned_Staffing_Fte_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Planned_Staffing__Amount_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Professional_SeervicesThree_Year_Average_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Professional_Seervices_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<string>("Program_Activity_TXT")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<double?>("Rentals_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Rentals_Three_Year_Average_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Repairs_and_MaintenanceThree_Year_Average_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Repairs_and_Maintenance_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Revised_Budget_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Revised_Forecasts_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<int?>("SectorAndBranchSectorBranch_ID")
                        .HasColumnType("int");

                    b.Property<string>("Sector_Priorities_TXT")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SubLevel_TXT")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<double?>("Total_Capital_Forecast_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Total_Forecasted_Expenditures_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Total_GnC_Forecast_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Total_OnM_Forecast_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Total_Salary_Fte_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Total_Salary__Amount_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Transportation_and_Communication_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Transportation_and_Communication_Three_Year_Average_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Utilities_Materials_and_Supplies_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.Property<double?>("Utilities_Materials_and_Supplies_Three_Year_Average_NUM")
                        .HasMaxLength(20)
                        .HasColumnType("float");

                    b.HasKey("Budget_ID");

                    b.HasIndex("SectorAndBranchSectorBranch_ID");

                    b.ToTable("Budgets");
                });

            modelBuilder.Entity("NRCan.Datahub.Portal.Data.Finance.SectorAndBranch", b =>
                {
                    b.Property<int>("SectorBranch_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<decimal?>("Allocated_Budget_NUM")
                        .HasColumnType("Money");

                    b.Property<decimal?>("Branch_Budget_NUM")
                        .HasColumnType("Money");

                    b.Property<string>("Branch_TXT")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Last_Updated_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Last_Updated_UserId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Sector_TXT")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<decimal?>("Unallocated_Budget_NUM")
                        .HasColumnType("Money");

                    b.HasKey("SectorBranch_ID");

                    b.ToTable("SectorAndBranches");
                });

            modelBuilder.Entity("NRCan.Datahub.Portal.Data.Finance.Budget", b =>
                {
                    b.HasOne("NRCan.Datahub.Portal.Data.Finance.SectorAndBranch", "SectorAndBranch")
                        .WithMany()
                        .HasForeignKey("SectorAndBranchSectorBranch_ID");

                    b.Navigation("SectorAndBranch");
                });
#pragma warning restore 612, 618
        }
    }
}
