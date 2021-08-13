using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Metadata.Migrations
{
    public partial class AddingApprovalFormTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprovalForms",
                columns: table => new
                {
                    ApprovalFormId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Dataset_Title_TXT = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Type_Of_Data_TXT = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Copyright_Restrictions_FLAG = table.Column<bool>(type: "bit", nullable: false),
                    Authority_To_Release_FLAG = table.Column<bool>(type: "bit", nullable: false),
                    Private_Personal_Information_FLAG = table.Column<bool>(type: "bit", nullable: false),
                    Subject_To_Exceptions_Or_Eclusions_FLAG = table.Column<bool>(type: "bit", nullable: false),
                    Not_Clasified_Or_Protected_FLAG = table.Column<bool>(type: "bit", nullable: false),
                    Can_Be_Released_For_Free_FLAG = table.Column<bool>(type: "bit", nullable: false),
                    Machine_Readable_FLAG = table.Column<bool>(type: "bit", nullable: false),
                    Non_Propietary_Format_FLAG = table.Column<bool>(type: "bit", nullable: false),
                    Localized_Metadata_FLAG = table.Column<bool>(type: "bit", nullable: false),
                    Updated_On_Going_Basis_FLAG = table.Column<bool>(type: "bit", nullable: false),
                    Collection_Of_Datasets_FLAG = table.Column<bool>(type: "bit", nullable: false),
                    Approval_InSitu_FLAG = table.Column<bool>(type: "bit", nullable: false),
                    Approval_Other_TXT = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalForms", x => x.ApprovalFormId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalForms");
        }
    }
}
