using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aimbys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEvaluationDesk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DraftScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriterionIndex = table.Column<int>(type: "int", nullable: false),
                    PointsAwarded = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SavedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftScores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EvaluatedScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalPointsAwarded = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false),
                    MaxPointsPossible = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EvaluatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    EvaluatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluatedScores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Evaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttemptAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluatorTeacherProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RubricScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriterionIndex = table.Column<int>(type: "int", nullable: false),
                    PointsAwarded = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    MaxPoints = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RubricScores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScoringSchemes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaperVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriteriaJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoringSchemes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UX_DraftScores_EvaluationId_CriterionIndex",
                table: "DraftScores",
                columns: new[] { "EvaluationId", "CriterionIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvaluatedScores_EvaluationId",
                table: "EvaluatedScores",
                column: "EvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_AttemptAnswerId",
                table: "Evaluations",
                column: "AttemptAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_EvaluatorTeacherProfileId_Status",
                table: "Evaluations",
                columns: new[] { "EvaluatorTeacherProfileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "UX_RubricScores_EvaluationId_CriterionIndex",
                table: "RubricScores",
                columns: new[] { "EvaluationId", "CriterionIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ScoringSchemes_PaperVersionId_QuestionId",
                table: "ScoringSchemes",
                columns: new[] { "PaperVersionId", "QuestionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DraftScores");

            migrationBuilder.DropTable(
                name: "EvaluatedScores");

            migrationBuilder.DropTable(
                name: "Evaluations");

            migrationBuilder.DropTable(
                name: "RubricScores");

            migrationBuilder.DropTable(
                name: "ScoringSchemes");
        }
    }
}
