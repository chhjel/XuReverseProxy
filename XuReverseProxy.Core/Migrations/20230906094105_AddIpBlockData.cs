using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XuReverseProxy.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddIpBlockData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlockedIpDatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RelatedClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IP = table.Column<string>(type: "text", nullable: true),
                    IPRegex = table.Column<string>(type: "text", nullable: true),
                    CidrRange = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    BlockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BlockedUntilUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedIpDatas", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockedIpDatas");
        }
    }
}
