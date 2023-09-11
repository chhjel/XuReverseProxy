using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XuReverseProxy.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminAuditLogEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IP = table.Column<string>(type: "text", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: true),
                    RelatedProxyConfigId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedProxyConfigName = table.Column<string>(type: "text", nullable: true),
                    RelatedClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedClientName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminAuditLogEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientAuditLogEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    IP = table.Column<string>(type: "text", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: true),
                    RelatedProxyConfigId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedProxyConfigName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientAuditLogEntries", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminAuditLogEntries");

            migrationBuilder.DropTable(
                name: "ClientAuditLogEntries");
        }
    }
}
