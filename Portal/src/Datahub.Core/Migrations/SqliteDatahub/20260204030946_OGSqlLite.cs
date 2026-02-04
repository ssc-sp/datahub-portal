using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.SqliteDatahub
{
    /// <inheritdoc />
    public partial class OGSqlLite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OpenGovPublishingBlocklist",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DepartmentName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    EmailHostname = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateRemoved = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AddedByUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    RemovedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenGovPublishingBlocklist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenGovPublishingBlocklist_PortalUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OpenGovPublishingBlocklist_PortalUsers_RemovedByUserId",
                        column: x => x.RemovedByUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpenGovPublishingBlocklist_AddedByUserId",
                table: "OpenGovPublishingBlocklist",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenGovPublishingBlocklist_DepartmentName",
                table: "OpenGovPublishingBlocklist",
                column: "DepartmentName");

            migrationBuilder.CreateIndex(
                name: "IX_OpenGovPublishingBlocklist_EmailHostname",
                table: "OpenGovPublishingBlocklist",
                column: "EmailHostname");

            migrationBuilder.CreateIndex(
                name: "IX_OpenGovPublishingBlocklist_RemovedByUserId",
                table: "OpenGovPublishingBlocklist",
                column: "RemovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenGovPublishingBlocklist_Status",
                table: "OpenGovPublishingBlocklist",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpenGovPublishingBlocklist");
        }
    }
}
