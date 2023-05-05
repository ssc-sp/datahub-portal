using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    /// <inheritdoc />
    public partial class MoveUserTrackingIntoContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserRecent",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRecent", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AcceptedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "UserRecentLinks",
                columns: table => new
                {
                    UserRecentActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LinkType = table.Column<int>(type: "int", nullable: false),
                    PowerBIURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Variant = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatabricksURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WebFormsURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataProject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PBIReportId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PBIWorkspaceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    accessedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExternalUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResourceArticleId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResourceArticleTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserRecentUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRecentLinks", x => x.UserRecentActionId);
                    table.ForeignKey(
                        name: "FK_UserRecentLinks_UserRecent_UserRecentUserId",
                        column: x => x.UserRecentUserId,
                        principalTable: "UserRecent",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRecentLinks_UserRecentUserId",
                table: "UserRecentLinks",
                column: "UserRecentUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRecentLinks");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropTable(
                name: "UserRecent");
        }
    }
}
