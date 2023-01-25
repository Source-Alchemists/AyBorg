using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AyBorg.Data.Agent.Migrations.PostgreSQL
{
    /// <inheritdoc />
    public partial class ApprovedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "VersionName",
                table: "AyBorgProjectMetas",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "AyBorgProjectMetas",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "AyBorgProjectMetas");

            migrationBuilder.AlterColumn<string>(
                name: "VersionName",
                table: "AyBorgProjectMetas",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }
    }
}
