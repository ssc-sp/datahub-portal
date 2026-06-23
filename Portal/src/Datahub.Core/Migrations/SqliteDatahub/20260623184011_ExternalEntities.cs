using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.SqliteDatahub
{
    /// <inheritdoc />
    public partial class ExternalEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalUserLockAuditEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PortalUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    EventType = table.Column<string>(type: "TEXT", nullable: false),
                    EventDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: true),
                    EvidenceUrl = table.Column<string>(type: "TEXT", nullable: true),
                    PreviousExpiryDate = table.Column<long>(type: "INTEGER", nullable: true),
                    AppliedExpiryDate = table.Column<long>(type: "INTEGER", nullable: true),
                    PerformedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
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
        }
    }
}
