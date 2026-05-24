using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aimbys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnterpriseInfrastructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Institutes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "Institutes",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RestoredAtUtc",
                table: "Institutes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionExpiresAtUtc",
                table: "Institutes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubscriptionStatus",
                table: "Institutes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "FileAssets",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RestoredAtUtc",
                table: "FileAssets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ArchivePolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Strategy = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivePolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeatureToggles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsEnabledGlobally = table.Column<bool>(type: "bit", nullable: false),
                    InstituteOverridesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureToggles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstituteSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstituteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ValueJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstituteSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstituteSettings_Institutes_InstituteId",
                        column: x => x.InstituteId,
                        principalTable: "Institutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlatformSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ValueJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RetentionPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    RetentionDays = table.Column<int>(type: "int", nullable: false),
                    ArchiveAfterDays = table.Column<int>(type: "int", nullable: false),
                    LegalHold = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetentionPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobKey = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CronExpression = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    NextRunAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastRunAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    MaxRetries = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledJobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Institutes_SubscriptionStatus",
                table: "Institutes",
                column: "SubscriptionStatus");

            migrationBuilder.CreateIndex(
                name: "UX_ArchivePolicies_EntityType",
                table: "ArchivePolicies",
                column: "EntityType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_FeatureToggles_Key",
                table: "FeatureToggles",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_InstituteSettings_InstituteId_Key",
                table: "InstituteSettings",
                columns: new[] { "InstituteId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_PlatformSettings_Key",
                table: "PlatformSettings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_RetentionPolicies_EntityType",
                table: "RetentionPolicies",
                column: "EntityType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJobs_JobKey",
                table: "ScheduledJobs",
                column: "JobKey");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJobs_Status_NextRunAtUtc",
                table: "ScheduledJobs",
                columns: new[] { "Status", "NextRunAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchivePolicies");

            migrationBuilder.DropTable(
                name: "FeatureToggles");

            migrationBuilder.DropTable(
                name: "InstituteSettings");

            migrationBuilder.DropTable(
                name: "PlatformSettings");

            migrationBuilder.DropTable(
                name: "RetentionPolicies");

            migrationBuilder.DropTable(
                name: "ScheduledJobs");

            migrationBuilder.DropIndex(
                name: "IX_Institutes_SubscriptionStatus",
                table: "Institutes");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "Institutes");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Institutes");

            migrationBuilder.DropColumn(
                name: "RestoredAtUtc",
                table: "Institutes");

            migrationBuilder.DropColumn(
                name: "SubscriptionExpiresAtUtc",
                table: "Institutes");

            migrationBuilder.DropColumn(
                name: "SubscriptionStatus",
                table: "Institutes");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "FileAssets");

            migrationBuilder.DropColumn(
                name: "RestoredAtUtc",
                table: "FileAssets");
        }
    }
}
