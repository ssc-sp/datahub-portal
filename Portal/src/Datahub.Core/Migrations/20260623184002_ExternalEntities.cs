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
            migrationBuilder.DropTable(
                name: "UserWorkspaceLocks");

            migrationBuilder.CreateTable(
                name: "ExternalUserLockAuditEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortalUserId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EvidenceUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviousExpiryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AppliedExpiryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PerformedByUserId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalUserLockAuditEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalUserLockAuditEvents_PortalUsers_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExternalUserLockAuditEvents_PortalUsers_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalUserLockAuditEvents_EventType",
                table: "ExternalUserLockAuditEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalUserLockAuditEvents_PerformedByUserId",
                table: "ExternalUserLockAuditEvents",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalUserLockAuditEvents_PortalUserId_EventDate",
                table: "ExternalUserLockAuditEvents",
                columns: new[] { "PortalUserId", "EventDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalUserLockAuditEvents");

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
