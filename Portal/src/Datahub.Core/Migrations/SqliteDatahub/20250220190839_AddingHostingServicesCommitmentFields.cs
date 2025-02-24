using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.SqliteDatahub
{
    /// <inheritdoc />
    public partial class AddingHostingServicesCommitmentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FinancialAuthorityCommitmentIsOrg",
                table: "GCHostingWorkspaceDetails",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinancialAuthorityCommitmentIsRef",
                table: "GCHostingWorkspaceDetails",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinancialAuthorityCommitmentIsOrg",
                table: "GCHostingWorkspaceDetails");

            migrationBuilder.DropColumn(
                name: "FinancialAuthorityCommitmentIsRef",
                table: "GCHostingWorkspaceDetails");
        }
    }
}
