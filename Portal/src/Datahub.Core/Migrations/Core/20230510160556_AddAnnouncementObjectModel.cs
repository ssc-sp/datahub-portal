using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
	/// <inheritdoc />
	public partial class AddAnnouncementObjectModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Announcements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TitleEn = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    TitleFr = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PreviewEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreviewFr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BodyEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BodyFr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ForceHidden = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcements", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Announcements");
        }
    }
}
