using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.ProjectForms.Migrations.DatahubProjectDB
{
    public partial class ProjectUser2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<int>(
            //    name: "WebForm_ID",
            //    table: "WebForms",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            //migrationBuilder.AlterColumn<int>(
            //    name: "Project_ID",
            //    table: "Projects",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Databricks_URL",
                table: "Projects",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PowerBI_URL",
                table: "Projects",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebForms_URL",
                table: "Projects",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            //migrationBuilder.AlterColumn<int>(
            //    name: "Comment_ID",
            //    table: "Project_Comments",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            //migrationBuilder.AlterColumn<int>(
            //    name: "ID",
            //    table: "PowerBI_License_User_Requests",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            //migrationBuilder.AlterColumn<int>(
            //    name: "Request_ID",
            //    table: "PowerBI_License_Requests",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            //migrationBuilder.AlterColumn<int>(
            //    name: "FieldID",
            //    table: "Fields",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.CreateTable(
                name: "Project_Users",
                columns: table => new
                {
                    ProjectUser_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    User_ID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Databricks = table.Column<bool>(type: "bit", nullable: true),
                    PowerBI = table.Column<bool>(type: "bit", nullable: true),
                    WebForms = table.Column<bool>(type: "bit", nullable: true),
                    Project_ID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Users", x => x.ProjectUser_ID);
                    table.ForeignKey(
                        name: "FK_Project_Users_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Project_Users_Project_ID",
                table: "Project_Users",
                column: "Project_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Project_Users");

            migrationBuilder.DropColumn(
                name: "Databricks_URL",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "PowerBI_URL",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "WebForms_URL",
                table: "Projects");

            migrationBuilder.AlterColumn<int>(
                name: "WebForm_ID",
                table: "WebForms",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "Project_ID",
                table: "Projects",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "Comment_ID",
                table: "Project_Comments",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "ID",
                table: "PowerBI_License_User_Requests",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "Request_ID",
                table: "PowerBI_License_Requests",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "FieldID",
                table: "Fields",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");
        }
    }
}
