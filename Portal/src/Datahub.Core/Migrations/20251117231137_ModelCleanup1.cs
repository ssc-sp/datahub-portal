using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations
{
    /// <inheritdoc />
    public partial class ModelCleanup1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Project_Delete_Questionnaires_PortalUsers_DeletedById",
                table: "Project_Delete_Questionnaires");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Delete_Questionnaires_Projects_Project_ID",
                table: "Project_Delete_Questionnaires");

            migrationBuilder.DropColumn(
                name: "CloudProvider",
                table: "Project_Costs");

            // Delete orphaned records before making Project_ID non-nullable
            migrationBuilder.Sql(@"
                DELETE FROM [Project_Delete_Questionnaires] 
                WHERE [Project_ID] IS NULL 
                   OR [Project_ID] NOT IN (SELECT [Project_ID] FROM [Projects])
                   OR [DeletedById] IS NULL
                   OR [DeletedById] NOT IN (SELECT [Id] FROM [PortalUsers])
            ");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserInactivityNotifications",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AchievementId",
                table: "UserAchievements",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(8)",
                oldMaxLength: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CloudProvider",
                table: "Project_Storage_Avgs",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldMaxLength: 16,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Project_ID",
                table: "Project_Delete_Questionnaires",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DeletedById",
                table: "Project_Delete_Questionnaires",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ServiceName",
                table: "Project_Costs",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConcatenatedRules",
                table: "Achivements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Delete_Questionnaires_PortalUsers_DeletedById",
                table: "Project_Delete_Questionnaires",
                column: "DeletedById",
                principalTable: "PortalUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Delete_Questionnaires_Projects_Project_ID",
                table: "Project_Delete_Questionnaires",
                column: "Project_ID",
                principalTable: "Projects",
                principalColumn: "Project_ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Project_Delete_Questionnaires_PortalUsers_DeletedById",
                table: "Project_Delete_Questionnaires");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Delete_Questionnaires_Projects_Project_ID",
                table: "Project_Delete_Questionnaires");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserInactivityNotifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "AchievementId",
                table: "UserAchievements",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(8)",
                oldMaxLength: 8);

            migrationBuilder.AlterColumn<string>(
                name: "CloudProvider",
                table: "Project_Storage_Avgs",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldMaxLength: 16);

            migrationBuilder.AlterColumn<int>(
                name: "Project_ID",
                table: "Project_Delete_Questionnaires",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DeletedById",
                table: "Project_Delete_Questionnaires",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ServiceName",
                table: "Project_Costs",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AddColumn<string>(
                name: "CloudProvider",
                table: "Project_Costs",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConcatenatedRules",
                table: "Achivements",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Delete_Questionnaires_PortalUsers_DeletedById",
                table: "Project_Delete_Questionnaires",
                column: "DeletedById",
                principalTable: "PortalUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Delete_Questionnaires_Projects_Project_ID",
                table: "Project_Delete_Questionnaires",
                column: "Project_ID",
                principalTable: "Projects",
                principalColumn: "Project_ID");
        }
    }
}
