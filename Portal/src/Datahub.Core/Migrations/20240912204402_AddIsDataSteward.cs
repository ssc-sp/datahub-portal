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

            // add new role
            migrationBuilder.InsertData(
                table: "Project_Roles",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 6, "Responsible for controlling the quality of data and ensuring data policies are followed", "Data Steward" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove new column
            migrationBuilder.DropColumn(
                name: "IsDataSteward",
                table: "Project_Users");

            // remove role
            migrationBuilder.DeleteData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
