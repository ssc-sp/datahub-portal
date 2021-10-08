using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Shared.Migrations.Core
{
    public partial class DataProjectDbColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DB_Name",
                table: "Projects",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DB_Server",
                table: "Projects",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DB_Type",
                table: "Projects",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CHK_DB_Type",
                table: "Projects",
                sql: "DB_Type in ('SQL Server', 'Postgres')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CHK_DB_Type",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DB_Name",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DB_Server",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DB_Type",
                table: "Projects");
        }
    }
}
