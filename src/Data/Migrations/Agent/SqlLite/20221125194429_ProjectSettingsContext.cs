using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AyBorg.Data.Agent.Migrations.SqlLite
{
    /// <inheritdoc />
    public partial class ProjectSettingsContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AyBorgProjects_ProjectSettingsRecord_SettingsDbId",
                table: "AyBorgProjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectSettingsRecord",
                table: "ProjectSettingsRecord");

            migrationBuilder.RenameTable(
                name: "ProjectSettingsRecord",
                newName: "AyBorgProjectSettings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AyBorgProjectSettings",
                table: "AyBorgProjectSettings",
                column: "DbId");

            migrationBuilder.AddForeignKey(
                name: "FK_AyBorgProjects_AyBorgProjectSettings_SettingsDbId",
                table: "AyBorgProjects",
                column: "SettingsDbId",
                principalTable: "AyBorgProjectSettings",
                principalColumn: "DbId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AyBorgProjects_AyBorgProjectSettings_SettingsDbId",
                table: "AyBorgProjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AyBorgProjectSettings",
                table: "AyBorgProjectSettings");

            migrationBuilder.RenameTable(
                name: "AyBorgProjectSettings",
                newName: "ProjectSettingsRecord");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectSettingsRecord",
                table: "ProjectSettingsRecord",
                column: "DbId");

            migrationBuilder.AddForeignKey(
                name: "FK_AyBorgProjects_ProjectSettingsRecord_SettingsDbId",
                table: "AyBorgProjects",
                column: "SettingsDbId",
                principalTable: "ProjectSettingsRecord",
                principalColumn: "DbId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
