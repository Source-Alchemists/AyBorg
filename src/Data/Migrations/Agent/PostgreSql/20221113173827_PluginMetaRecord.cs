using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AyBorg.Data.Agent.Migrations.PostgreSQL
{
    /// <inheritdoc />
    public partial class PluginMetaRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AyBorgSteps_PluginMetaInfo_MetaInfoId",
                table: "AyBorgSteps");

            migrationBuilder.DropTable(
                name: "PluginMetaInfo");

            migrationBuilder.RenameColumn(
                name: "MetaInfoId",
                table: "AyBorgSteps",
                newName: "MetaInfoDbId");

            migrationBuilder.RenameIndex(
                name: "IX_AyBorgSteps_MetaInfoId",
                table: "AyBorgSteps",
                newName: "IX_AyBorgSteps_MetaInfoDbId");

            migrationBuilder.CreateTable(
                name: "PluginMetaInfoRecord",
                columns: table => new
                {
                    DbId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssemblyName = table.Column<string>(type: "text", nullable: false),
                    AssemblyVersion = table.Column<string>(type: "text", nullable: false),
                    TypeName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluginMetaInfoRecord", x => x.DbId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_AyBorgSteps_PluginMetaInfoRecord_MetaInfoDbId",
                table: "AyBorgSteps",
                column: "MetaInfoDbId",
                principalTable: "PluginMetaInfoRecord",
                principalColumn: "DbId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AyBorgSteps_PluginMetaInfoRecord_MetaInfoDbId",
                table: "AyBorgSteps");

            migrationBuilder.DropTable(
                name: "PluginMetaInfoRecord");

            migrationBuilder.RenameColumn(
                name: "MetaInfoDbId",
                table: "AyBorgSteps",
                newName: "MetaInfoId");

            migrationBuilder.RenameIndex(
                name: "IX_AyBorgSteps_MetaInfoDbId",
                table: "AyBorgSteps",
                newName: "IX_AyBorgSteps_MetaInfoId");

            migrationBuilder.CreateTable(
                name: "PluginMetaInfo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssemblyName = table.Column<string>(type: "text", nullable: false),
                    AssemblyVersion = table.Column<string>(type: "text", nullable: false),
                    TypeName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluginMetaInfo", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_AyBorgSteps_PluginMetaInfo_MetaInfoId",
                table: "AyBorgSteps",
                column: "MetaInfoId",
                principalTable: "PluginMetaInfo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
