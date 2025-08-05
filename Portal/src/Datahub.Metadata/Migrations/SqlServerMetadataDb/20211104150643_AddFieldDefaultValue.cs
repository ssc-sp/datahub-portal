using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Metadata.Migrations
{
    public partial class AddFieldDefaultValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Default_Value_TXT",
                table: "FieldDefinitions",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Default_Value_TXT",
                table: "FieldDefinitions");
        }
    }
}
