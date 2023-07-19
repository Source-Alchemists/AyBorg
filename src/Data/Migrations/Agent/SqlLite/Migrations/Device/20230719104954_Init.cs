using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AyBorg.Data.Agent.Migrations.SqlLite.Migrations.Device
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AyBorgDevicePluginMetaInfo",
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
                    table.PrimaryKey("PK_AyBorgDevicePluginMetaInfo", x => x.DbId);
                });

            migrationBuilder.CreateTable(
                name: "AyBorgDevices",
                columns: table => new
                {
                    DbId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Id = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    MetaInfoDbId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ProviderMetaInfoDbId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AyBorgDevices", x => x.DbId);
                    table.ForeignKey(
                        name: "FK_AyBorgDevices_AyBorgDevicePluginMetaInfo_MetaInfoDbId",
                        column: x => x.MetaInfoDbId,
                        principalTable: "AyBorgDevicePluginMetaInfo",
                        principalColumn: "DbId");
                    table.ForeignKey(
                        name: "FK_AyBorgDevices_AyBorgDevicePluginMetaInfo_ProviderMetaInfoDbId",
                        column: x => x.ProviderMetaInfoDbId,
                        principalTable: "AyBorgDevicePluginMetaInfo",
                        principalColumn: "DbId");
                });

            migrationBuilder.CreateTable(
                name: "AyBorgDevicePorts",
                columns: table => new
                {
                    DbId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceRecordId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Direction = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    Brand = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AyBorgDevicePorts", x => x.DbId);
                    table.ForeignKey(
                        name: "FK_AyBorgDevicePorts_AyBorgDevices_DeviceRecordId",
                        column: x => x.DeviceRecordId,
                        principalTable: "AyBorgDevices",
                        principalColumn: "DbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AyBorgDevicePorts_DeviceRecordId",
                table: "AyBorgDevicePorts",
                column: "DeviceRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_AyBorgDevices_MetaInfoDbId",
                table: "AyBorgDevices",
                column: "MetaInfoDbId");

            migrationBuilder.CreateIndex(
                name: "IX_AyBorgDevices_ProviderMetaInfoDbId",
                table: "AyBorgDevices",
                column: "ProviderMetaInfoDbId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AyBorgDevicePorts");

            migrationBuilder.DropTable(
                name: "AyBorgDevices");

            migrationBuilder.DropTable(
                name: "AyBorgDevicePluginMetaInfo");
        }
    }
}
