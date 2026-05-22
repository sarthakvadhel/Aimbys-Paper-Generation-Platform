namespace Aimbys.Domain.Enums;

/// <summary>
/// Lifecycle states for a generation job.
/// Ordered so default(JobStatus) == Pending.
/// </summary>
public enum JobStatus
{
    Pending = 0,
    Running = 1,
    Succeeded = 2,
    Failed = 3,
    Cancelled = 4
}
