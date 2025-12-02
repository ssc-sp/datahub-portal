using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations
{
    /// <inheritdoc />
    public partial class ExternalUser1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Project_Users_PortalUsers_ApprovedPortalUserId",
                table: "Project_Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Users_PortalUsers_PortalUserId",
                table: "Project_Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Users_Project_Roles_RoleId",
                table: "Project_Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Users_Projects_Project_ID",
                table: "Project_Users");

            migrationBuilder.DropIndex(
                name: "IX_PortalUsers_GraphGuid",
                table: "PortalUsers");

            migrationBuilder.DropColumn(
                name: "GraphGuid",
                table: "PortalUsers");

            migrationBuilder.AddColumn<int>(
                name: "Datahub_ProjectProject_ID",
                table: "Project_Users",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "PortalUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "PortalUsers",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "EntraUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GraphGuid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PortalUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntraUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntraUsers_PortalUsers_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExternalUsers",
                columns: table => new
                {
                    ExternalUserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstLogin_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLogin_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastPermissionsUpdated_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PortalUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalUsers", x => x.ExternalUserID);
                    table.ForeignKey(
                        name: "FK_ExternalUsers_PortalUsers_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExternalUserRequests",
                columns: table => new
                {
                    RequestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserOID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Request_DT = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RequestContent = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalUserRequests", x => x.RequestID);
                    table.ForeignKey(
                        name: "FK_ExternalUserRequests_ExternalUsers_UserOID",
                        column: x => x.UserOID,
                        principalTable: "ExternalUsers",
                        principalColumn: "ExternalUserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Project_Users_Datahub_ProjectProject_ID",
                table: "Project_Users",
                column: "Datahub_ProjectProject_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EntraUsers_GraphGuid",
                table: "EntraUsers",
                column: "GraphGuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntraUsers_PortalUserId",
                table: "EntraUsers",
                column: "PortalUserId",
                unique: true,
                filter: "[PortalUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalUserRequests_UserOID",
                table: "ExternalUserRequests",
                column: "UserOID");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalUsers_PortalUserId",
                table: "ExternalUsers",
                column: "PortalUserId",
                unique: true,
                filter: "[PortalUserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Users_PortalUsers_ApprovedPortalUserId",
                table: "Project_Users",
                column: "ApprovedPortalUserId",
                principalTable: "PortalUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Users_PortalUsers_PortalUserId",
                table: "Project_Users",
                column: "PortalUserId",
                principalTable: "PortalUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Users_Project_Roles_RoleId",
                table: "Project_Users",
                column: "RoleId",
                principalTable: "Project_Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Users_Projects_Datahub_ProjectProject_ID",
                table: "Project_Users",
                column: "Datahub_ProjectProject_ID",
                principalTable: "Projects",
                principalColumn: "Project_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Users_Projects_Project_ID",
                table: "Project_Users",
                column: "Project_ID",
                principalTable: "Projects",
                principalColumn: "Project_ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Project_Users_PortalUsers_ApprovedPortalUserId",
                table: "Project_Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Users_PortalUsers_PortalUserId",
                table: "Project_Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Users_Project_Roles_RoleId",
                table: "Project_Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Users_Projects_Datahub_ProjectProject_ID",
                table: "Project_Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Users_Projects_Project_ID",
                table: "Project_Users");

            migrationBuilder.DropTable(
                name: "EntraUsers");

            migrationBuilder.DropTable(
                name: "ExternalUserRequests");

            migrationBuilder.DropTable(
                name: "ExternalUsers");

            migrationBuilder.DropIndex(
                name: "IX_Project_Users_Datahub_ProjectProject_ID",
                table: "Project_Users");

            migrationBuilder.DropColumn(
                name: "Datahub_ProjectProject_ID",
                table: "Project_Users");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "PortalUsers",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "PortalUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GraphGuid",
                table: "PortalUsers",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PortalUsers_GraphGuid",
                table: "PortalUsers",
                column: "GraphGuid",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Users_PortalUsers_ApprovedPortalUserId",
                table: "Project_Users",
                column: "ApprovedPortalUserId",
                principalTable: "PortalUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Users_PortalUsers_PortalUserId",
                table: "Project_Users",
                column: "PortalUserId",
                principalTable: "PortalUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Users_Project_Roles_RoleId",
                table: "Project_Users",
                column: "RoleId",
                principalTable: "Project_Roles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Users_Projects_Project_ID",
                table: "Project_Users",
                column: "Project_ID",
                principalTable: "Projects",
                principalColumn: "Project_ID");
        }
    }
}
