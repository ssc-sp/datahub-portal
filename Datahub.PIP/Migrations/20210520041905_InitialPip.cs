using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Portal.Migrations.Forms.PIP
{
    public partial class InitialPip : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tombstones",
                columns: table => new
                {
                    Tombstone_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Program_Title = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Lead_Sector = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Program_Official_Title = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Core_Responsbility_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Date_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Planned_Spending_AMTL = table.Column<decimal>(type: "Money", nullable: true),
                    Actual_Spending_AMTL = table.Column<decimal>(type: "Money", nullable: true),
                    Approval_By_Program_Offical_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Consultation_With_The_Head_Of_Evaluation_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Functional_SignOff_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Program_Inventory_Program_Description_URL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Program_Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Departmental_Result_1_CD = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Departmental_Result_2_CD = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Departmental_Result_3_CD = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Strategic_Priorities_1_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Strategic_Priorities_2_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Mandate_Letter_Commitment_1_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Mandate_Letter_Commitment_2_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Mandate_Letter_Commitment_3_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Mandate_Letter_Commitment_4_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Transfer_Payment_Programs_1_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Transfer_Payment_Programs_2_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Transfer_Payment_Programs_3_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Transfer_Payment_Programs_4_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Transfer_Payment_Programs_5_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Transfer_Payment_Programs_6_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Transfer_Payment_Programs_7_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Transfer_Payment_Programs_Less5_1_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Transfer_Payment_Programs_Less5_2_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Transfer_Payment_Programs_Less5_3_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Horizontal_Initiative_1_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Horizontal_Initiative_2_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Horizontal_Initiative_3_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Related_Program_Or_Activities = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Method_Of_Intervention_1_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Method_Of_Intervention_2_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Target_Group_1_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Target_Group_2_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Target_Group_3_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Target_Group_4_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Target_Group_5_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Government_Of_Canada_Activity_Tags_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Canadian_Classification_Of_Functions_Of_Government_DESC = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Last_Updated_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tombstones", x => x.Tombstone_ID);
                });

            migrationBuilder.CreateTable(
                name: "IndicatorAndResults",
                columns: table => new
                {
                    IndicatorAndResult_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DRF_Indicator_No = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Outcome_Level_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Program_Output_Or_Outcome_DESC = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Indicator_DESC = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Source_Of_Indicator_DESC = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Source_Of_Indicator2_DESC = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Source_Of_Indicator3_DESC = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Tb_Sub_Indicator_Identification_Number_ID = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Indicator_Category_DESC = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Indicator_Direction_DESC = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Indicator__Progressive_Or_Aggregate_DESC = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Indicator_Rationale_DESC = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Indicator_Calculation_Formula_NUM = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Measurement_Strategy = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Baseline_DESC = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Date_Of_Baseline_DT = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes_Definitions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Data_Owner_NAME = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Branch_Optional_DESC = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Sub_Program = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Data_Source_DESC = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Frequency_DESC = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Data_Type_DESC = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Target_Type_DESC = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Target_202021_DESC = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Date_To_Achieve_Target_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Target_Met = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Date_201920_Result_Collected_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Result_202021_DESC = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    Explanation = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    Result_201920_DESC = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Result_201819_DESC = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Result_201718_DESC = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Does_Indicator_Enable_Program_Measure_Equity = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    Methodology_How_Will_The_Indicator_Be_Measured = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    Is_Equity_Seeking_Group = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Is_Equity_Seeking_Group2 = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Is_Equity_Seeking_Group_Other = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    No_Equity_Seeking_Group = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    PIP_TombstoneTombstone_ID = table.Column<int>(type: "int", nullable: true),
                    Last_Updated_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndicatorAndResults", x => x.IndicatorAndResult_ID);
                    table.ForeignKey(
                        name: "FK_IndicatorAndResults_Tombstones_PIP_TombstoneTombstone_ID",
                        column: x => x.PIP_TombstoneTombstone_ID,
                        principalTable: "Tombstones",
                        principalColumn: "Tombstone_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Risks",
                columns: table => new
                {
                    Risks_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Risk_Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Risk_Description_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: false),
                    Risk_Id_TXT = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Risk_Category = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Risk_Trend_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Risk_Drivers_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: false),
                    Risk_Drivers_TXT2 = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Risk_Drivers_TXT3 = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Risk_Drivers_TXT4 = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Impact1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Likelihood1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Impact2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Likelihood2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Impact3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Likelihood3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ongoing_Monitoring_Activities_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: false),
                    Ongoing_Monitoring_Timeframe_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: false),
                    Ongoing_Monitoring_Activities_TXT2 = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Ongoing_Monitoring_Timeframe2_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Ongoing_Monitoring_Activities_TXT3 = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Ongoing_Monitoring_Timeframe3_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Future_Mitigation_Activities_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: false),
                    Strategy_Timeline1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Future_Mitigation_Activities_TXT2 = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Strategy_Timeline2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Future_Mitigation_Activities_TXT3 = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Strategy_Timeline3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Future_Mitigation_Activities_TXT4 = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Strategy_Timeline4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Future_Mitigation_Activities_TXT5 = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Strategy_Timeline5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Relevant_Corporate_Priorities_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Relevant_Corporate_Risks_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Comments_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    PIP_TombstoneTombstone_ID = table.Column<int>(type: "int", nullable: true),
                    Last_Updated_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Risks", x => x.Risks_ID);
                    table.ForeignKey(
                        name: "FK_Risks_Tombstones_PIP_TombstoneTombstone_ID",
                        column: x => x.PIP_TombstoneTombstone_ID,
                        principalTable: "Tombstones",
                        principalColumn: "Tombstone_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IndicatorRisks",
                columns: table => new
                {
                    IndicatorRisk_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Pip_IndicatorIndicatorAndResult_ID = table.Column<int>(type: "int", nullable: true),
                    Pip_RiskRisks_ID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndicatorRisks", x => x.IndicatorRisk_ID);
                    table.ForeignKey(
                        name: "FK_IndicatorRisks_IndicatorAndResults_Pip_IndicatorIndicatorAndResult_ID",
                        column: x => x.Pip_IndicatorIndicatorAndResult_ID,
                        principalTable: "IndicatorAndResults",
                        principalColumn: "IndicatorAndResult_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IndicatorRisks_Risks_Pip_RiskRisks_ID",
                        column: x => x.Pip_RiskRisks_ID,
                        principalTable: "Risks",
                        principalColumn: "Risks_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TombstoneRisks",
                columns: table => new
                {
                    TombstoneRisk_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Pip_TombstoneTombstone_ID = table.Column<int>(type: "int", nullable: true),
                    Pip_RiskRisks_ID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TombstoneRisks", x => x.TombstoneRisk_ID);
                    table.ForeignKey(
                        name: "FK_TombstoneRisks_Risks_Pip_RiskRisks_ID",
                        column: x => x.Pip_RiskRisks_ID,
                        principalTable: "Risks",
                        principalColumn: "Risks_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TombstoneRisks_Tombstones_Pip_TombstoneTombstone_ID",
                        column: x => x.Pip_TombstoneTombstone_ID,
                        principalTable: "Tombstones",
                        principalColumn: "Tombstone_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndicatorAndResults_PIP_TombstoneTombstone_ID",
                table: "IndicatorAndResults",
                column: "PIP_TombstoneTombstone_ID");

            migrationBuilder.CreateIndex(
                name: "IX_IndicatorRisks_Pip_IndicatorIndicatorAndResult_ID",
                table: "IndicatorRisks",
                column: "Pip_IndicatorIndicatorAndResult_ID");

            migrationBuilder.CreateIndex(
                name: "IX_IndicatorRisks_Pip_RiskRisks_ID",
                table: "IndicatorRisks",
                column: "Pip_RiskRisks_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Risks_PIP_TombstoneTombstone_ID",
                table: "Risks",
                column: "PIP_TombstoneTombstone_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TombstoneRisks_Pip_RiskRisks_ID",
                table: "TombstoneRisks",
                column: "Pip_RiskRisks_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TombstoneRisks_Pip_TombstoneTombstone_ID",
                table: "TombstoneRisks",
                column: "Pip_TombstoneTombstone_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndicatorRisks");

            migrationBuilder.DropTable(
                name: "TombstoneRisks");

            migrationBuilder.DropTable(
                name: "IndicatorAndResults");

            migrationBuilder.DropTable(
                name: "Risks");

            migrationBuilder.DropTable(
                name: "Tombstones");
        }
    }
}
