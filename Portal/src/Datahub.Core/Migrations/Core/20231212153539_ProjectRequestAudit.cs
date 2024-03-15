using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
	/// <inheritdoc />
	public partial class ProjectRequestAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Is_Completed",
                table: "Project_Requests");

            migrationBuilder.DropColumn(
                name: "ServiceType",
                table: "Project_Requests");

            migrationBuilder.DropColumn(
                name: "User_ID",
                table: "Project_Requests");

            migrationBuilder.DropColumn(
                name: "User_Name",
                table: "Project_Requests");

            migrationBuilder.RenameColumn(
                name: "ServiceRequests_Date_DT",
                table: "Project_Requests",
                newName: "RequestedDateTime");

            migrationBuilder.RenameColumn(
                name: "Notification_Sent",
                table: "Project_Requests",
                newName: "CompletedDateTime");

            migrationBuilder.RenameColumn(
                name: "ServiceRequests_ID",
                table: "Project_Requests",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "RequestType",
                table: "Project_Requests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "Project_Requests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestType",
                table: "Project_Requests");

            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "Project_Requests");

            migrationBuilder.RenameColumn(
                name: "RequestedDateTime",
                table: "Project_Requests",
                newName: "ServiceRequests_Date_DT");

            migrationBuilder.RenameColumn(
                name: "CompletedDateTime",
                table: "Project_Requests",
                newName: "Notification_Sent");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Project_Requests",
                newName: "ServiceRequests_ID");

            migrationBuilder.AddColumn<DateTime>(
                name: "Is_Completed",
                table: "Project_Requests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceType",
                table: "Project_Requests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "User_ID",
                table: "Project_Requests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "User_Name",
                table: "Project_Requests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
