using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.ProjectForms.Migrations.DatahubProjectDB
{
    public partial class PipelineLinks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "Project_Pipeline_Links",
                columns: table => new
                {
                    Project_ID = table.Column<int>(type: "int", nullable: false),
                    Process_Nm = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Pipeline_Links", x => new { x.Project_ID, x.Process_Nm });
                    table.ForeignKey(
                        name: "FK_Project_Pipeline_Links_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Project_Pipeline_Links");

        }
    }
}
