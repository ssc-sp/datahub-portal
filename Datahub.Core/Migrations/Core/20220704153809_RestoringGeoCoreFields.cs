using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    public partial class RestoringGeoCoreFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeoObjectShares");

            migrationBuilder.CreateTable(
                name: "SpatialObjectShares",
                columns: table => new
                {
                    GeoObjectShare_ID = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Json_TXT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email_Contact_TXT = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ApprovalForm_ID = table.Column<int>(type: "int", nullable: false),
                    ShareStatus = table.Column<int>(type: "int", nullable: false),
                    Approval_Document_URL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Publication_ID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpatialObjectShares", x => x.GeoObjectShare_ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpatialObjectShares");

            migrationBuilder.CreateTable(
                name: "GeoObjectShares",
                columns: table => new
                {
                    GeoObjectShare_ID = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ApprovalForm_ID = table.Column<int>(type: "int", nullable: false),
                    Approval_Document_URL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Email_Contact_TXT = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Json_TXT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Publication_ID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShareStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoObjectShares", x => x.GeoObjectShare_ID);
                });
        }
    }
}
