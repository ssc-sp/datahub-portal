using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Core.Migrations.Core
{
    public partial class AddingSharedFileSizeField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ApprovalFormRead_FLAG",
                table: "OpenDataSharedFile",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "FileSize_VAL",
                table: "OpenDataSharedFile",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalFormRead_FLAG",
                table: "OpenDataSharedFile");

            migrationBuilder.DropColumn(
                name: "FileSize_VAL",
                table: "OpenDataSharedFile");
        }
    }
}
