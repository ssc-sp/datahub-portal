using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Metadata.Migrations
{
    public partial class AddingSubjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    SubjectId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Subject_TXT = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.SubjectId);
                });

            migrationBuilder.CreateTable(
                name: "SubSubjects",
                columns: table => new
                {
                    SubSubjectId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name_English_TXT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name_French_TXT = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubSubjects", x => x.SubSubjectId);
                });

            migrationBuilder.CreateTable(
                name: "SubSubjectSubject",
                columns: table => new
                {
                    SubSubjectsSubSubjectId = table.Column<int>(type: "int", nullable: false),
                    SubjectsSubjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubSubjectSubject", x => new { x.SubSubjectsSubSubjectId, x.SubjectsSubjectId });
                    table.ForeignKey(
                        name: "FK_SubSubjectSubject_Subjects_SubjectsSubjectId",
                        column: x => x.SubjectsSubjectId,
                        principalTable: "Subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubSubjectSubject_SubSubjects_SubSubjectsSubSubjectId",
                        column: x => x.SubSubjectsSubSubjectId,
                        principalTable: "SubSubjects",
                        principalColumn: "SubSubjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Subject_TXT",
                table: "Subjects",
                column: "Subject_TXT",
                unique: true,
                filter: "[Subject_TXT] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SubSubjectSubject_SubjectsSubjectId",
                table: "SubSubjectSubject",
                column: "SubjectsSubjectId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubSubjectSubject");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "SubSubjects");
        }
    }
}
