using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aimbys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionAnalytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuestionDifficultyAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthoredDifficulty = table.Column<int>(type: "int", nullable: false),
                    ComputedDifficulty = table.Column<int>(type: "int", nullable: false),
                    DriftDirection = table.Column<int>(type: "int", nullable: false),
                    ConfidencePercent = table.Column<double>(type: "float(5)", precision: 5, scale: 2, nullable: false),
                    SampleSize = table.Column<int>(type: "int", nullable: false),
                    ComputedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionDifficultyAudits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionExposureLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaperId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstituteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExposedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionExposureLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionUsageAnalytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PapersUsedIn = table.Column<int>(type: "int", nullable: false),
                    AttemptsCount = table.Column<int>(type: "int", nullable: false),
                    MeanTimeSeconds = table.Column<double>(type: "float", nullable: false),
                    PValue = table.Column<double>(type: "float(5)", precision: 5, scale: 4, nullable: false),
                    DiscriminationIndex = table.Column<double>(type: "float(5)", precision: 5, scale: 4, nullable: false),
                    ComputedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionUsageAnalytics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionDifficultyAudits_QuestionId_ComputedAtUtc",
                table: "QuestionDifficultyAudits",
                columns: new[] { "QuestionId", "ComputedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionExposureLogs_PaperId",
                table: "QuestionExposureLogs",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionExposureLogs_QuestionId_InstituteId",
                table: "QuestionExposureLogs",
                columns: new[] { "QuestionId", "InstituteId" });

            migrationBuilder.CreateIndex(
                name: "UX_QuestionUsageAnalytics_QuestionId_AcademicYearId",
                table: "QuestionUsageAnalytics",
                columns: new[] { "QuestionId", "AcademicYearId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionDifficultyAudits");

            migrationBuilder.DropTable(
                name: "QuestionExposureLogs");

            migrationBuilder.DropTable(
                name: "QuestionUsageAnalytics");
        }
    }
}
