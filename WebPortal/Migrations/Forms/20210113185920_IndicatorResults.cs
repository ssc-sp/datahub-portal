using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.ProjectForms.Migrations
{
    public partial class IndicatorResults : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<int>(
            //    name: "Tombstone_ID",
            //    table: "Tombstones",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.CreateTable(
                name: "IndicatorAndResults",
                columns: table => new
                {
                    IndicatorAndResult_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Outcome_Level_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Program_Output_Or_Outcome_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Indicator_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Indicator_Calculation_Formula_NUM = table.Column<double>(type: "float", maxLength: 100, nullable: false),
                    Indicator_Rationale_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Source_Of_Indicator_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Tb_Sub_Indicator_Identification_Number_ID = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Indicator_Category_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Indicator_Direction_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Indicator__Progressive_Or_Aggregate_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Branch_Optional_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Data_Owner_NAME = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Data_Source_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Frequency_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Data_Type_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Baseline_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Date_Of_Baseline_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Target_Value_Minimum_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Target_Value__Maximum_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Target_Value__Exact_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Target_Type_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Target_202021_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Date_To_Achieve_Target_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Date_201920_Result_Collected_DT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Result_201920_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Result_201819_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Result_201718_DESC = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Does_This_Indicator_Support_Gba = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    If_Yes_Please_Provide_An_Explanation_Of_How = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Methodology_How_Will_The_Indicator_Be_Measured = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    PIP_TombstoneTombstone_ID = table.Column<int>(type: "int", nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "IX_IndicatorAndResults_PIP_TombstoneTombstone_ID",
                table: "IndicatorAndResults",
                column: "PIP_TombstoneTombstone_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndicatorAndResults");

            migrationBuilder.AlterColumn<int>(
                name: "Tombstone_ID",
                table: "Tombstones",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");
        }
    }
}
