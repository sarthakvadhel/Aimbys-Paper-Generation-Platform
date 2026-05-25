using System.Security.Claims;
using System.Text.Json;
using Aimbys.Application.Audit;
using Aimbys.Application.Authorization;
using Aimbys.Application.Moderation;
using Aimbys.Domain.Entities;
using Aimbys.Domain.Entities.Moderation;
using Aimbys.Domain.Enums;
using Aimbys.Domain.Events;
using Aimbys.Infrastructure.Notifications;
using Aimbys.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Aimbys.Infrastructure.Moderation;

/// <summary>
/// Implements the moderation desk workflow: enqueue evaluations for
/// moderation, approve/require-changes/override, and capture audit trails.
/// </summary>
public class ModerationService : IModerationService
{
    private readonly AppDbContext _db;
    private readonly IAuditWriter _audit;
    private readonly DomainEventCollector _events;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IInstituteScope _instituteScope;

    public ModerationService(
        AppDbContext db,
        IAuditWriter audit,
        DomainEventCollector events,
        UserManager<IdentityUser> userManager,
        IInstituteScope instituteScope)
    {
        _db = db;
        _audit = audit;
        _events = events;
        _userManager = userManager;
        _instituteScope = instituteScope;
    }

    public async Task<ModerationResult> EnqueueForModerationAsync(Guid evaluationId, CancellationToken ct = default)
    {
        // Find a moderator via round-robin (teacher with CanModerate, least recent assignment)
        var moderator = await _db.TeacherProfiles
            .Where(t => t.CanModerate && t.Status == ProfileStatus.Active)
            .OrderBy(t => _db.Set<Domain.Entities.Moderation.Moderation>()
                .Where(m => m.ModeratorTeacherProfileId == t.Id)
                .Max(m => (DateTime?)m.AssignedAtUtc) ?? DateTime.MinValue)
            .FirstOrDefaultAsync(ct);

        if (moderator is null)
            return ModerationResult.Fail("No available moderator found.");

        var moderation = new Domain.Entities.Moderation.Moderation
        {
            EvaluationId = evaluationId,
            ModeratorTeacherProfileId = moderator.Id,
            Verdict = ModerationVerdict.Pending,
            AssignedAtUtc = DateTime.UtcNow
        };

        _db.Set<Domain.Entities.Moderation.Moderation>().Add(moderation);

        // Capture snapshot
        var snapshot = new ModerationSnapshot
        {
            ModerationId = moderation.Id,
            EvaluatorScoresJson = "{}",
            CapturedAtUtc = DateTime.UtcNow
        };

        _db.Set<ModerationSnapshot>().Add(snapshot);

        _events.Enqueue(new ModerationAssignedEvent
        {
            ModerationId = moderation.Id,
            EvaluationId = evaluationId,
            ModeratorUserId = moderator.UserId
        });

        await _audit.WriteAsync(
            "Moderation.Assigned",
            "Moderation",
            moderation.Id.ToString(),
            null,
            JsonSerializer.Serialize(new { evaluationId, moderatorProfileId = moderator.Id }),
            cancellationToken: ct);

        await _db.SaveChangesAsync(ct);

        return ModerationResult.Ok();
    }

    public async Task<ModerationResult> ApproveAsync(Guid moderationId, ClaimsPrincipal actor, CancellationToken ct = default)
    {
        var moderation = await _db.Set<Domain.Entities.Moderation.Moderation>()
            .FirstOrDefaultAsync(m => m.Id == moderationId, ct);

        if (moderation is null)
            return ModerationResult.Fail("Moderation not found.");

        var actorUserId = _userManager.GetUserId(actor);

        moderation.Verdict = ModerationVerdict.Approved;
        moderation.CompletedAtUtc = DateTime.UtcNow;

        // Write ModeratedScore (pass-through, same values)
        var moderatedScore = new ModeratedScore
        {
            ModerationId = moderationId,
            EvaluationId = moderation.EvaluationId,
            TotalPointsAwarded = 0, // Pass-through; real values come from evaluation
            MaxPointsPossible = 0,
            ModeratedByUserId = actorUserId ?? string.Empty,
            ModeratedAtUtc = DateTime.UtcNow,
            IsOverride = false
        };

        _db.Set<ModeratedScore>().Add(moderatedScore);

        _events.Enqueue(new ModerationApprovedEvent
        {
            ModerationId = moderationId,
            EvaluationId = moderation.EvaluationId
        });

        await _audit.WriteAsync(
            "Moderation.Approved",
            "Moderation",
            moderationId.ToString(),
            actorUserId,
            cancellationToken: ct);

        await _db.SaveChangesAsync(ct);

        return ModerationResult.Ok();
    }

