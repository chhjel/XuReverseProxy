using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XuReverseProxy.Core.Migrations
{
    /// <inheritdoc />
    public partial class RefactorConditions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProxyAuthenticationConditions");

            migrationBuilder.AddColumn<string>(
                name: "ConditionsNotMetMessage",
                table: "ProxyConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowConditionsNotMet",
                table: "ProxyConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ConditionData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Group = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    DateTimeUtc1 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeUtc2 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimeOnlyUtc1 = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    TimeOnlyUtc2 = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    DaysOfWeekUtc = table.Column<int>(type: "integer", nullable: true),
                    IPCondition = table.Column<string>(type: "text", nullable: true),
                    ProxyAuthenticationDataId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProxyConfigId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConditionData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConditionData_ProxyAuthenticationDatas_ProxyAuthenticationD~",
                        column: x => x.ProxyAuthenticationDataId,
                        principalTable: "ProxyAuthenticationDatas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConditionData_ProxyConfigs_ProxyConfigId",
                        column: x => x.ProxyConfigId,
                        principalTable: "ProxyConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConditionData_ProxyAuthenticationDataId",
                table: "ConditionData",
                column: "ProxyAuthenticationDataId");

            migrationBuilder.CreateIndex(
                name: "IX_ConditionData_ProxyConfigId",
                table: "ConditionData",
                column: "ProxyConfigId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConditionData");

            migrationBuilder.DropColumn(
                name: "ConditionsNotMetMessage",
                table: "ProxyConfigs");

            migrationBuilder.DropColumn(
                name: "ShowConditionsNotMet",
                table: "ProxyConfigs");

            migrationBuilder.CreateTable(
                name: "ProxyAuthenticationConditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthenticationDataId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConditionType = table.Column<int>(type: "integer", nullable: false),
                    DateTimeUtc1 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeUtc2 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DaysOfWeekUtc = table.Column<int>(type: "integer", nullable: true),
                    TimeOnlyUtc1 = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    TimeOnlyUtc2 = table.Column<TimeOnly>(type: "time without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProxyAuthenticationConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProxyAuthenticationConditions_ProxyAuthenticationDatas_Auth~",
                        column: x => x.AuthenticationDataId,
                        principalTable: "ProxyAuthenticationDatas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProxyAuthenticationConditions_AuthenticationDataId",
                table: "ProxyAuthenticationConditions",
                column: "AuthenticationDataId");
        }
    }
}
