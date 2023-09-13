﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using XuReverseProxy.Core.Models.DbEntity;

#nullable disable

namespace XuReverseProxy.Core.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20230913184638_AddProxyConfigStaticHTML")]
    partial class AddProxyConfigStaticHTML
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("FriendlyName")
                        .HasColumnType("text");

                    b.Property<string>("Xml")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("DataProtectionKeys");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);

                    b.HasDiscriminator<string>("Discriminator").HasValue("IdentityUser");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .HasColumnType("text");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("XuReverseProxy.Core.Models.DbEntity.AdminAuditLogEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Action")
                        .HasColumnType("text");

                    b.Property<Guid>("AdminUserId")
                        .HasColumnType("uuid");

                    b.Property<string>("IP")
                        .HasColumnType("text");

                    b.Property<Guid?>("RelatedClientId")
                        .HasColumnType("uuid");

                    b.Property<string>("RelatedClientName")
                        .HasColumnType("text");

                    b.Property<Guid?>("RelatedProxyConfigId")
                        .HasColumnType("uuid");

                    b.Property<string>("RelatedProxyConfigName")
                        .HasColumnType("text");

                    b.Property<DateTime>("TimestampUtc")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("AdminAuditLogEntries");
                });

            modelBuilder.Entity("XuReverseProxy.Core.Models.DbEntity.ApplicationUserRecoveryCode", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("RecoveryCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("RecoveryCodes");
                });

            modelBuilder.Entity("XuReverseProxy.Core.Models.DbEntity.BlockedIpData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("BlockedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("BlockedUntilUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CidrRange")
                        .HasColumnType("text");

                    b.Property<string>("IP")
                        .HasColumnType("text");

                    b.Property<string>("IPRegex")
                        .HasColumnType("text");

                    b.Property<string>("Note")
                        .HasColumnType("text");

                    b.Property<Guid?>("RelatedClientId")
                        .HasColumnType("uuid");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("BlockedIpDatas");
                });

            modelBuilder.Entity("XuReverseProxy.Core.Models.DbEntity.ClientAuditLogEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Action")
                        .HasColumnType("text");

                    b.Property<Guid>("ClientId")
                        .HasColumnType("uuid");

                    b.Property<string>("ClientName")
                        .HasColumnType("text");

                    b.Property<string>("IP")
                        .HasColumnType("text");

                    b.Property<Guid?>("RelatedProxyConfigId")
                        .HasColumnType("uuid");

                    b.Property<string>("RelatedProxyConfigName")
                        .HasColumnType("text");

                    b.Property<DateTime>("TimestampUtc")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("ClientAuditLogEntries");
                });

            modelBuilder.Entity("XuReverseProxy.Core.Models.DbEntity.ProxyAuthenticationCondition", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AuthenticationDataId")
                        .HasColumnType("uuid");

                    b.Property<int>("ConditionType")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("DateTimeUtc1")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("DateTimeUtc2")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("DaysOfWeekUtc")
                        .HasColumnType("integer");

                    b.Property<TimeOnly?>("TimeOnlyUtc1")
                        .HasColumnType("time without time zone");

                    b.Property<TimeOnly?>("TimeOnlyUtc2")
                        .HasColumnType("time without time zone");

                    b.HasKey("Id");

                    b.HasIndex("AuthenticationDataId");

                    b.ToTable("ProxyAuthenticationConditions");
                });

            modelBuilder.Entity("XuReverseProxy.Core.Models.DbEntity.ProxyAuthenticationData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ChallengeJson")
                        .HasColumnType("text");

                    b.Property<string>("ChallengeTypeId")
                        .HasColumnType("text");

                    b.Property<int>("Order")
                        .HasColumnType("integer");

                    b.Property<Guid>("ProxyConfigId")
                        .HasColumnType("uuid");

                    b.Property<TimeSpan?>("SolvedDuration")
                        .HasColumnType("interval");

                    b.Property<Guid>("SolvedId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ProxyConfigId");

                    b.ToTable("ProxyAuthenticationDatas");
                });

            modelBuilder.Entity("XuReverseProxy.Core.Models.DbEntity.ProxyClientIdentity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<bool>("Blocked")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("BlockedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("BlockedMessage")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("IP")
                        .HasColumnType("text");

                    b.Property<DateTime?>("LastAccessedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("LastAttemptedAccessedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Note")
                        .HasColumnType("text");

                    b.Property<string>("UserAgent")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("ProxyClientIdentities");
                });

            modelBuilder.Entity("XuReverseProxy.Core.Models.DbEntity.ProxyClientIdentityData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("AuthenticationId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("IdentityId")
                        .HasColumnType("uuid");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("IdentityId");

                    b.ToTable("ProxyClientIdentityDatas");
                });

            modelBuilder.Entity("XuReverseProxy.Core.Models.DbEntity.ProxyClientIdentitySolvedChallengeData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AuthenticationId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("IdentityId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("SolvedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("SolvedId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("AuthenticationId");

                    b.HasIndex("IdentityId");

                    b.HasIndex("SolvedId");

                    b.ToTable("ProxyClientIdentitySolvedChallengeDatas");
                });

            modelBuilder.Entity("XuReverseProxy.Core.Models.DbEntity.ProxyConfig", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ChallengeDescription")
                        .HasColumnType("text");

                    b.Property<string>("ChallengeTitle")
                        .HasColumnType("text");

                    b.Property<string>("DestinationPrefix")
                        .HasColumnType("text");

                    b.Property<bool>("Enabled")
                        .HasColumnType("boolean");

                    b.Property<int>("Mode")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<int?>("Port")
                        .HasColumnType("integer");

                    b.Property<bool>("ShowChallengesWithUnmetRequirements")
                        .HasColumnType("boolean");

                    b.Property<bool>("ShowCompletedChallenges")
                        .HasColumnType("boolean");

                    b.Property<string>("StaticHTML")
                        .HasColumnType("text");

                    b.Property<string>("Subdomain")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("ProxyConfigs");
                });

            modelBuilder.Entity("XuReverseProxy.Core.Models.DbEntity.RuntimeServerConfigItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Key")
                        .HasColumnType("text");

                    b.Property<DateTime>("LastUpdatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LastUpdatedBy")
                        .HasColumnType("text");

                    b.Property<string>("LastUpdatedSourceIP")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Key");

                    b.ToTable("RuntimeServerConfigItems");
                });

            modelBuilder.Entity("XuReverseProxy.Core.Models.DbEntity.ApplicationUser", b =>
                {
                    b.HasBaseType("Microsoft.AspNetCore.Identity.IdentityUser");

                    b.Property<string>("LastConnectedFromIP")
                        .HasColumnType("text");

                    b.Property<string>("TOTPSecretKey")
                        .HasColumnType("text");

                    b.HasDiscriminator().HasValue("ApplicationUser");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("XuReverseProxy.Core.Models.DbEntity.ApplicationUserRecoveryCode", b =>
                {
                    b.HasOne("XuReverseProxy.Core.Models.DbEntity.ApplicationUser", "User")
                        .WithMany("RecoveryCodes")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("XuReverseProxy.Core.Models.DbEntity.ProxyAuthenticationCondition", b =>
                {
                    b.HasOne("XuReverseProxy.Core.Models.DbEntity.ProxyAuthenticationData", "AuthenticationData")
                        .WithMany("Conditions")
                        .HasForeignKey("AuthenticationDataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AuthenticationData");
                });

            modelBuilder.Entity("XuReverseProxy.Core.Models.DbEntity.ProxyAuthenticationData", b =>
                {
                    b.HasOne("XuReverseProxy.Core.Models.DbEntity.ProxyConfig", "ProxyConfig")
                        .WithMany("Authentications")
                        .HasForeignKey("ProxyConfigId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ProxyConfig");
                });

            modelBuilder.Entity("XuReverseProxy.Core.Models.DbEntity.ProxyClientIdentityData", b =>
                {
                    b.HasOne("XuReverseProxy.Core.Models.DbEntity.ProxyClientIdentity", "Identity")
                        .WithMany("Data")
                        .HasForeignKey("IdentityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Identity");
                });

            modelBuilder.Entity("XuReverseProxy.Core.Models.DbEntity.ProxyClientIdentitySolvedChallengeData", b =>
                {
                    b.HasOne("XuReverseProxy.Core.Models.DbEntity.ProxyClientIdentity", "Identity")
                        .WithMany("SolvedChallenges")
                        .HasForeignKey("IdentityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Identity");
                });

            modelBuilder.Entity("XuReverseProxy.Core.Models.DbEntity.ProxyAuthenticationData", b =>
                {
                    b.Navigation("Conditions");
                });

            modelBuilder.Entity("XuReverseProxy.Core.Models.DbEntity.ProxyClientIdentity", b =>
                {
                    b.Navigation("Data");

                    b.Navigation("SolvedChallenges");
                });

            modelBuilder.Entity("XuReverseProxy.Core.Models.DbEntity.ProxyConfig", b =>
                {
                    b.Navigation("Authentications");
                });

            modelBuilder.Entity("XuReverseProxy.Core.Models.DbEntity.ApplicationUser", b =>
                {
                    b.Navigation("RecoveryCodes");
                });
#pragma warning restore 612, 618
        }
    }
}
