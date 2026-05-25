using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aimbys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionAuthoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuestionAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionAssets", x => x.Id);
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
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstituteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuthorTeacherProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    BodyHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Difficulty = table.Column<int>(type: "int", nullable: false),
                    BloomLevel = table.Column<int>(type: "int", nullable: false),
                    Marks = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    EstimatedTimeMinutes = table.Column<int>(type: "int", nullable: true),
                    InstructionsHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCurrentVersion = table.Column<bool>(type: "bit", nullable: false),
                    AuthorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionVersions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionOptions_QuestionVersions_VersionId",
                        column: x => x.VersionId,
                        principalTable: "QuestionVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionRubricCriteria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Criterion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MaxPoints = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionRubricCriteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionRubricCriteria_QuestionVersions_VersionId",
                        column: x => x.VersionId,
                        principalTable: "QuestionVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionTestCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Input = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedOutput = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsHidden = table.Column<bool>(type: "bit", nullable: false),
                    TimeoutMs = table.Column<int>(type: "int", nullable: false),
                    MemoryLimitMb = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionTestCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionTestCases_QuestionVersions_VersionId",
                        column: x => x.VersionId,
                        principalTable: "QuestionVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionAssets_FileAssetId",
                table: "QuestionAssets",
                column: "FileAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionAssets_QuestionId",
                table: "QuestionAssets",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionExposureLogs_InstituteId_ExposedAtUtc",
                table: "QuestionExposureLogs",
                columns: new[] { "InstituteId", "ExposedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionExposureLogs_QuestionId",
                table: "QuestionExposureLogs",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_VersionId",
                table: "QuestionOptions",
                column: "VersionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionRubricCriteria_VersionId",
                table: "QuestionRubricCriteria",
                column: "VersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_AuthorTeacherProfileId",
                table: "Questions",
                column: "AuthorTeacherProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_InstituteId",
                table: "Questions",
                column: "InstituteId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_InstituteId_Status",
                table: "Questions",
                columns: new[] { "InstituteId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_SubjectId",
                table: "Questions",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTestCases_VersionId",
                table: "QuestionTestCases",
                column: "VersionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionVersions_QuestionId_IsCurrentVersion",
                table: "QuestionVersions",
                columns: new[] { "QuestionId", "IsCurrentVersion" });

            migrationBuilder.CreateIndex(
                name: "UX_QuestionVersions_QuestionId_VersionNumber",
                table: "QuestionVersions",
                columns: new[] { "QuestionId", "VersionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionAssets");

            migrationBuilder.DropTable(
                name: "QuestionExposureLogs");

            migrationBuilder.DropTable(
                name: "QuestionOptions");

            migrationBuilder.DropTable(
                name: "QuestionRubricCriteria");

            migrationBuilder.DropTable(
                name: "QuestionTestCases");

            migrationBuilder.DropTable(
                name: "QuestionVersions");

            migrationBuilder.DropTable(
                name: "Questions");
        }
    }
}
