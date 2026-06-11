using CoreMesh.Outbox.Abstractions;
using Domain.Notifications;
using Domain.Users.Events;

namespace Application.EventHandlers;

public sealed class WelcomeEmailHandler(IEmailSender emailSender)
    : IEventHandler<UserRegisteredEvent>
{
    public async Task HandleAsync(UserRegisteredEvent @event, CancellationToken cancellationToken = default)
    {
        await emailSender.SendAsync(
            @event.Email,
            "歡迎使用 KnowledgeHub！",
            "<p>嗨，歡迎加入 KnowledgeHub。</p><p>開始記錄你的知識吧！</p>",
            cancellationToken);
    }
}
