using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AyBorg.Data.Agent.Migrations.SqlLite
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AyBorgProjects",
                columns: table => new
                {
                    DbId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AyBorgProjects", x => x.DbId);
                });

            migrationBuilder.CreateTable(
                name: "PluginMetaInfo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AssemblyName = table.Column<string>(type: "TEXT", nullable: false),
                    AssemblyVersion = table.Column<string>(type: "TEXT", nullable: false),
                    TypeName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluginMetaInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AyBorgLinks",
                columns: table => new
                {
                    DbId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectRecordId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AyBorgLinks", x => x.DbId);
                    table.ForeignKey(
                        name: "FK_AyBorgLinks_AyBorgProjects_ProjectRecordId",
                        column: x => x.ProjectRecordId,
                        principalTable: "AyBorgProjects",
                        principalColumn: "DbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AyBorgProjectMetas",
                columns: table => new
                {
                    DbId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    ServiceUniqueName = table.Column<string>(type: "TEXT", nullable: false),
                    ProjectRecordId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AyBorgProjectMetas", x => x.DbId);
                    table.ForeignKey(
                        name: "FK_AyBorgProjectMetas_AyBorgProjects_ProjectRecordId",
                        column: x => x.ProjectRecordId,
                        principalTable: "AyBorgProjects",
                        principalColumn: "DbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AyBorgSteps",
                columns: table => new
                {
                    DbId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    MetaInfoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    X = table.Column<int>(type: "INTEGER", nullable: false),
                    Y = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectRecordId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AyBorgSteps", x => x.DbId);
                    table.ForeignKey(
                        name: "FK_AyBorgSteps_AyBorgProjects_ProjectRecordId",
                        column: x => x.ProjectRecordId,
                        principalTable: "AyBorgProjects",
                        principalColumn: "DbId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AyBorgSteps_PluginMetaInfo_MetaInfoId",
                        column: x => x.MetaInfoId,
                        principalTable: "PluginMetaInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AyBorgPorts",
                columns: table => new
                {
                    DbId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Direction = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    Brand = table.Column<int>(type: "INTEGER", nullable: false),
                    StepRecordId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AyBorgPorts", x => x.DbId);
                    table.ForeignKey(
                        name: "FK_AyBorgPorts_AyBorgSteps_StepRecordId",
                        column: x => x.StepRecordId,
                        principalTable: "AyBorgSteps",
                        principalColumn: "DbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AyBorgLinks_ProjectRecordId",
                table: "AyBorgLinks",
                column: "ProjectRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_AyBorgPorts_StepRecordId",
                table: "AyBorgPorts",
                column: "StepRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_AyBorgProjectMetas_ProjectRecordId",
                table: "AyBorgProjectMetas",
                column: "ProjectRecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AyBorgSteps_MetaInfoId",
                table: "AyBorgSteps",
                column: "MetaInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_AyBorgSteps_ProjectRecordId",
                table: "AyBorgSteps",
                column: "ProjectRecordId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AyBorgLinks");

            migrationBuilder.DropTable(
                name: "AyBorgPorts");

            migrationBuilder.DropTable(
                name: "AyBorgProjectMetas");

            migrationBuilder.DropTable(
                name: "AyBorgSteps");

            migrationBuilder.DropTable(
                name: "AyBorgProjects");

            migrationBuilder.DropTable(
                name: "PluginMetaInfo");
        }
    }
}
