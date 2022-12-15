using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Metadata.Migrations
{
    public partial class AddingKeywordSourceAndFrequency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Frequency",
                table: "Keywords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Keywords",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "Keywords");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Keywords");
        }
    }
}
