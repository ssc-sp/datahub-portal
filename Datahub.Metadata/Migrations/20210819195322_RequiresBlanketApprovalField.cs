using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Metadata.Migrations
{
    public partial class RequiresBlanketApprovalField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Requires_Blanket_Approval_FLAG",
                table: "ApprovalForms",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Requires_Blanket_Approval_FLAG",
                table: "ApprovalForms");
        }
    }
}
