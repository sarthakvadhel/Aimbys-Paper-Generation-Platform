using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aimbys.Infrastructure.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Chunk 35 — SuperAdmin governance. Adds the <c>Broadcasts</c>
    /// table consumed by the broadcast banner partial and the
    /// SuperAdmin governance screens.
    ///
    /// <para>
    /// This migration also restores <c>AppDbContextModelSnapshot.cs</c>,
    /// which was inadvertently deleted in the chunk 34 commit
    /// (<c>bdf36f5</c>). The snapshot is regenerated fresh from the
    /// current model so future <c>dotnet ef migrations add</c> calls
    /// produce clean deltas instead of recreating the entire schema.
    /// </para>
    /// </remarks>
    public partial class AddBroadcasts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Broadcasts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BodyHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AudienceFilterJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Broadcasts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Broadcasts_EndsAtUtc",
                table: "Broadcasts",
                column: "EndsAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Broadcasts_IsActive",
                table: "Broadcasts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Broadcasts_IsActive_StartsAtUtc_EndsAtUtc",
                table: "Broadcasts",
                columns: new[] { "IsActive", "StartsAtUtc", "EndsAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Broadcasts_StartsAtUtc",
                table: "Broadcasts",
                column: "StartsAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Broadcasts");
        }
    }
}
