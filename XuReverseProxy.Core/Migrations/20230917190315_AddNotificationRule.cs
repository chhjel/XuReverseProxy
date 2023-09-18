using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XuReverseProxy.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    TriggerType = table.Column<int>(type: "integer", nullable: false),
                    AlertType = table.Column<int>(type: "integer", nullable: false),
                    WebHookUrl = table.Column<string>(type: "text", nullable: true),
                    WebHookMethod = table.Column<string>(type: "text", nullable: true),
                    WebHookHeaders = table.Column<string>(type: "text", nullable: true),
                    WebHookBody = table.Column<string>(type: "text", nullable: true),
                    CooldownDistinctPattern = table.Column<string>(type: "text", nullable: true),
                    Cooldown = table.Column<TimeSpan>(type: "interval", nullable: true),
                    LastNotifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastNotifyResult = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationRules", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationRules");
        }
    }
}
