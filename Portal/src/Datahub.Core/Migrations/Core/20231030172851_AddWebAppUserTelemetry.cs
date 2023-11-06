using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    /// <inheritdoc />
    public partial class AddWebAppUserTelemetry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AzureWebAppUrl",
                table: "UserRecentLink",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AzureWebAppUrl",
                table: "UserRecentLink");
        }
    }
}
