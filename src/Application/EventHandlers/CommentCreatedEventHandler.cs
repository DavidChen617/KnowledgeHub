using CoreMesh.Outbox.Abstractions;
using Domain.Comments.Events;
using Domain.Notifications;
using Domain.Notes;
using Domain.Users;

namespace Application.EventHandlers;

public sealed class CommentCreatedEventHandler(
    INoteRepository noteRepository,
    IUserRepository userRepository,
    IPushNotificationSender pushSender)
    : IEventHandler<CommentCreatedEvent>
{
    public async Task HandleAsync(CommentCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAsync(@event.NoteId, cancellationToken);
        if (note is null || note.UserId == @event.UserId) return;

        var commenter = await userRepository.GetByIdAsync(@event.UserId, cancellationToken);
        if (commenter is null) return;

        var action = @event.ParentCommentId is null ? "留言" : "回覆";
        await pushSender.SendAsync(
            note.UserId,
            $"{commenter.Username} {action}了你的筆記",
            note.Title,
            cancellationToken);
    }
}
