using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace XuReverseProxy.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Discriminator = table.Column<string>(type: "text", nullable: false),
                    TOTPSecretKey = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProxyClientIdentities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IP = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastAttemptedAccessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastAccessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Blocked = table.Column<bool>(type: "boolean", nullable: false),
                    BlockedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BlockedMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProxyClientIdentities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProxyConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Subdomain = table.Column<string>(type: "text", nullable: true),
                    Port = table.Column<int>(type: "integer", nullable: true),
                    ChallengeTitle = table.Column<string>(type: "text", nullable: true),
                    ChallengeDescription = table.Column<string>(type: "text", nullable: true),
                    ShowCompletedChallenges = table.Column<bool>(type: "boolean", nullable: false),
                    ShowChallengesWithUnmetRequirements = table.Column<bool>(type: "boolean", nullable: false),
                    DestinationPrefix = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProxyConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RuntimeServerConfigItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "text", nullable: true),
                    LastUpdatedSourceIP = table.Column<string>(type: "text", nullable: true),
                    Key = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuntimeServerConfigItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecoveryCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RecoveryCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecoveryCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecoveryCodes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProxyClientIdentityDatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdentityId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthenticationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProxyClientIdentityDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProxyClientIdentityDatas_ProxyClientIdentities_IdentityId",
                        column: x => x.IdentityId,
                        principalTable: "ProxyClientIdentities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProxyClientIdentitySolvedChallengeDatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthenticationId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdentityId = table.Column<Guid>(type: "uuid", nullable: false),
                    SolvedId = table.Column<Guid>(type: "uuid", nullable: false),
                    SolvedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProxyClientIdentitySolvedChallengeDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProxyClientIdentitySolvedChallengeDatas_ProxyClientIdentiti~",
                        column: x => x.IdentityId,
                        principalTable: "ProxyClientIdentities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProxyAuthenticationDatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProxyConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    ChallengeTypeId = table.Column<string>(type: "text", nullable: true),
                    ChallengeJson = table.Column<string>(type: "text", nullable: true),
                    SolvedId = table.Column<Guid>(type: "uuid", nullable: false),
                    SolvedDuration = table.Column<TimeSpan>(type: "interval", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProxyAuthenticationDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProxyAuthenticationDatas_ProxyConfigs_ProxyConfigId",
                        column: x => x.ProxyConfigId,
                        principalTable: "ProxyConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProxyAuthenticationConditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthenticationDataId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConditionType = table.Column<int>(type: "integer", nullable: false),
                    DateTimeUtc1 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeUtc2 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimeOnlyUtc1 = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    TimeOnlyUtc2 = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    DaysOfWeekUtc = table.Column<int>(type: "integer", nullable: true)
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
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProxyAuthenticationConditions_AuthenticationDataId",
                table: "ProxyAuthenticationConditions",
                column: "AuthenticationDataId");

            migrationBuilder.CreateIndex(
                name: "IX_ProxyAuthenticationDatas_ProxyConfigId",
                table: "ProxyAuthenticationDatas",
                column: "ProxyConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_ProxyClientIdentityDatas_IdentityId",
                table: "ProxyClientIdentityDatas",
                column: "IdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_ProxyClientIdentitySolvedChallengeDatas_AuthenticationId",
                table: "ProxyClientIdentitySolvedChallengeDatas",
                column: "AuthenticationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProxyClientIdentitySolvedChallengeDatas_IdentityId",
                table: "ProxyClientIdentitySolvedChallengeDatas",
                column: "IdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_ProxyClientIdentitySolvedChallengeDatas_SolvedId",
                table: "ProxyClientIdentitySolvedChallengeDatas",
                column: "SolvedId");

            migrationBuilder.CreateIndex(
                name: "IX_RecoveryCodes_UserId",
                table: "RecoveryCodes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RuntimeServerConfigItems_Key",
                table: "RuntimeServerConfigItems",
                column: "Key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ProxyAuthenticationConditions");

            migrationBuilder.DropTable(
                name: "ProxyClientIdentityDatas");

            migrationBuilder.DropTable(
                name: "ProxyClientIdentitySolvedChallengeDatas");

            migrationBuilder.DropTable(
                name: "RecoveryCodes");

            migrationBuilder.DropTable(
                name: "RuntimeServerConfigItems");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "ProxyAuthenticationDatas");

            migrationBuilder.DropTable(
                name: "ProxyClientIdentities");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ProxyConfigs");
        }
    }
}
