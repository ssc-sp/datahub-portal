using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    public partial class AddingGeoObjectShares : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeoObjectShares",
                columns: table => new
                {
                    GeoObjectShare_ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Json_TXT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovalForm_ID = table.Column<int>(type: "int", nullable: true),
                    ApprovalFormCompleted = table.Column<bool>(type: "bit", nullable: false),
                    ShareApproved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoObjectShares", x => x.GeoObjectShare_ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeoObjectShares");
        }
    }
}
