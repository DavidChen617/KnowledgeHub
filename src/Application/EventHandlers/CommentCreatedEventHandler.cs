using CoreMesh.Outbox.Abstractions;
using Domain.Comments.Events;
using Domain.Notifications;
using Domain.Notes;
using Domain.Users;

namespace Application.EventHandlers;

public sealed class CommentCreatedEventHandler(
    INoteRepository noteRepository,
    IUserRepository userRepository,
    IEmailSender emailSender)
    : IEventHandler<CommentCreatedEvent>
{
    public async Task HandleAsync(CommentCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAsync(@event.NoteId, cancellationToken);
        if (note is null || Equals(note.UserId, @event.UserId)) return;

        var noteOwner = await userRepository.GetByIdAsync(note.UserId, cancellationToken);
        var commenter = await userRepository.GetByIdAsync(@event.UserId, cancellationToken);
        if (noteOwner is null || commenter is null) return;

        var action = @event.ParentCommentId is null ? "留言" : "回覆";
        await emailSender.SendAsync(
            noteOwner.Email,
            $"{commenter.Username} {action}了你的筆記《{note.Title}》",
            $"<p>{commenter.Username} 在你的筆記《{note.Title}》留下了{action}。</p>",
            cancellationToken);
    }
}
