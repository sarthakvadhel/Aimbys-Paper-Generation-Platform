using Aimbys.Domain.Enums;

namespace Aimbys.Domain.Entities;

/// <summary>
/// A generation job submitted to a provider (OpenAI, Azure OpenAI, etc.).
/// The <see cref="Provider"/> field is the routing key used by the
/// provider-agnostic generation abstraction.
/// </summary>
public class Job
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Optional project the job belongs to.</summary>
    public Guid? ProjectId { get; set; }

    /// <summary>Optional template the job started from.</summary>
    public Guid? TemplateId { get; set; }

    public JobStatus Status { get; set; } = JobStatus.Pending;

    /// <summary>
    /// Routing key for the generation provider, e.g. "openai-responses".
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>Serialized JSON request payload.</summary>
    public string RequestPayload { get; set; } = string.Empty;

    /// <summary>Serialized JSON response payload (set on success).</summary>
    public string? ResponsePayload { get; set; }

    /// <summary>Error message captured on failure.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>ASP.NET Identity user id of the requester.</summary>
    public string RequestedByUserId { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    // Navigation
    public Project? Project { get; set; }
    public Template? Template { get; set; }
}
