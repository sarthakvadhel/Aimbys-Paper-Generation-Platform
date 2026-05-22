using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aimbys.Infrastructure.Migrations
{
    /// <summary>
    /// Refresh of <c>TeacherProfiles</c> permission columns to the canonical
    /// thirteen flags. The PR #7 baseline shipped six provisional flags
    /// (<c>CanAuthorQuestions</c>, <c>CanGeneratePapers</c>, <c>CanEvaluate</c>,
    /// <c>CanModerate</c>, <c>CanReview</c>, <c>CanProctor</c>); three of those
    /// (<c>CanEvaluate</c> / <c>CanModerate</c> / <c>CanProctor</c>) survived the
    /// rename because their semantics matched the canonical list. The other
    /// three are dropped and ten new flags are added.
    ///
    /// Drops are intentional: a <c>CanReview = true</c> value in the
    /// provisional model is <strong>not</strong> the same as
    /// <c>CanApproveQuestions = true</c> in the canonical model, so we do not
    /// rename across them. Any pre-Chunk-8 dev databases lose those three
    /// values; new permissions are re-assigned by an Institute Admin via
    /// Chunk 17. No production database existed at this point.
    /// </summary>
    public partial class AddTeacherPermissionsRefresh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- Drop the three retired provisional flags --------------------
            migrationBuilder.DropColumn(
                name: "CanAuthorQuestions",
                table: "TeacherProfiles");

            migrationBuilder.DropColumn(
                name: "CanGeneratePapers",
                table: "TeacherProfiles");

            migrationBuilder.DropColumn(
                name: "CanReview",
                table: "TeacherProfiles");

            // --- Add the ten new canonical flags -----------------------------
            // (CanEvaluate / CanModerate / CanProctor already exist from PR #7
            //  and are intentionally untouched.)
            migrationBuilder.AddColumn<bool>(
                name: "CanCreateQuestions",
                table: "TeacherProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanGeneratePaper",
                table: "TeacherProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageBlueprints",
                table: "TeacherProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanPublishResults",
                table: "TeacherProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanScheduleExam",
                table: "TeacherProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanReviewCodingQuestions",
                table: "TeacherProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageQuestionBank",
                table: "TeacherProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanAssignEvaluators",
                table: "TeacherProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageAnalytics",
                table: "TeacherProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanApproveQuestions",
                table: "TeacherProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the ten canonical flags …
            migrationBuilder.DropColumn(name: "CanCreateQuestions",       table: "TeacherProfiles");
            migrationBuilder.DropColumn(name: "CanGeneratePaper",         table: "TeacherProfiles");
            migrationBuilder.DropColumn(name: "CanManageBlueprints",      table: "TeacherProfiles");
            migrationBuilder.DropColumn(name: "CanPublishResults",        table: "TeacherProfiles");
            migrationBuilder.DropColumn(name: "CanScheduleExam",          table: "TeacherProfiles");
            migrationBuilder.DropColumn(name: "CanReviewCodingQuestions", table: "TeacherProfiles");
            migrationBuilder.DropColumn(name: "CanManageQuestionBank",    table: "TeacherProfiles");
            migrationBuilder.DropColumn(name: "CanAssignEvaluators",      table: "TeacherProfiles");
            migrationBuilder.DropColumn(name: "CanManageAnalytics",       table: "TeacherProfiles");
            migrationBuilder.DropColumn(name: "CanApproveQuestions",      table: "TeacherProfiles");

            // … and put the three provisional flags back so we can fall back to
            // the PR #7 schema. CanAuthorQuestions defaulted to true in the
            // provisional model; preserve that here for parity.
            migrationBuilder.AddColumn<bool>(
                name: "CanAuthorQuestions",
                table: "TeacherProfiles",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanGeneratePapers",
                table: "TeacherProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanReview",
                table: "TeacherProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
