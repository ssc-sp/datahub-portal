using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    public partial class ModifyingFGPShareTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeoObjectShares");

            migrationBuilder.CreateTable(
                name: "FGPObjectShares",
                columns: table => new
                {
                    GeoObjectShare_ID = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Json_TXT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovalForm_ID = table.Column<int>(type: "int", nullable: false),
                    ShareStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FGPObjectShares", x => x.GeoObjectShare_ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FGPObjectShares");

            migrationBuilder.CreateTable(
                name: "GeoObjectShares",
                columns: table => new
                {
                    GeoObjectShare_ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ApprovalFormCompleted = table.Column<bool>(type: "bit", nullable: false),
                    ApprovalForm_ID = table.Column<int>(type: "int", nullable: true),
                    Json_TXT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShareApproved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoObjectShares", x => x.GeoObjectShare_ID);
                });
        }
    }
}
