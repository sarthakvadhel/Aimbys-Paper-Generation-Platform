using Aimbys.Application.Notifications;
using Aimbys.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Aimbys.Infrastructure.Notifications;

/// <summary>
/// V1 notification channel: logs each notification at Information level.
/// Future channels (email, SMS, WhatsApp, push) implement
/// <see cref="INotificationChannel"/> and are registered in DI alongside
/// this one.
/// </summary>
public class LoggingNotificationChannel : INotificationChannel
{
    private readonly ILogger<LoggingNotificationChannel> _logger;

    public LoggingNotificationChannel(ILogger<LoggingNotificationChannel> logger) => _logger = logger;

    public Task DeliverAsync(IReadOnlyList<Notification> notifications, CancellationToken cancellationToken = default)
    {
        foreach (var n in notifications)
        {
            _logger.LogInformation(
                "[Notification] To={UserId} Severity={Severity} Title=\"{Title}\" Route={RouteUrl}",
                n.RecipientUserId, n.Severity, n.Title, n.RouteUrl);
        }
        return Task.CompletedTask;
    }
}
