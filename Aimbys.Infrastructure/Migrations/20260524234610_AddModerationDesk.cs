using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aimbys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModerationDesk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModeratedScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModerationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalPointsAwarded = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false),
                    MaxPointsPossible = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false),
                    ModeratedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ModeratedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsOverride = table.Column<bool>(type: "bit", nullable: false),
                    OverrideReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModeratedScores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Moderations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModeratorTeacherProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Verdict = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OverrideReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AssignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Moderations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModerationSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModerationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluatorScoresJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CapturedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationSnapshots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModeratedScores_EvaluationId",
                table: "ModeratedScores",
                column: "EvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_ModeratedScores_ModerationId",
                table: "ModeratedScores",
                column: "ModerationId");

            migrationBuilder.CreateIndex(
                name: "IX_Moderations_EvaluationId",
                table: "Moderations",
                column: "EvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_Moderations_ModeratorTeacherProfileId_Verdict",
                table: "Moderations",
                columns: new[] { "ModeratorTeacherProfileId", "Verdict" });

            migrationBuilder.CreateIndex(
                name: "UX_ModerationSnapshots_ModerationId",
                table: "ModerationSnapshots",
                column: "ModerationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModeratedScores");

            migrationBuilder.DropTable(
                name: "Moderations");

            migrationBuilder.DropTable(
                name: "ModerationSnapshots");
        }
    }
}
