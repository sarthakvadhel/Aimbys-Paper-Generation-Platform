using Aimbys.Domain.Entities;

namespace Aimbys.Application.Notifications;

/// <summary>
/// Delivery channel for notifications. V1 ships only
/// <c>LoggingNotificationChannel</c> (writes to ILogger). Future channels
/// (email, SMS, WhatsApp, push) implement this same interface and are
/// registered in DI; the dispatcher fans out to all registered channels.
/// </summary>
public interface INotificationChannel
{
    /// <summary>
    /// Delivers a batch of notifications through this channel. Must not
    /// throw on partial failure; log and continue.
    /// </summary>
    Task DeliverAsync(IReadOnlyList<Notification> notifications, CancellationToken cancellationToken = default);
}
