using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aimbys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditVisibilityRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionPattern = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    VisibleToRolesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VisibleToPermissionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaskFieldsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiresComplianceMode = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditVisibilityRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationChannelConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChannelKey = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ConfigJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationChannelConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ChannelKey = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    MinimumSeverity = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TitleTemplate = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BodyTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultSeverity = table.Column<int>(type: "int", nullable: false),
                    DefaultRoutePattern = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplateTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    TitleTemplate = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BodyTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplateTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationTemplateTranslations_NotificationTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "NotificationTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_AuditVisibilityRules_ActionPattern",
                table: "AuditVisibilityRules",
                column: "ActionPattern",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_NotificationChannelConfigs_ChannelKey",
                table: "NotificationChannelConfigs",
                column: "ChannelKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_NotificationPreferences_UserId_ChannelKey",
                table: "NotificationPreferences",
                columns: new[] { "UserId", "ChannelKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_NotificationTemplates_Key",
                table: "NotificationTemplates",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_NotificationTemplateTranslations_TemplateId_LanguageCode",
                table: "NotificationTemplateTranslations",
                columns: new[] { "TemplateId", "LanguageCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditVisibilityRules");

            migrationBuilder.DropTable(
                name: "NotificationChannelConfigs");

            migrationBuilder.DropTable(
                name: "NotificationPreferences");

            migrationBuilder.DropTable(
                name: "NotificationTemplateTranslations");

            migrationBuilder.DropTable(
                name: "NotificationTemplates");
        }
    }
}
