using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core;

public partial class NewProjectResources2 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Project_Databases");

        migrationBuilder.CreateTable(
            name: "Project_Resources2",
            columns: table => new
            {
                ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ResourceType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                ClassName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                JsonContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ProjectId = table.Column<int>(type: "int", nullable: false),
                TimeRequested = table.Column<DateTime>(type: "datetime2", nullable: false),
                TimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Project_Resources2", x => x.ResourceId);
                table.ForeignKey(
                    name: "FK_Project_Resources2_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "Projects",
                    principalColumn: "Project_ID",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Project_Resources2_ProjectId",
            table: "Project_Resources2",
            column: "ProjectId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Project_Resources2");

        migrationBuilder.CreateTable(
            name: "Project_Databases",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Project_ID = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Project_Databases", x => x.Id);
                table.ForeignKey(
                    name: "FK_Project_Databases_Projects_Project_ID",
                    column: x => x.Project_ID,
                    principalTable: "Projects",
                    principalColumn: "Project_ID");
            });

        migrationBuilder.CreateIndex(
            name: "IX_Project_Databases_Project_ID",
            table: "Project_Databases",
            column: "Project_ID");
    }
}