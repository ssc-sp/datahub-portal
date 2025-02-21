using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingHostingServicesFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FinancialAuthorityEmail",
                table: "GCHostingWorkspaceDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionPeriodStartDate",
                table: "GCHostingWorkspaceDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "RetentionValue",
                table: "GCHostingWorkspaceDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinancialAuthorityEmail",
                table: "GCHostingWorkspaceDetails");

            migrationBuilder.DropColumn(
                name: "RetentionPeriodStartDate",
                table: "GCHostingWorkspaceDetails");

            migrationBuilder.DropColumn(
                name: "RetentionValue",
                table: "GCHostingWorkspaceDetails");
        }
    }
}
