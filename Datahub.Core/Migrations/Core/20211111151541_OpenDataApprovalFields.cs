using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Core.Migrations.Core
{
    public partial class OpenDataApprovalFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "OpenDataSharedFile",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Read",
                table: "OpenDataSharedFile",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approved",
                table: "OpenDataSharedFile");

            migrationBuilder.DropColumn(
                name: "Read",
                table: "OpenDataSharedFile");
        }
    }
}
