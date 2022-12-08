using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    public partial class AddingOpenDataApprovalFormEdit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalFormEdited_FLAG",
                table: "SharedDataFiles");

            migrationBuilder.AddColumn<bool>(
                name: "ApprovalFormEdited_FLAG",
                table: "OpenDataSharedFile",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalFormEdited_FLAG",
                table: "OpenDataSharedFile");

            migrationBuilder.AddColumn<bool>(
                name: "ApprovalFormEdited_FLAG",
                table: "SharedDataFiles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
