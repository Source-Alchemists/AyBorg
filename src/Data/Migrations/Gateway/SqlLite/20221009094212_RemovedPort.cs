using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AyBorg.Data.Gateway.Migrations.SqlLite
{
    public partial class RemovedPort : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Port",
                table: "ServiceEntries");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "ServiceEntries",
                newName: "Url");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Url",
                table: "ServiceEntries",
                newName: "Address");

            migrationBuilder.AddColumn<int>(
                name: "Port",
                table: "ServiceEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
