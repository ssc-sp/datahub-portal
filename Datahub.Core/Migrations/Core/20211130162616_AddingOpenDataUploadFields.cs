using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Core.Migrations.Core
{
    public partial class AddingOpenDataUploadFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileSize_VAL",
                table: "OpenDataSharedFile");

            migrationBuilder.DropColumn(
                name: "Read_FLAG",
                table: "OpenDataSharedFile");

            migrationBuilder.AddColumn<string>(
                name: "UploadError_TXT",
                table: "OpenDataSharedFile",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UploadStatus_CD",
                table: "OpenDataSharedFile",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploadError_TXT",
                table: "OpenDataSharedFile");

            migrationBuilder.DropColumn(
                name: "UploadStatus_CD",
                table: "OpenDataSharedFile");

            migrationBuilder.AddColumn<long>(
                name: "FileSize_VAL",
                table: "OpenDataSharedFile",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "Read_FLAG",
                table: "OpenDataSharedFile",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
