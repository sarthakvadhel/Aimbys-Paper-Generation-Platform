using Aimbys.Application.Notifications;
using Aimbys.Domain.Entities;
using Aimbys.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aimbys.Infrastructure.Notifications;

/// <summary>
/// Default dispatcher: for each event, resolves its
/// <see cref="INotificationProjection{TEvent}"/> from DI, projects
/// notifications, persists them, then fans out to all registered
/// <see cref="INotificationChannel"/>s.
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _services;
    private readonly INotificationService _notificationService;
    private readonly IEnumerable<INotificationChannel> _channels;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(
        IServiceProvider services,
        INotificationService notificationService,
        IEnumerable<INotificationChannel> channels,
        ILogger<DomainEventDispatcher> logger)
    {
        _services = services;
        _notificationService = notificationService;
        _channels = channels;
        _logger = logger;
    }

    public async Task DispatchAsync(IReadOnlyList<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        if (events is null || events.Count == 0) return;

        var allNotifications = new List<Notification>();

        foreach (var domainEvent in events)
        {
            try
            {
                var notifications = await ProjectEventAsync(domainEvent, cancellationToken);
                if (notifications.Count > 0)
                {
                    allNotifications.AddRange(notifications);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to project domain event {EventType}. Skipping.",
                    domainEvent.GetType().Name);
            }
        }

        if (allNotifications.Count == 0) return;

        await _notificationService.CreateBatchAsync(allNotifications, cancellationToken);

        foreach (var channel in _channels)
        {
            try
            {
                await channel.DeliverAsync(allNotifications, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Notification channel {Channel} failed. Notifications are persisted; delivery will retry on next read.",
                    channel.GetType().Name);
            }
        }
    }

    private Task<IReadOnlyList<Notification>> ProjectEventAsync(
        IDomainEvent domainEvent, CancellationToken ct)
    {
        // Resolve INotificationProjection<TConcreteEvent> from DI via
        // reflection on the runtime event type.
        var eventType = domainEvent.GetType();
        var projectionType = typeof(INotificationProjection<>).MakeGenericType(eventType);
        var projection = _services.GetService(projectionType);

        if (projection is null)
        {
            _logger.LogDebug(
                "No projection registered for {EventType}. No notifications emitted.",
                eventType.Name);
            return Task.FromResult<IReadOnlyList<Notification>>(Array.Empty<Notification>());
        }

        // Call ProjectAsync via reflection (the generic type is only known at
        // runtime). The method signature is:
        //   Task<IReadOnlyList<Notification>> ProjectAsync(TEvent, CancellationToken)
        var method = projectionType.GetMethod(nameof(INotificationProjection<IDomainEvent>.ProjectAsync))!;
        var task = (Task<IReadOnlyList<Notification>>)method.Invoke(projection, new object[] { domainEvent, ct })!;
        return task;
    }
}
