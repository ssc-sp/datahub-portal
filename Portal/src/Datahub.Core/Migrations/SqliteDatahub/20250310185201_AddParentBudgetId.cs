using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.SqliteDatahub
{
    /// <inheritdoc />
    public partial class AddParentBudgetId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentGCHostingBudgetId",
                table: "Projects",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ParentGCHostingBudgetId",
                table: "Projects",
                column: "ParentGCHostingBudgetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_GCHostingWorkspaceDetails_ParentGCHostingBudgetId",
                table: "Projects",
                column: "ParentGCHostingBudgetId",
                principalTable: "GCHostingWorkspaceDetails",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_GCHostingWorkspaceDetails_ParentGCHostingBudgetId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ParentGCHostingBudgetId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ParentGCHostingBudgetId",
                table: "Projects");
        }
    }
}
