using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Datahub.Core.Migrations
{
    /// <inheritdoc />
    public partial class ExternalUser : Migration
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

            migrationBuilder.AddColumn<int>(
                name: "ExternalUserId",
                table: "PortalUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Timestamp",
                table: "PortalUsers",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LeadEmail",
                table: "GCHostingWorkspaceDetails",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "FinancialAuthorityEmail",
                table: "GCHostingWorkspaceDetails",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateTable(
                name: "EntraUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GraphGuid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
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
INSERT INTO EntraUsers (GraphGuid, PortalUserId)
SELECT t.GraphGuid,  t.Id
FROM (
    SELECT GraphGuid, Id,
           ROW_NUMBER() OVER (PARTITION BY GraphGuid ORDER BY Id) rn
    FROM PortalUsers
    WHERE GraphGuid IS NOT NULL AND GraphGuid <> ''
) t
WHERE t.rn = 1
");
            // Now that EntraUsers has been populated, remove the old GraphGuid column and index
            migrationBuilder.DropIndex(
                name: "IX_PortalUsers_GraphGuid",
                table: "PortalUsers");

            migrationBuilder.DropColumn(
                name: "GraphGuid",
                table: "PortalUsers");

            migrationBuilder.CreateTable(
                name: "ExternalUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalSubject = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Organization = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Affiliation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FirstLoginDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastLoginDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UserDeactivatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeactivatedByUserId = table.Column<int>(type: "int", nullable: true),
                    DeactivationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PortalUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalUsers", x => x.Id);
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
                name: "WorkspaceInvitations",
                columns: table => new
                {
                    RequestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Project_ID = table.Column<int>(type: "int", nullable: false),
                    InvitationToken = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvitedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    InvitationExpiry = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    InvitationTokenAccepted = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    InvitationCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvitationCodeAccepted = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Request_DT = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceInvitations", x => x.RequestID);
                    table.ForeignKey(
                        name: "FK_WorkspaceInvitations_ExternalUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ExternalUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkspaceInvitations_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
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
                name: "IX_PortalUsers_ExternalUserId",
                table: "PortalUsers",
                column: "ExternalUserId",
                unique: true,
                filter: "[ExternalUserId] IS NOT NULL");

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
                name: "IX_ExternalUsers_DeactivatedByUserId",
                table: "ExternalUsers",
                column: "DeactivatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalUsers_ExternalSubject",
                table: "ExternalUsers",
                column: "ExternalSubject",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalUsers_PortalUserId",
                table: "ExternalUsers",
                column: "PortalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceInvitations_InvitationToken",
                table: "WorkspaceInvitations",
                column: "InvitationToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceInvitations_Project_ID",
                table: "WorkspaceInvitations",
                column: "Project_ID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceInvitations_UserId",
                table: "WorkspaceInvitations",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PortalUsers_ExternalUsers_ExternalUserId",
                table: "PortalUsers",
                column: "ExternalUserId",
                principalTable: "ExternalUsers",
                principalColumn: "Id");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortalUsers_ExternalUsers_ExternalUserId",
                table: "PortalUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Users_PortalUsers_ApprovedPortalUserId",
                table: "Project_Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Users_PortalUsers_PortalUserId",
                table: "Project_Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Users_Project_Roles_RoleId",
                table: "Project_Users");

            migrationBuilder.DropTable(
                name: "EntraUsers");

            migrationBuilder.DropTable(
                name: "WorkspaceInvitations");

            migrationBuilder.DropTable(
                name: "ExternalUsers");

            migrationBuilder.DropIndex(
                name: "IX_PortalUsers_ExternalUserId",
                table: "PortalUsers");

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
                name: "ExternalUserNotes",
                table: "Project_Users");

            migrationBuilder.DropColumn(
                name: "IsExternalRole",
                table: "Project_Roles");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "Project_Roles");

            migrationBuilder.DropColumn(
                name: "ExternalUserId",
                table: "PortalUsers");

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

            migrationBuilder.AlterColumn<string>(
                name: "LeadEmail",
                table: "GCHostingWorkspaceDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "FinancialAuthorityEmail",
                table: "GCHostingWorkspaceDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

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
        }
    }
}
