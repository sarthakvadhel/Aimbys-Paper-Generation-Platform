using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aimbys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AggregatedAnalyticsTables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Scope = table.Column<int>(type: "int", nullable: false),
                    ScopeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DimensionKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DimensionValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MetricJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ComputedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AggregatedAnalyticsTables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnalyticsSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Scope = table.Column<int>(type: "int", nullable: false),
                    ScopeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MetricKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MetricValueJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CapturedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CachedLeaderboardEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    Percentile = table.Column<double>(type: "float", nullable: false),
                    TotalScore = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false),
                    ComputedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedLeaderboardEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AggregatedAnalyticsTables_Scope_ScopeId_DimensionKey_PeriodStart",
                table: "AggregatedAnalyticsTables",
                columns: new[] { "Scope", "ScopeId", "DimensionKey", "PeriodStart" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsSnapshots_Scope_ScopeId_MetricKey_CapturedAtUtc",
                table: "AnalyticsSnapshots",
                columns: new[] { "Scope", "ScopeId", "MetricKey", "CapturedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CachedLeaderboardEntries_ExamId_ClassBatchId_Rank",
                table: "CachedLeaderboardEntries",
                columns: new[] { "ExamId", "ClassBatchId", "Rank" });

            migrationBuilder.CreateIndex(
                name: "IX_CachedLeaderboardEntries_StudentProfileId",
                table: "CachedLeaderboardEntries",
                column: "StudentProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AggregatedAnalyticsTables");

            migrationBuilder.DropTable(
                name: "AnalyticsSnapshots");

            migrationBuilder.DropTable(
                name: "CachedLeaderboardEntries");
        }
    }
}
