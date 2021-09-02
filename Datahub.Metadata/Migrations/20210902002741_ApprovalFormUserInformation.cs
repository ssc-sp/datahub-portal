using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Metadata.Migrations
{
    public partial class ApprovalFormUserInformation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Branch_NAME",
                table: "ApprovalForms",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department_NAME",
                table: "ApprovalForms",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Division_NAME",
                table: "ApprovalForms",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email_EMAIL",
                table: "ApprovalForms",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name_NAME",
                table: "ApprovalForms",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone_TXT",
                table: "ApprovalForms",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Section_NAME",
                table: "ApprovalForms",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sector_NAME",
                table: "ApprovalForms",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Branch_NAME",
                table: "ApprovalForms");

            migrationBuilder.DropColumn(
                name: "Department_NAME",
                table: "ApprovalForms");

            migrationBuilder.DropColumn(
                name: "Division_NAME",
                table: "ApprovalForms");

            migrationBuilder.DropColumn(
                name: "Email_EMAIL",
                table: "ApprovalForms");

            migrationBuilder.DropColumn(
                name: "Name_NAME",
                table: "ApprovalForms");

            migrationBuilder.DropColumn(
                name: "Phone_TXT",
                table: "ApprovalForms");

            migrationBuilder.DropColumn(
                name: "Section_NAME",
                table: "ApprovalForms");

            migrationBuilder.DropColumn(
                name: "Sector_NAME",
                table: "ApprovalForms");
        }
    }
}
