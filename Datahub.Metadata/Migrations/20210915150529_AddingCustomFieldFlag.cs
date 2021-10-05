using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Metadata.Migrations
{
    public partial class AddingCustomFieldFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Custom_Field_FLAG",
                table: "FieldDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Custom_Field_FLAG",
                table: "FieldDefinitions");
        }
    }
}
