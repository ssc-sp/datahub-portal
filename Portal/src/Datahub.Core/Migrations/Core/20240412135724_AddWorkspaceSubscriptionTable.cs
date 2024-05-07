using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    /// <inheritdoc />
    public partial class AddWorkspaceSubscriptionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscriptionId",
                table: "Projects");

            migrationBuilder.AddColumn<int>(
                name: "DatahubAzureSubscriptionId",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AzureSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    SubscriptionId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_AzureSubscriptions", x => x.Id); });

            // add the original subscription to the AzureSubscriptions table
            migrationBuilder.Sql(@"
             SET IDENTITY_INSERT AzureSubscriptions ON;
             INSERT INTO AzureSubscriptions(Id, TenantId, SubscriptionId, Name)
             VALUES(1, '8c1a4d93-d828-4d0e-9303-fd3bd611c822', 'bc4bcb08-d617-49f4-b6af-69d6f10c240b', 'Proof of Concept 1');
             SET IDENTITY_INSERT AzureSubscriptions ON;
            ");

            // associate the original subscription with all projects
            migrationBuilder.Sql(@"
             UPDATE Projects
             SET DatahubAzureSubscriptionId = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_DatahubAzureSubscriptionId",
                table: "Projects",
                column: "DatahubAzureSubscriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_AzureSubscriptions_DatahubAzureSubscriptionId",
                table: "Projects",
                column: "DatahubAzureSubscriptionId",
                principalTable: "AzureSubscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_AzureSubscriptions_DatahubAzureSubscriptionId",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "AzureSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Projects_DatahubAzureSubscriptionId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DatahubAzureSubscriptionId",
                table: "Projects");

            migrationBuilder.AddColumn<string>(
                name: "SubscriptionId",
                table: "Projects",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: true);
        }
    }
}