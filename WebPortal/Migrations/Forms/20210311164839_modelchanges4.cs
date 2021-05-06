using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.ProjectForms.Migrations
{
    public partial class modelchanges4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AddColumn<string>(
                name: "Transfer_Payment_Programs_3_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Transfer_Payment_Programs_Less5_1_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Transfer_Payment_Programs_Less5_2_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Transfer_Payment_Programs_Less5_3_DESC",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);



        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Transfer_Payment_Programs_3_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Transfer_Payment_Programs_Less5_1_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Transfer_Payment_Programs_Less5_2_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Transfer_Payment_Programs_Less5_3_DESC",
                table: "Tombstones");

            migrationBuilder.AlterColumn<int>(
                name: "Tombstone_ID",
                table: "Tombstones",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "TombstoneRisk_ID",
                table: "TombstoneRisks",
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
                name: "IndicatorRisk_ID",
                table: "IndicatorRisks",
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
