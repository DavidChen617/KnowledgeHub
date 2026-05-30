using Domain.Users;

namespace Domain.Notifications;

public interface IPushNotificationSender
{
    Task SendAsync(UserId userId, string title, string body, CancellationToken ct = default);
}
