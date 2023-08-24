using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    /// <inheritdoc />
    public partial class AddingCatalogObjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatalogObjects",
                columns: table => new
                {
                    ObjectType = table.Column<int>(type: "int", nullable: false),
                    ObjectId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name_English = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name_French = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Desc_English = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Desc_French = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogObjects", x => new { x.ObjectType, x.ObjectId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogObjects");
        }
    }
}
