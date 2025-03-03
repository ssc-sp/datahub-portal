using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations
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
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinancialAuthorityCommitmentIsRef",
                table: "GCHostingWorkspaceDetails",
                type: "nvarchar(max)",
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
