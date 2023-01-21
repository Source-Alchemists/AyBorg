using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AyBorg.Data.Agent.Migrations.SqlLite
{
    /// <inheritdoc />
    public partial class RemovedWebUi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsForceWebUiCommunicationEnabled",
                table: "AyBorgProjectSettings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsForceWebUiCommunicationEnabled",
                table: "AyBorgProjectSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
