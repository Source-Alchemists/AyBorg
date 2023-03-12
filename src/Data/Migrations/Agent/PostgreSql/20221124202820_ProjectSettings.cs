using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AyBorg.Data.Agent.Migrations.PostgreSQL
{
    /// <inheritdoc />
    public partial class ProjectSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SettingsDbId",
                table: "AyBorgProjects",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ProjectSettingsRecord",
                columns: table => new
                {
                    DbId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsForceResultCommunicationEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsForceWebUiCommunicationEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectSettingsRecord", x => x.DbId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AyBorgProjects_SettingsDbId",
                table: "AyBorgProjects",
                column: "SettingsDbId");

            migrationBuilder.AddForeignKey(
                name: "FK_AyBorgProjects_ProjectSettingsRecord_SettingsDbId",
                table: "AyBorgProjects",
                column: "SettingsDbId",
                principalTable: "ProjectSettingsRecord",
                principalColumn: "DbId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AyBorgProjects_ProjectSettingsRecord_SettingsDbId",
                table: "AyBorgProjects");

            migrationBuilder.DropTable(
                name: "ProjectSettingsRecord");

            migrationBuilder.DropIndex(
                name: "IX_AyBorgProjects_SettingsDbId",
                table: "AyBorgProjects");

            migrationBuilder.DropColumn(
                name: "SettingsDbId",
                table: "AyBorgProjects");
        }
    }
}
