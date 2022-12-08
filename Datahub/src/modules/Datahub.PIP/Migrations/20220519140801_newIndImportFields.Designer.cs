// <auto-generated />
using System;
using Datahub.PIP.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.PIP
{
    [DbContext(typeof(PIPDBContext))]
    [Migration("20220519140801_newIndImportFields")]
    partial class NewIndImportFields
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Datahub.Portal.Data.PIP.PIP_FiscalYears", b =>
                {
                    b.Property<int>("YearId")
                        .HasColumnType("int");

                    b.Property<string>("FiscalYear")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("YearId");

                    b.ToTable("FiscalYears");

                    b.HasData(
                        new
                        {
                            YearId = 2018,
                            FiscalYear = "2017-18"
                        },
                        new
                        {
                            YearId = 2019,
                            FiscalYear = "2018-19"
                        },
                        new
                        {
                            YearId = 2020,
                            FiscalYear = "2019-20"
                        },
                        new
                        {
                            YearId = 2021,
                            FiscalYear = "2020-21"
                        },
                        new
                        {
                            YearId = 2022,
                            FiscalYear = "2021-22"
                        });
                });

            modelBuilder.Entity("Datahub.Portal.Data.PIP.PIP_IndicatorAndResults", b =>
                {
                    b.Property<int>("IndicatorAndResult_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IndicatorAndResult_ID"), 1L, 1);

                    b.Property<string>("Baseline_DESC")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("Branch_Optional_DESC")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("Can_Report_On_Indicator")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Cannot_Report_On_Indicator")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("DRF_Indicator_No")
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<string>("DataFactoryRunId")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("Data_Owner_NAME")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("Data_Source_DESC")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("Data_Type_DESC")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("Date_Of_Baseline_DT")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime?>("Date_Result_Collected")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Date_To_Achieve_Target_DT")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("Date_Updated_DT")
                        .HasColumnType("datetime2");

                    b.Property<int>("DuplicateCount")
                        .HasColumnType("int");

                    b.Property<string>("EditingUserId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Explanation")
                        .HasMaxLength(8000)
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("FiscalYearId")
                        .HasColumnType("int");

                    b.Property<string>("Frequency_DESC")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("IndicatorCode")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Indicator_Calculation_Formula_NUM")
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<string>("Indicator_Category_DESC")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("Indicator_DESC")
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<string>("Indicator_Direction_DESC")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("Indicator_Rationale_DESC")
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<string>("Indicator_Status")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<bool>("IsActualResultsLocked")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<bool>("IsFiscalYearLocked")
                        .HasColumnType("bit");

                    b.Property<bool>("IsIndicatorDetailsLocked")
                        .HasColumnType("bit");

                    b.Property<bool>("IsIndicatorStatusLocked")
                        .HasColumnType("bit");

                    b.Property<bool>("IsLatestUpdateLocked")
                        .HasColumnType("bit");

                    b.Property<bool>("IsMethodologyLocked")
                        .HasColumnType("bit");

                    b.Property<bool>("IsTargetLocked")
                        .HasColumnType("bit");

                    b.Property<DateTime>("Last_Updated_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Last_Updated_UserId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Measurement_Strategy")
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<string>("Methodology_How_Will_The_Indicator_Be_Measured")
                        .HasMaxLength(8000)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Notes_Definitions")
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<string>("Outcome_Level_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<int?>("PIP_TombstoneTombstone_ID")
                        .HasColumnType("int");

                    b.Property<string>("Program_Output_Or_Outcome_DESC")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("Result_DESC")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("SourceFileName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("SourceFileUploadDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Source_Of_Indicator2_DESC")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("Source_Of_Indicator3_DESC")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("Source_Of_Indicator_DESC")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("Sub_Program")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("Target_DESC")
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<string>("Target_Met")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("Target_Type_DESC")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("Trend_Rationale")
                        .HasMaxLength(8000)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserIdWhoDeleted")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IndicatorAndResult_ID");

                    b.HasIndex("FiscalYearId");

                    b.HasIndex("PIP_TombstoneTombstone_ID");

                    b.ToTable("IndicatorAndResults");
                });

            modelBuilder.Entity("Datahub.Portal.Data.PIP.PIP_IndicatorRisks", b =>
                {
                    b.Property<int>("IndicatorRisk_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IndicatorRisk_ID"), 1L, 1);

                    b.Property<int?>("Pip_IndicatorIndicatorAndResult_ID")
                        .HasColumnType("int");

                    b.Property<int?>("Pip_RiskRisks_ID")
                        .HasColumnType("int");

                    b.HasKey("IndicatorRisk_ID");

                    b.HasIndex("Pip_IndicatorIndicatorAndResult_ID");

                    b.HasIndex("Pip_RiskRisks_ID");

                    b.ToTable("IndicatorRisks");
                });

            modelBuilder.Entity("Datahub.Portal.Data.PIP.PIP_Risks", b =>
                {
                    b.Property<int>("Risks_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Risks_ID"), 1L, 1);

                    b.Property<string>("Comments_TXT")
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Date_Updated_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("EditingUserId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("FiscalYearId")
                        .HasColumnType("int");

                    b.Property<string>("Future_Mitigation_Activities_TXT")
                        .IsRequired()
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Future_Mitigation_Activities_TXT2")
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Future_Mitigation_Activities_TXT3")
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Future_Mitigation_Activities_TXT4")
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Future_Mitigation_Activities_TXT5")
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Impact1")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Impact2")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Impact3")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime>("Last_Updated_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Last_Updated_UserId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Likelihood1")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Likelihood2")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Likelihood3")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Ongoing_Monitoring_Activities_TXT")
                        .IsRequired()
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Ongoing_Monitoring_Activities_TXT2")
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Ongoing_Monitoring_Activities_TXT3")
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Ongoing_Monitoring_Timeframe2_TXT")
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Ongoing_Monitoring_Timeframe3_TXT")
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Ongoing_Monitoring_Timeframe_TXT")
                        .IsRequired()
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("PIP_TombstoneTombstone_ID")
                        .HasColumnType("int");

                    b.Property<string>("Relevant_Corporate_Priorities_TXT")
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Relevant_Corporate_Risks_TXT")
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RiskCode")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Risk_Category")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Risk_Description_TXT")
                        .IsRequired()
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Risk_Drivers_TXT")
                        .IsRequired()
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Risk_Drivers_TXT2")
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Risk_Drivers_TXT3")
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Risk_Drivers_TXT4")
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Risk_Id_TXT")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Risk_Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Risk_Trend_TXT")
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Strategy_Timeline1")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Strategy_Timeline2")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Strategy_Timeline3")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Strategy_Timeline4")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Strategy_Timeline5")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("UserIdWhoDeleted")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Year")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Risks_ID");

                    b.HasIndex("FiscalYearId");

                    b.HasIndex("PIP_TombstoneTombstone_ID");

                    b.ToTable("Risks");
                });

            modelBuilder.Entity("Datahub.Portal.Data.PIP.PIP_Tombstone", b =>
                {
                    b.Property<int>("Tombstone_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Tombstone_ID"), 1L, 1);

                    b.Property<decimal?>("Actual_Spending_AMTL")
                        .HasColumnType("Money");

                    b.Property<DateTime?>("Approval_By_Program_Offical_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Canadian_Classification_Of_Functions_Of_Government_DESC")
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<string>("Collecting_Data")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<DateTime?>("Consultation_With_The_Head_Of_Evaluation_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Core_Responsbility_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<DateTime>("Date_Updated_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Departmental_Result_1_CD")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Departmental_Result_2_CD")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Departmental_Result_3_CD")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Disaggregated_Data")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("Disaggregated_Data_Information")
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<string>("Does_Indicator_Enable_Program_Measure_Equity")
                        .HasMaxLength(8000)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Does_Indicator_Enable_Program_Measure_Equity_Option")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("EditingUserId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("FiscalYearId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("Functional_SignOff_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Government_Of_Canada_Activity_Tags_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Horizontal_Initiative_1_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Horizontal_Initiative_2_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Horizontal_Initiative_3_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<bool>("IsDateOfPipApprovalLocked")
                        .HasColumnType("bit");

                    b.Property<bool>("IsGBALocked")
                        .HasColumnType("bit");

                    b.Property<bool>("IsGCInfoBaseProgramTagsLocked")
                        .HasColumnType("bit");

                    b.Property<bool>("IsLatestUpdateInformationLocked")
                        .HasColumnType("bit");

                    b.Property<bool>("IsProgramInformationLocked")
                        .HasColumnType("bit");

                    b.Property<bool>("IsProgramNotesLocked")
                        .HasColumnType("bit");

                    b.Property<bool>("IsSectorProgramTagsLocked")
                        .HasColumnType("bit");

                    b.Property<bool>("IsSpendingLocked")
                        .HasColumnType("bit");

                    b.Property<string>("Is_Equity_Seeking_Group")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("Is_Equity_Seeking_Group2")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("Is_Equity_Seeking_Group_Other")
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<DateTime>("Last_Updated_DT")
                        .HasColumnType("datetime2");

                    b.Property<string>("Last_Updated_UserId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Lead_Sector")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Logic_Model")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Mandate_Letter_Commitment_1_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Mandate_Letter_Commitment_2_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Mandate_Letter_Commitment_3_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Mandate_Letter_Commitment_4_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Method_Of_Intervention_1_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Method_Of_Intervention_2_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("No_Equity_Seeking_Group")
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<decimal?>("Planned_Spending_AMTL")
                        .HasColumnType("Money");

                    b.Property<string>("ProgramCode")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Program_Inventory_Program_Description_URL")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Program_Notes")
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<string>("Program_Official_Title")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Program_Title")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Related_Program_Or_Activities")
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<string>("Strategic_Priorities_1_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Strategic_Priorities_2_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Target_Group_1_DESC")
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
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Transfer_Payment_Programs_2_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Transfer_Payment_Programs_3_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Transfer_Payment_Programs_4_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Transfer_Payment_Programs_5_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Transfer_Payment_Programs_6_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Transfer_Payment_Programs_7_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Transfer_Payment_Programs_8_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Transfer_Payment_Programs_Less5_1_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Transfer_Payment_Programs_Less5_2_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<string>("Transfer_Payment_Programs_Less5_3_DESC")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.HasKey("Tombstone_ID");

                    b.HasIndex("FiscalYearId");

                    b.ToTable("Tombstones");
                });

            modelBuilder.Entity("Datahub.Portal.Data.PIP.PIP_TombstoneRisks", b =>
                {
                    b.Property<int>("TombstoneRisk_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TombstoneRisk_ID"), 1L, 1);

                    b.Property<int?>("Pip_RiskRisks_ID")
                        .HasColumnType("int");

                    b.Property<int?>("Pip_TombstoneTombstone_ID")
                        .HasColumnType("int");

                    b.HasKey("TombstoneRisk_ID");

                    b.HasIndex("Pip_RiskRisks_ID");

                    b.HasIndex("Pip_TombstoneTombstone_ID");

                    b.ToTable("TombstoneRisks");
                });

            modelBuilder.Entity("Datahub.Portal.Data.PIP.PIP_IndicatorAndResults", b =>
                {
                    b.HasOne("Datahub.Portal.Data.PIP.PIP_FiscalYears", "FiscalYear")
                        .WithMany()
                        .HasForeignKey("FiscalYearId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Datahub.Portal.Data.PIP.PIP_Tombstone", "PIP_Tombstone")
                        .WithMany()
                        .HasForeignKey("PIP_TombstoneTombstone_ID");

                    b.Navigation("FiscalYear");

                    b.Navigation("PIP_Tombstone");
                });

            modelBuilder.Entity("Datahub.Portal.Data.PIP.PIP_IndicatorRisks", b =>
                {
                    b.HasOne("Datahub.Portal.Data.PIP.PIP_IndicatorAndResults", "Pip_Indicator")
                        .WithMany()
                        .HasForeignKey("Pip_IndicatorIndicatorAndResult_ID");

                    b.HasOne("Datahub.Portal.Data.PIP.PIP_Risks", "Pip_Risk")
                        .WithMany("PIP_IndicatorRisks")
                        .HasForeignKey("Pip_RiskRisks_ID");

                    b.Navigation("Pip_Indicator");

                    b.Navigation("Pip_Risk");
                });

            modelBuilder.Entity("Datahub.Portal.Data.PIP.PIP_Risks", b =>
                {
                    b.HasOne("Datahub.Portal.Data.PIP.PIP_FiscalYears", "FiscalYear")
                        .WithMany()
                        .HasForeignKey("FiscalYearId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Datahub.Portal.Data.PIP.PIP_Tombstone", "PIP_Tombstone")
                        .WithMany()
                        .HasForeignKey("PIP_TombstoneTombstone_ID");

                    b.Navigation("FiscalYear");

                    b.Navigation("PIP_Tombstone");
                });

            modelBuilder.Entity("Datahub.Portal.Data.PIP.PIP_Tombstone", b =>
                {
                    b.HasOne("Datahub.Portal.Data.PIP.PIP_FiscalYears", "FiscalYear")
                        .WithMany()
                        .HasForeignKey("FiscalYearId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FiscalYear");
                });

            modelBuilder.Entity("Datahub.Portal.Data.PIP.PIP_TombstoneRisks", b =>
                {
                    b.HasOne("Datahub.Portal.Data.PIP.PIP_Risks", "Pip_Risk")
                        .WithMany()
                        .HasForeignKey("Pip_RiskRisks_ID");

                    b.HasOne("Datahub.Portal.Data.PIP.PIP_Tombstone", "Pip_Tombstone")
                        .WithMany()
                        .HasForeignKey("Pip_TombstoneTombstone_ID");

                    b.Navigation("Pip_Risk");

                    b.Navigation("Pip_Tombstone");
                });

            modelBuilder.Entity("Datahub.Portal.Data.PIP.PIP_Risks", b =>
                {
                    b.Navigation("PIP_IndicatorRisks");
                });
#pragma warning restore 612, 618
        }
    }
}
