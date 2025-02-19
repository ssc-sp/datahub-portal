using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.SqliteDatahub
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
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionPeriodStartDate",
                table: "GCHostingWorkspaceDetails",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "RetentionValue",
                table: "GCHostingWorkspaceDetails",
                type: "TEXT",
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
