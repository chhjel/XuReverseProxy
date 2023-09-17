using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XuReverseProxy.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationRuleDistinctPattern : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CooldownDistinctPattern",
                table: "NotificationRules",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CooldownDistinctPattern",
                table: "NotificationRules");
        }
    }
}
