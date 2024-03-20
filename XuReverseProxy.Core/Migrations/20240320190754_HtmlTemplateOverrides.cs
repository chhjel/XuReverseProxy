using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XuReverseProxy.Core.Migrations
{
    /// <inheritdoc />
    public partial class HtmlTemplateOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProxyConfigId",
                table: "HtmlTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HtmlTemplates_ProxyConfigId",
                table: "HtmlTemplates",
                column: "ProxyConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_HtmlTemplates_ProxyConfigs_ProxyConfigId",
                table: "HtmlTemplates",
                column: "ProxyConfigId",
                principalTable: "ProxyConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HtmlTemplates_ProxyConfigs_ProxyConfigId",
                table: "HtmlTemplates");

            migrationBuilder.DropIndex(
                name: "IX_HtmlTemplates_ProxyConfigId",
                table: "HtmlTemplates");

            migrationBuilder.DropColumn(
                name: "ProxyConfigId",
                table: "HtmlTemplates");
        }
    }
}
