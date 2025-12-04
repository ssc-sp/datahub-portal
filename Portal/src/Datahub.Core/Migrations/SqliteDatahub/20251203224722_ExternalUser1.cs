using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Datahub.Core.Migrations.SqliteDatahub
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
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalUserNotes",
                table: "Project_Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExternalRole",
                table: "Project_Roles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "Timestamp",
                table: "Project_Roles",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "PortalUsers",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Timestamp",
                table: "PortalUsers",
                type: "BLOB",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EntraUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GraphGuid = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    PortalUserId = table.Column<int>(type: "INTEGER", nullable: true)
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
                    ExternalUserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    OID = table.Column<Guid>(type: "TEXT", nullable: true),
                    FirstLogin_DT = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastLogin_DT = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeactivatedDate_DT = table.Column<long>(type: "INTEGER", nullable: true),
                    DeactivatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    PortalUserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalUsers", x => x.ExternalUserID);
                    table.ForeignKey(
                        name: "FK_ExternalUsers_PortalUsers_DeactivatedByUserId",
                        column: x => x.DeactivatedByUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExternalUsers_PortalUsers_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExternalUserInvites",
                columns: table => new
                {
                    RequestID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserExternalUserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    InvitationToken = table.Column<string>(type: "TEXT", nullable: false),
                    InvitationExpiry = table.Column<long>(type: "INTEGER", nullable: false),
                    InvitationTokenAccepted = table.Column<long>(type: "INTEGER", nullable: true),
                    InvitationCode = table.Column<string>(type: "TEXT", nullable: false),
                    InvitationCodeAccepted = table.Column<long>(type: "INTEGER", nullable: true),
                    Request_DT = table.Column<long>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalUserInvites", x => x.RequestID);
                    table.ForeignKey(
                        name: "FK_ExternalUserInvites_ExternalUsers_UserExternalUserID",
                        column: x => x.UserExternalUserID,
                        principalTable: "ExternalUsers",
                        principalColumn: "ExternalUserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "IsExternalRole", "Timestamp" },
                values: new object[] { false, null });

            migrationBuilder.UpdateData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "IsExternalRole", "Timestamp" },
                values: new object[] { false, null });

            migrationBuilder.UpdateData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "IsExternalRole", "Timestamp" },
                values: new object[] { false, null });

            migrationBuilder.UpdateData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "IsExternalRole", "Timestamp" },
                values: new object[] { false, null });

            migrationBuilder.UpdateData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "IsExternalRole", "Timestamp" },
                values: new object[] { false, null });

            migrationBuilder.UpdateData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "IsExternalRole", "Timestamp" },
                values: new object[] { false, null });

            migrationBuilder.InsertData(
                table: "Project_Roles",
                columns: new[] { "Id", "Description", "IsExternalRole", "Name", "Timestamp" },
                values: new object[,]
                {
                    { 7, "Limited access to the web application interface only", true, "Web Application Access", null },
                    { 8, "Limited access to storage upload and download", true, "Storage", null },
                    { 9, "Access to both web application interface and storage resources", true, "Web Application and Storage", null }
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
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalUserInvites_UserExternalUserID",
                table: "ExternalUserInvites",
                column: "UserExternalUserID");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalUsers_DeactivatedByUserId",
                table: "ExternalUsers",
                column: "DeactivatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalUsers_OID",
                table: "ExternalUsers",
                column: "OID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalUsers_PortalUserId",
                table: "ExternalUsers",
                column: "PortalUserId",
                unique: true);

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
                name: "ExternalUserInvites");

            migrationBuilder.DropTable(
                name: "ExternalUsers");

            migrationBuilder.DropIndex(
                name: "IX_Project_Users_Datahub_ProjectProject_ID",
                table: "Project_Users");

            migrationBuilder.DeleteData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DropColumn(
                name: "Datahub_ProjectProject_ID",
                table: "Project_Users");

            migrationBuilder.DropColumn(
                name: "ExternalUserNotes",
                table: "Project_Users");

            migrationBuilder.DropColumn(
                name: "IsExternalRole",
                table: "Project_Roles");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "Project_Roles");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "PortalUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "PortalUsers",
                type: "TEXT",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 256);

            migrationBuilder.AddColumn<string>(
                name: "GraphGuid",
                table: "PortalUsers",
                type: "TEXT",
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
