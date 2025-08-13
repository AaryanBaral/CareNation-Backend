using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class Imperonate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FundContributions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ContributionDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Remarks = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundContributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundContributions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ImpersonationTickets",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(255)", nullable: false),
                    AdminId = table.Column<string>(type: "longtext", nullable: false),
                    TargetUserId = table.Column<string>(type: "longtext", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    Used = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Reason = table.Column<string>(type: "longtext", nullable: true),
                    ReturnUrl = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    UsedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImpersonationTickets", x => x.Code);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RewardPayouts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                    PayoutDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    MilestoneAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RankLabel = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    RewardItem = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    RoyaltyAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    TravelFundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    CarFundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    HouseFundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardPayouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RewardPayouts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SystemCounters",
                columns: table => new
                {
                    Name = table.Column<string>(type: "varchar(255)", nullable: false),
                    NextValue = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemCounters", x => x.Name);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TeamSalesProgress",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                    LeftTeamSales = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    RightTeamSales = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    MatchedVolumeConsumed = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamSalesProgress", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_TeamSalesProgress_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_FundContributions_UserId_ContributionDate",
                table: "FundContributions",
                columns: new[] { "UserId", "ContributionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FundContributions_UserId_Type_ContributionDate",
                table: "FundContributions",
                columns: new[] { "UserId", "Type", "ContributionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Reward_MilestoneOnce",
                table: "RewardPayouts",
                columns: new[] { "UserId", "MilestoneAmount" });

            migrationBuilder.CreateIndex(
                name: "IX_RewardPayouts_UserId_PayoutDate",
                table: "RewardPayouts",
                columns: new[] { "UserId", "PayoutDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FundContributions");

            migrationBuilder.DropTable(
                name: "ImpersonationTickets");

            migrationBuilder.DropTable(
                name: "RewardPayouts");

            migrationBuilder.DropTable(
                name: "SystemCounters");

            migrationBuilder.DropTable(
                name: "TeamSalesProgress");
        }
    }
}
