using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations
{
    /// <inheritdoc />
    public partial class ExternalEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[UserWorkspaceLocks]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[UserWorkspaceLocks];
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserWorkspaceLocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerformedByUserId = table.Column<int>(type: "int", nullable: true),
                    PortalUserId = table.Column<int>(type: "int", nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EvidenceUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWorkspaceLocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserWorkspaceLocks_PortalUsers_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserWorkspaceLocks_PortalUsers_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserWorkspaceLocks_EventType",
                table: "UserWorkspaceLocks",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_UserWorkspaceLocks_PerformedByUserId",
                table: "UserWorkspaceLocks",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWorkspaceLocks_PortalUserId_EventDate",
                table: "UserWorkspaceLocks",
                columns: new[] { "PortalUserId", "EventDate" });
        }
    }
}
