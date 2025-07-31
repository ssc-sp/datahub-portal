using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Metadata.Migrations
{
    /// <inheritdoc />
    public partial class UpdateImsoFormFields1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Not_Clasified_Or_Protected_FLAG",
                table: "ApprovalForms",
                newName: "Security_Compliant_FLAG");

            migrationBuilder.RenameColumn(
                name: "Localized_Metadata_FLAG",
                table: "ApprovalForms",
                newName: "Localized_FLAG");

            migrationBuilder.RenameColumn(
                name: "Copyright_Restrictions_FLAG",
                table: "ApprovalForms",
                newName: "Misc_Compliant_FLAG");

            migrationBuilder.AddColumn<bool>(
                name: "Accessible_Format_FLAG",
                table: "ApprovalForms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Confidentiality_FLAG",
                table: "ApprovalForms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Privacy_Exemption_FLAG",
                table: "ApprovalForms",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Accessible_Format_FLAG",
                table: "ApprovalForms");

            migrationBuilder.DropColumn(
                name: "Confidentiality_FLAG",
                table: "ApprovalForms");

            migrationBuilder.DropColumn(
                name: "Privacy_Exemption_FLAG",
                table: "ApprovalForms");

            migrationBuilder.RenameColumn(
                name: "Security_Compliant_FLAG",
                table: "ApprovalForms",
                newName: "Not_Clasified_Or_Protected_FLAG");

            migrationBuilder.RenameColumn(
                name: "Misc_Compliant_FLAG",
                table: "ApprovalForms",
                newName: "Copyright_Restrictions_FLAG");

            migrationBuilder.RenameColumn(
                name: "Localized_FLAG",
                table: "ApprovalForms",
                newName: "Localized_Metadata_FLAG");
        }
    }
}
