using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

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

            // Note: we will migrate existing PortalUsers.GraphGuid values into the new EntraUsers
            // table after the table is created. The column and its index are dropped later
            // to ensure we can read the values when populating EntraUsers.

            migrationBuilder.AddColumn<int>(
                name: "Datahub_ProjectProject_ID",
                table: "Project_Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalUserNotes",
                table: "Project_Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExternalRole",
                table: "Project_Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "Timestamp",
                table: "Project_Roles",
                type: "varbinary(max)",
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

            migrationBuilder.AddColumn<byte[]>(
                name: "Timestamp",
                table: "PortalUsers",
                type: "varbinary(max)",
                nullable: true);

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

            // Populate EntraUsers from existing PortalUsers.GraphGuid values before we drop the column
            migrationBuilder.Sql(@"
INSERT INTO EntraUsers (GraphGuid, Email, PortalUserId)
SELECT t.GraphGuid, t.Email, t.Id
FROM (
    SELECT GraphGuid, Email, Id,
           ROW_NUMBER() OVER (PARTITION BY GraphGuid ORDER BY Id) rn
    FROM PortalUsers
    WHERE GraphGuid IS NOT NULL AND GraphGuid <> ''
) t
WHERE t.rn = 1
" );

            migrationBuilder.CreateTable(
                name: "ExternalUsers",
                columns: table => new
                {
                    ExternalUserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FirstLogin_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLogin_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeactivatedDate_DT = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeactivatedByUserId = table.Column<int>(type: "int", nullable: true),
                    PortalUserId = table.Column<int>(type: "int", nullable: false)
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
                    RequestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserExternalUserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvitationToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvitationExpiry = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    InvitationTokenAccepted = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    InvitationCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvitationCodeAccepted = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Request_DT = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
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
                unique: true,
                filter: "[PortalUserId] IS NOT NULL");

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
                unique: true,
                filter: "[OID] IS NOT NULL");

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

            // Now that EntraUsers has been populated, remove the old GraphGuid column and index
            migrationBuilder.DropIndex(
                name: "IX_PortalUsers_GraphGuid",
                table: "PortalUsers");

            migrationBuilder.DropColumn(
                name: "GraphGuid",
                table: "PortalUsers");
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
