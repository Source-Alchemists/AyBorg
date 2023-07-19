using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AyBorg.Data.Agent.Migrations.SqlLite.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AyBorgProjectSettings",
                columns: table => new
                {
                    DbId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsForceResultCommunicationEnabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AyBorgProjectSettings", x => x.DbId);
                });

            migrationBuilder.CreateTable(
                name: "PluginMetaInfoRecord",
                columns: table => new
                {
                    DbId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AssemblyName = table.Column<string>(type: "TEXT", nullable: false),
                    AssemblyVersion = table.Column<string>(type: "TEXT", nullable: false),
                    TypeName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluginMetaInfoRecord", x => x.DbId);
                });

            migrationBuilder.CreateTable(
                name: "AyBorgProjects",
                columns: table => new
                {
                    DbId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SettingsDbId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AyBorgProjects", x => x.DbId);
                    table.ForeignKey(
                        name: "FK_AyBorgProjects_AyBorgProjectSettings_SettingsDbId",
                        column: x => x.SettingsDbId,
                        principalTable: "AyBorgProjectSettings",
                        principalColumn: "DbId",
                        onDelete: ReferentialAction.Cascade);
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    ServiceUniqueName = table.Column<string>(type: "TEXT", nullable: false),
                    VersionName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    VersionIteration = table.Column<long>(type: "INTEGER", nullable: false),
                    ProjectRecordId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ApprovedBy = table.Column<string>(type: "TEXT", nullable: false)
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
                    MetaInfoDbId = table.Column<Guid>(type: "TEXT", nullable: false),
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
                        name: "FK_AyBorgSteps_PluginMetaInfoRecord_MetaInfoDbId",
                        column: x => x.MetaInfoDbId,
                        principalTable: "PluginMetaInfoRecord",
                        principalColumn: "DbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AyBorgPorts",
                columns: table => new
                {
                    DbId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StepRecordId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Direction = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    Brand = table.Column<int>(type: "INTEGER", nullable: false)
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
                name: "IX_AyBorgProjects_SettingsDbId",
                table: "AyBorgProjects",
                column: "SettingsDbId");

            migrationBuilder.CreateIndex(
                name: "IX_AyBorgSteps_MetaInfoDbId",
                table: "AyBorgSteps",
                column: "MetaInfoDbId");

            migrationBuilder.CreateIndex(
                name: "IX_AyBorgSteps_ProjectRecordId",
                table: "AyBorgSteps",
                column: "ProjectRecordId");
        }

        /// <inheritdoc />
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
                name: "PluginMetaInfoRecord");

            migrationBuilder.DropTable(
                name: "AyBorgProjectSettings");
        }
    }
}
