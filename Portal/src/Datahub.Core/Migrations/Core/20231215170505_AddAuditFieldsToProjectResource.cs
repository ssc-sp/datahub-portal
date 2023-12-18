using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    /// <inheritdoc />
    public partial class AddAuditFieldsToProjectResource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimeRequested",
                table: "Project_Resources2",
                newName: "RequestedAt");

            migrationBuilder.RenameColumn(
                name: "TimeCreated",
                table: "Project_Resources2",
                newName: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequestedAt",
                table: "Project_Resources2",
                newName: "TimeRequested");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Project_Resources2",
                newName: "TimeCreated");
        }
    }
}
