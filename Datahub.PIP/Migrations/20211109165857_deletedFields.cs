using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Portal.Migrations.Forms.PIP
{
    public partial class deletedFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Risks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserIdWhoDeleted",
                table: "Risks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "IndicatorAndResults",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserIdWhoDeleted",
                table: "IndicatorAndResults",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "UserIdWhoDeleted",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "IndicatorAndResults");

            migrationBuilder.DropColumn(
                name: "UserIdWhoDeleted",
                table: "IndicatorAndResults");
        }
    }
}
