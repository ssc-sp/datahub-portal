using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.SqliteDatahub
{
    /// <inheritdoc />
    public partial class AddHostingServicesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GCHostingWorkspaceDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GcHostingId = table.Column<string>(type: "TEXT", nullable: true),
                    LeadFirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LeadLastName = table.Column<string>(type: "TEXT", nullable: false),
                    DepartmentName = table.Column<string>(type: "TEXT", nullable: false),
                    LeadEmail = table.Column<string>(type: "TEXT", nullable: false),
                    FinancialAuthorityFirstName = table.Column<string>(type: "TEXT", nullable: false),
                    FinancialAuthorityLastName = table.Column<string>(type: "TEXT", nullable: false),
                    FinancialAuthorityCostCentre = table.Column<string>(type: "TEXT", nullable: false),
                    WorkspaceTitle = table.Column<string>(type: "TEXT", nullable: false),
                    WorkspaceDescription = table.Column<string>(type: "TEXT", nullable: false),
                    WorkspaceIdentifier = table.Column<string>(type: "TEXT", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", nullable: true),
                    Keywords = table.Column<string>(type: "TEXT", nullable: false),
                    AreaOfScience = table.Column<string>(type: "TEXT", nullable: false),
                    RetentionPeriodYears = table.Column<int>(type: "INTEGER", nullable: false),
                    SecurityClassification = table.Column<string>(type: "TEXT", nullable: false),
                    GeneratesInfoBusinessValue = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProjectTitle = table.Column<string>(type: "TEXT", nullable: false),
                    ProjectDescription = table.Column<string>(type: "TEXT", nullable: false),
                    ProjectStartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProjectEndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CBRName = table.Column<string>(type: "TEXT", nullable: true),
                    CBRID = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GCHostingWorkspaceDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GCHostingWorkspaceDetails_Projects_Id",
                        column: x => x.Id,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GCHostingWorkspaceDetails");
        }
    }
}
