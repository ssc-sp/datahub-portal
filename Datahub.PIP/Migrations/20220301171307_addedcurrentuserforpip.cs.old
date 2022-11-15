using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.PIP
{
    public partial class addedcurrentuserforpip : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EditingUserId",
                table: "Tombstones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EditingUserId",
                table: "Risks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EditingUserId",
                table: "IndicatorAndResults",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditingUserId",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "EditingUserId",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "EditingUserId",
                table: "IndicatorAndResults");
        }
    }
}
