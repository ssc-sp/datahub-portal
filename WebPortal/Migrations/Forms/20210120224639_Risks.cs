using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.ProjectForms.Migrations
{
    public partial class Risks : Migration
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

            migrationBuilder.AddColumn<string>(
                name: "Last_Updated_UserId",
                table: "Tombstones",
                type: "nvarchar(max)",
                nullable: true);

            //migrationBuilder.AlterColumn<int>(
            //    name: "IndicatorAndResult_ID",
            //    table: "IndicatorAndResults",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Last_Updated_UserId",
                table: "IndicatorAndResults",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Risks",
                columns: table => new
                {
                    Risks_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Risk_Id_TXT = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Risk_Description_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Risk_Drivers_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Residual_Risk_Level_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Target_Risk_Level_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Risk_Trend_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Ongoing_Monitoring_Activities_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    Future_Mitigation_Activities_TXT = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "IX_Risks_PIP_TombstoneTombstone_ID",
                table: "Risks",
                column: "PIP_TombstoneTombstone_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Risks");

            migrationBuilder.DropColumn(
                name: "Last_Updated_UserId",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Last_Updated_UserId",
                table: "IndicatorAndResults");

            migrationBuilder.AlterColumn<int>(
                name: "Tombstone_ID",
                table: "Tombstones",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "IndicatorAndResult_ID",
                table: "IndicatorAndResults",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");
        }
    }
}
