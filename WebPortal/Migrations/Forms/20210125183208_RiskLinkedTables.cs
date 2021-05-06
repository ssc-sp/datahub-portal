using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.ProjectForms.Migrations
{
    public partial class RiskLinkedTables : Migration
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

            //migrationBuilder.AlterColumn<int>(
            //    name: "Risks_ID",
            //    table: "Risks",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            //migrationBuilder.AlterColumn<int>(
            //    name: "IndicatorAndResult_ID",
            //    table: "IndicatorAndResults",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

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
                name: "IX_IndicatorRisks_Pip_IndicatorIndicatorAndResult_ID",
                table: "IndicatorRisks",
                column: "Pip_IndicatorIndicatorAndResult_ID");

            migrationBuilder.CreateIndex(
                name: "IX_IndicatorRisks_Pip_RiskRisks_ID",
                table: "IndicatorRisks",
                column: "Pip_RiskRisks_ID");

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

            migrationBuilder.AlterColumn<int>(
                name: "Tombstone_ID",
                table: "Tombstones",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "Risks_ID",
                table: "Risks",
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
