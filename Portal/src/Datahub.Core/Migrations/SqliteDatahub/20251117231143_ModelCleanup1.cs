using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.SqliteDatahub
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

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserInactivityNotifications",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AchievementId",
                table: "UserAchievements",
                type: "TEXT",
                maxLength: 8,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CloudProvider",
                table: "Project_Storage_Avgs",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 16,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Project_ID",
                table: "Project_Delete_Questionnaires",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DeletedById",
                table: "Project_Delete_Questionnaires",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ServiceName",
                table: "Project_Costs",
                type: "TEXT",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConcatenatedRules",
                table: "Achivements",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
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
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "AchievementId",
                table: "UserAchievements",
                type: "TEXT",
                maxLength: 8,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 8);

            migrationBuilder.AlterColumn<string>(
                name: "CloudProvider",
                table: "Project_Storage_Avgs",
                type: "TEXT",
                maxLength: 16,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 16);

            migrationBuilder.AlterColumn<int>(
                name: "Project_ID",
                table: "Project_Delete_Questionnaires",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "DeletedById",
                table: "Project_Delete_Questionnaires",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "ServiceName",
                table: "Project_Costs",
                type: "TEXT",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 64);

            migrationBuilder.AddColumn<string>(
                name: "CloudProvider",
                table: "Project_Costs",
                type: "TEXT",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConcatenatedRules",
                table: "Achivements",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

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
