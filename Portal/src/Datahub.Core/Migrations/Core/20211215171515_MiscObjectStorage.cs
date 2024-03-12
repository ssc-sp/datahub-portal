#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Core.Migrations.Core;

public partial class MiscObjectStorage : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "MiscStoredObjects",
            columns: table => new
            {
                GeneratedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                TypeName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                JsonContent = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MiscStoredObjects", x => x.GeneratedId);
                table.UniqueConstraint("AK_MiscStoredObjects_TypeName_Id", x => new { x.TypeName, x.Id });
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "MiscStoredObjects");
    }
}