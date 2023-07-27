using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AyBorg.Data.Agent.Migrations.PostgreSql.Migrations
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
                    DbId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsForceResultCommunicationEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AyBorgProjectSettings", x => x.DbId);
                });

            migrationBuilder.CreateTable(
                name: "AyBorgStepPluginMetaInfo",
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
                    table.PrimaryKey("PK_AyBorgStepPluginMetaInfo", x => x.DbId);
                });

            migrationBuilder.CreateTable(
                name: "AyBorgProjects",
                columns: table => new
                {
                    DbId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettingsDbId = table.Column<Guid>(type: "uuid", nullable: false)
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
                    DbId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectRecordId = table.Column<Guid>(type: "uuid", nullable: false)
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
                    DbId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    ServiceUniqueName = table.Column<string>(type: "text", nullable: false),
                    VersionName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Comment = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VersionIteration = table.Column<long>(type: "bigint", nullable: false),
                    ProjectRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedBy = table.Column<string>(type: "text", nullable: false)
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
                    DbId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MetaInfoDbId = table.Column<Guid>(type: "uuid", nullable: false),
                    X = table.Column<int>(type: "integer", nullable: false),
                    Y = table.Column<int>(type: "integer", nullable: false),
                    ProjectRecordId = table.Column<Guid>(type: "uuid", nullable: false)
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
                        name: "FK_AyBorgSteps_AyBorgStepPluginMetaInfo_MetaInfoDbId",
                        column: x => x.MetaInfoDbId,
                        principalTable: "AyBorgStepPluginMetaInfo",
                        principalColumn: "DbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AyBorgPorts",
                columns: table => new
                {
                    DbId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true),
                    Brand = table.Column<int>(type: "integer", nullable: false)
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
                name: "AyBorgStepPluginMetaInfo");

            migrationBuilder.DropTable(
                name: "AyBorgProjectSettings");
        }
    }
}
