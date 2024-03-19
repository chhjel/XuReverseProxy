using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XuReverseProxy.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddHtmlTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConditionsNotMetMessage",
                table: "ProxyConfigs");

            migrationBuilder.CreateTable(
                name: "HtmlTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ResponseCode = table.Column<int>(type: "integer", nullable: false),
                    Html = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HtmlTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HtmlTemplates_Type",
                table: "HtmlTemplates",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HtmlTemplates");

            migrationBuilder.AddColumn<string>(
                name: "ConditionsNotMetMessage",
                table: "ProxyConfigs",
                type: "text",
                nullable: true);
        }
    }
}
