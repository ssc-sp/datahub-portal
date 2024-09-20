using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddDataStewardBool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDataSteward",
                table: "Project_Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "Revoke the user's access to the workspace");

            migrationBuilder.UpdateData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "Head of the workspace and bears business responsibility for success of the workspace");

            migrationBuilder.UpdateData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Management authority within the workspace with direct supervision over the cloud resourcing and users");

            migrationBuilder.UpdateData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "Responsible for contributing to the overall workspace objectives and deliverables");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDataSteward",
                table: "Project_Users");

            migrationBuilder.UpdateData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "Revoke the user's access to the project's private resources");

            migrationBuilder.UpdateData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "Head of the business unit and bears business responsibility for successful implementation and availability");

            migrationBuilder.UpdateData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Management authority within the project with direct supervision over the project resources and deliverables");

            migrationBuilder.UpdateData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "Responsible for contributing to the overall project objectives and deliverables to ensure success");
        }
    }
}
