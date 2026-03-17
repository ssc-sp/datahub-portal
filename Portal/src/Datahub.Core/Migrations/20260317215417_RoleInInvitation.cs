using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations
{
    /// <inheritdoc />
    public partial class RoleInInvitation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InvitedById",
                table: "WorkspaceInvitations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Requested_RoleId",
                table: "WorkspaceInvitations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OpenGovPublishingBlocklist",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmailHostname = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateRemoved = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedByUserId = table.Column<int>(type: "int", nullable: false),
                    RemovedByUserId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
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
                name: "IX_WorkspaceInvitations_InvitedById",
                table: "WorkspaceInvitations",
                column: "InvitedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceInvitations_Requested_RoleId",
                table: "WorkspaceInvitations",
                column: "Requested_RoleId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceInvitations_PortalUsers_InvitedById",
                table: "WorkspaceInvitations",
                column: "InvitedById",
                principalTable: "PortalUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceInvitations_Project_Roles_Requested_RoleId",
                table: "WorkspaceInvitations",
                column: "Requested_RoleId",
                principalTable: "Project_Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceInvitations_PortalUsers_InvitedById",
                table: "WorkspaceInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceInvitations_Project_Roles_Requested_RoleId",
                table: "WorkspaceInvitations");

            migrationBuilder.DropTable(
                name: "OpenGovPublishingBlocklist");

            migrationBuilder.DropIndex(
                name: "IX_WorkspaceInvitations_InvitedById",
                table: "WorkspaceInvitations");

            migrationBuilder.DropIndex(
                name: "IX_WorkspaceInvitations_Requested_RoleId",
                table: "WorkspaceInvitations");

            migrationBuilder.DropColumn(
                name: "InvitedById",
                table: "WorkspaceInvitations");

            migrationBuilder.DropColumn(
                name: "Requested_RoleId",
                table: "WorkspaceInvitations");
        }
    }
}
