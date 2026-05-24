using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aimbys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModerationQueues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstituteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModeratorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    EnqueuedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    ResolvedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationQueues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReviewerAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InstituteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentLoad = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewerAssignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowDeadlines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InstituteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DueAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReminderSentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EscalatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsOverdue = table.Column<bool>(type: "bit", nullable: false),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    ResolvedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowDeadlines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    StatesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransitionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EscalationRulesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowEscalationRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DefinitionKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaxDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    ReminderAtPercent = table.Column<int>(type: "int", nullable: false),
                    EscalateToRole = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EscalateToPermission = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowEscalationRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DefinitionKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DefinitionVersion = table.Column<int>(type: "int", nullable: false),
                    SubjectType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstituteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CurrentState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    StartedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowInstances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowReminders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeadlineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecipientUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    IsEscalation = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowReminders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalQueues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DefinitionKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QueueName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InstituteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignedToUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    EnqueuedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    ResolvedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalQueues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalQueues_WorkflowInstances_InstanceId",
                        column: x => x.InstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTransitions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ToState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ActorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TransitionedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitions_WorkflowInstances_InstanceId",
                        column: x => x.InstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QueueItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedToUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskAssignments_ApprovalQueues_QueueItemId",
                        column: x => x.QueueItemId,
                        principalTable: "ApprovalQueues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalQueues_DefinitionKey_QueueName_AssignedToUserId_EnqueuedAtUtc",
                table: "ApprovalQueues",
                columns: new[] { "DefinitionKey", "QueueName", "AssignedToUserId", "EnqueuedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalQueues_InstanceId",
                table: "ApprovalQueues",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalQueues_IsResolved_InstituteId",
                table: "ApprovalQueues",
                columns: new[] { "IsResolved", "InstituteId" });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationQueues_EvaluationId",
                table: "ModerationQueues",
                column: "EvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationQueues_InstituteId",
                table: "ModerationQueues",
                column: "InstituteId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationQueues_IsResolved_ModeratorUserId_EnqueuedAtUtc",
                table: "ModerationQueues",
                columns: new[] { "IsResolved", "ModeratorUserId", "EnqueuedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewerAssignments_InstituteId",
                table: "ReviewerAssignments",
                column: "InstituteId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewerAssignments_ReviewerUserId_IsActive",
                table: "ReviewerAssignments",
                columns: new[] { "ReviewerUserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewerAssignments_SubjectType_SubjectId",
                table: "ReviewerAssignments",
                columns: new[] { "SubjectType", "SubjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_AssignedToUserId_IsActive",
                table: "TaskAssignments",
                columns: new[] { "AssignedToUserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_QueueItemId_IsActive",
                table: "TaskAssignments",
                columns: new[] { "QueueItemId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDeadlines_InstanceId",
                table: "WorkflowDeadlines",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDeadlines_InstituteId",
                table: "WorkflowDeadlines",
                column: "InstituteId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDeadlines_IsResolved_IsOverdue_DueAtUtc",
                table: "WorkflowDeadlines",
                columns: new[] { "IsResolved", "IsOverdue", "DueAtUtc" });

            migrationBuilder.CreateIndex(
                name: "UX_WorkflowDefinitions_Key_Version",
                table: "WorkflowDefinitions",
                columns: new[] { "Key", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_WorkflowEscalationRules_DefinitionKey_State",
                table: "WorkflowEscalationRules",
                columns: new[] { "DefinitionKey", "State" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_DefinitionKey_SubjectType_SubjectId",
                table: "WorkflowInstances",
                columns: new[] { "DefinitionKey", "SubjectType", "SubjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_InstituteId",
                table: "WorkflowInstances",
                column: "InstituteId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_IsCompleted_CurrentState",
                table: "WorkflowInstances",
                columns: new[] { "IsCompleted", "CurrentState" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_SubjectType_SubjectId",
                table: "WorkflowInstances",
                columns: new[] { "SubjectType", "SubjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowReminders_DeadlineId_SentAtUtc",
                table: "WorkflowReminders",
                columns: new[] { "DeadlineId", "SentAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowReminders_RecipientUserId",
                table: "WorkflowReminders",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_InstanceId_TransitionedAtUtc",
                table: "WorkflowTransitions",
                columns: new[] { "InstanceId", "TransitionedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModerationQueues");

            migrationBuilder.DropTable(
                name: "ReviewerAssignments");

            migrationBuilder.DropTable(
                name: "TaskAssignments");

            migrationBuilder.DropTable(
                name: "WorkflowDeadlines");

            migrationBuilder.DropTable(
                name: "WorkflowDefinitions");

            migrationBuilder.DropTable(
                name: "WorkflowEscalationRules");

            migrationBuilder.DropTable(
                name: "WorkflowReminders");

            migrationBuilder.DropTable(
                name: "WorkflowTransitions");

            migrationBuilder.DropTable(
                name: "ApprovalQueues");

            migrationBuilder.DropTable(
                name: "WorkflowInstances");
        }
    }
}
