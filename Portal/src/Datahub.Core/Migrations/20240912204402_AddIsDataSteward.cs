using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDataSteward : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new column
            migrationBuilder.AddColumn<bool>(
                name: "IsDataSteward",
                table: "Project_Users",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove new column
            migrationBuilder.DropColumn(
                name: "IsDataSteward",
                table: "Project_Users");
        }
    }
}
