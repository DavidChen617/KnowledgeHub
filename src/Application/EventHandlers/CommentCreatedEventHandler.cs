using CoreMesh.Outbox.Abstractions;
using Domain.Comments;
using Domain.Comments.Events;
using Domain.Notifications;
using Domain.Notes;
using Domain.Users;

namespace Application.EventHandlers;

public sealed class CommentCreatedEventHandler(
    INoteRepository noteRepository,
    ICommentRepository commentRepository,
    IUserRepository userRepository,
    IEmailSender emailSender)
    : IEventHandler<CommentCreatedEvent>
{
    public async Task HandleAsync(CommentCreatedEvent @event, CancellationToken cancellationToken = default)
    {

        var note = await noteRepository.GetByIdAsync(new NoteId(@event.NoteId), cancellationToken);
        if (note is null) return;

        var commenter = await userRepository.GetByIdAsync(new UserId(@event.UserId), cancellationToken);
        if (commenter is null) return;

        // 收件人：筆記作者 + 父留言作者，去重，排除留言者本人
        var recipientIds = new HashSet<UserId> { note.UserId };

        if (@event.ParentCommentId is not null)
        {
            var parentComment = await commentRepository.GetByIdAsync(new CommentId(@event.ParentCommentId.Value), cancellationToken);
            if (parentComment is not null)
                recipientIds.Add(parentComment.UserId);
        }

        recipientIds.Remove(new UserId(@event.UserId));
        if (recipientIds.Count == 0) return;

        var action = @event.ParentCommentId is null ? "留言" : "回覆";
        foreach (var recipientId in recipientIds)
        {
            var recipient = await userRepository.GetByIdAsync(recipientId, cancellationToken);
            if (recipient is null) continue;
            await emailSender.SendAsync(
                recipient.Email,
                $"{commenter.Username} {action}了你的筆記《{note.Title}》",
                $"<p>{commenter.Username} 在你的筆記《{note.Title}》留下了{action}。</p>",
                cancellationToken);
        }
    }
}
