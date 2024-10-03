using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations
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
                    Id = table.Column<int>(type: "int", nullable: false),
                    GcHostingId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LeadFirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeadLastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartmentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeadEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FinancialAuthorityFirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FinancialAuthorityLastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FinancialAuthorityCostCentre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkspaceTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkspaceDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkspaceIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Keywords = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AreaOfScience = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RetentionPeriodYears = table.Column<int>(type: "int", nullable: false),
                    SecurityClassification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneratesInfoBusinessValue = table.Column<bool>(type: "bit", nullable: false),
                    ProjectTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProjectDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProjectStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProjectEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CBRName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CBRID = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
