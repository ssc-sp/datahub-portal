using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Core.Migrations.Core
{
    public partial class OpenDataUpdateFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approved",
                table: "OpenDataSharedFile");

            migrationBuilder.RenameColumn(
                name: "Read",
                table: "OpenDataSharedFile",
                newName: "Read_FLAG");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Read_FLAG",
                table: "OpenDataSharedFile",
                newName: "Read");

            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "OpenDataSharedFile",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
