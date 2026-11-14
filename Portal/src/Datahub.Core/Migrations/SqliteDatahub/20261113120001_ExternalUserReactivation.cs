using Datahub.Core.Model.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.SqliteDatahub;

[DbContext(typeof(SqliteDatahubContext))]
[Migration("20261113120001_ExternalUserReactivation")]
public partial class ExternalUserReactivation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "ReactivationAccountExpiry",
            table: "WorkspaceInvitations",
            type: "INTEGER",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ReactivationAccountExpiry",
            table: "WorkspaceInvitations");
    }
}
