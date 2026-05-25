using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aimbys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCodingExam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CodeExecutionQueues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    EnqueuedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkerIdentifier = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeExecutionQueues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CodingSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamAttemptAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SourceCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExecutionStatus = table.Column<int>(type: "int", nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalTestCases = table.Column<int>(type: "int", nullable: true),
                    PassedTestCases = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodingSubmissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CodingTestCaseResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Passed = table.Column<bool>(type: "bit", nullable: false),
                    ActualOutput = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpectedOutput = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExecutionTimeMs = table.Column<int>(type: "int", nullable: false),
                    MemoryUsedKb = table.Column<int>(type: "int", nullable: false),
                    ErrorOutput = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodingTestCaseResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodingTestCaseResults_CodingSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "CodingSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodeExecutionQueues_Priority_EnqueuedAtUtc",
                table: "CodeExecutionQueues",
                columns: new[] { "Priority", "EnqueuedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CodingSubmissions_ExamAttemptAnswerId",
                table: "CodingSubmissions",
                column: "ExamAttemptAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_CodingTestCaseResults_SubmissionId_TestCaseId",
                table: "CodingTestCaseResults",
                columns: new[] { "SubmissionId", "TestCaseId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodeExecutionQueues");

            migrationBuilder.DropTable(
                name: "CodingTestCaseResults");

            migrationBuilder.DropTable(
                name: "CodingSubmissions");
        }
    }
}