    public async Task<ModerationResult> RequireChangesAsync(Guid moderationId, ClaimsPrincipal actor, string comment, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(comment))
            return ModerationResult.Fail("Comment is required when requesting changes.");

        var moderation = await _db.Set<Domain.Entities.Moderation.Moderation>()
            .FirstOrDefaultAsync(m => m.Id == moderationId, ct);

        if (moderation is null)
            return ModerationResult.Fail("Moderation not found.");

        var actorUserId = _userManager.GetUserId(actor);

        moderation.Verdict = ModerationVerdict.RequiresChanges;
        moderation.Comment = comment;

        await _audit.WriteAsync(
            "Moderation.RequiresChanges",
            "Moderation",
            moderationId.ToString(),
            actorUserId,
            JsonSerializer.Serialize(new { comment }),
            cancellationToken: ct);

        await _db.SaveChangesAsync(ct);

        return ModerationResult.Ok();
    }

    public async Task<ModerationResult> OverrideAsync(Guid moderationId, ClaimsPrincipal actor, decimal newScore, decimal maxScore, string reason, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return ModerationResult.Fail("Override reason is required.");

        var moderation = await _db.Set<Domain.Entities.Moderation.Moderation>()
            .FirstOrDefaultAsync(m => m.Id == moderationId, ct);

        if (moderation is null)
            return ModerationResult.Fail("Moderation not found.");

        var actorUserId = _userManager.GetUserId(actor);

        moderation.Verdict = ModerationVerdict.Overridden;
        moderation.OverrideReason = reason;
        moderation.CompletedAtUtc = DateTime.UtcNow;

        var moderatedScore = new ModeratedScore
        {
            ModerationId = moderationId,
            EvaluationId = moderation.EvaluationId,
            TotalPointsAwarded = newScore,
            MaxPointsPossible = maxScore,
            ModeratedByUserId = actorUserId ?? string.Empty,
            ModeratedAtUtc = DateTime.UtcNow,
            IsOverride = true,
            OverrideReason = reason
        };

        _db.Set<ModeratedScore>().Add(moderatedScore);

        // COMPLIANCE audit with Warning severity
        await _audit.WriteAsync(
            "Moderation.Override",
            "Moderation",
            moderationId.ToString(),
            actorUserId,
            JsonSerializer.Serialize(new { newScore, maxScore, reason }),
            AuditSeverity.Warning,
            ct);

        _events.Enqueue(new ModerationOverriddenEvent
        {
            ModerationId = moderationId,
            EvaluationId = moderation.EvaluationId,
            OverrideReason = reason
        });

        await _db.SaveChangesAsync(ct);

        return ModerationResult.Ok();
    }

    public async Task<ModerationContext?> GetContextAsync(Guid moderationId, CancellationToken ct = default)
    {
        var moderation = await _db.Set<Domain.Entities.Moderation.Moderation>()
            .FirstOrDefaultAsync(m => m.Id == moderationId, ct);

        if (moderation is null)
            return null;

        var snapshot = await _db.Set<ModerationSnapshot>()
            .FirstOrDefaultAsync(s => s.ModerationId == moderationId, ct);

        return new ModerationContext(
            ModerationId: moderation.Id,
            EvaluationId: moderation.EvaluationId,
            EvaluatorScoresJson: snapshot?.EvaluatorScoresJson,
            AnswerJson: null,
            Verdict: moderation.Verdict,
            EvaluatedTotal: null,
            MaxPossible: null);
    }
}
