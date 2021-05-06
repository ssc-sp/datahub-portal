using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.ProjectForms.Migrations
{
    public partial class FixSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Core_Responsbility_1_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Core_Responsbility_2_DESC",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Core_Responsbility_3_DESC",
                table: "Tombstones");

            //migrationBuilder.AlterColumn<int>(
            //    name: "Tombstone_ID",
            //    table: "Tombstones",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Core_Responsbility_DESC",
                table: "Tombstones",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Core_Responsbility_DESC",
                table: "Tombstones");

            migrationBuilder.AlterColumn<int>(
                name: "Tombstone_ID",
                table: "Tombstones",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Core_Responsbility_1_DESC",
                table: "Tombstones",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Core_Responsbility_2_DESC",
                table: "Tombstones",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Core_Responsbility_3_DESC",
                table: "Tombstones",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
