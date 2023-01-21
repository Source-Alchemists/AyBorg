using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AyBorg.Data.Agent.Migrations.SqlLite
{
    /// <inheritdoc />
    public partial class ProjectMetaVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "AyBorgProjectMetas",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "VersionIteration",
                table: "AyBorgProjectMetas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "VersionName",
                table: "AyBorgProjectMetas",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "AyBorgProjectMetas");

            migrationBuilder.DropColumn(
                name: "VersionIteration",
                table: "AyBorgProjectMetas");

            migrationBuilder.DropColumn(
                name: "VersionName",
                table: "AyBorgProjectMetas");
        }
    }
}
