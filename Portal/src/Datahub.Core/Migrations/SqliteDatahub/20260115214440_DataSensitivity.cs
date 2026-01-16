using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.SqliteDatahub
{
    /// <inheritdoc />
    public partial class DataSensitivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Affiliation",
                table: "ExternalUsers");

            migrationBuilder.AddColumn<string>(
                name: "ExternalSubjectInvited",
                table: "WorkspaceInvitations",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvitationRationale_EN",
                table: "WorkspaceInvitations",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "Timestamp",
                table: "WorkspaceInvitations",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserExpiryDate",
                table: "ExternalUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalSubjectInvited",
                table: "WorkspaceInvitations");

            migrationBuilder.DropColumn(
                name: "InvitationRationale_EN",
                table: "WorkspaceInvitations");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "WorkspaceInvitations");

            migrationBuilder.DropColumn(
                name: "UserExpiryDate",
                table: "ExternalUsers");

            migrationBuilder.AddColumn<string>(
                name: "Affiliation",
                table: "ExternalUsers",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }
    }
}
