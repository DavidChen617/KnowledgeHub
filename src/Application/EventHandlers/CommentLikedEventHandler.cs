using CoreMesh.Outbox.Abstractions;
using Domain.Comments;
using Domain.Comments.Events;
using Domain.Notifications;
using Domain.Users;

namespace Application.EventHandlers;

public sealed class CommentLikedEventHandler(
    ICommentRepository commentRepository,
    IUserRepository userRepository,
    IEmailSender emailSender)
    : IEventHandler<CommentLikedEvent>
{
    public async Task HandleAsync(CommentLikedEvent @event, CancellationToken cancellationToken = default)
    {
        var comment = await commentRepository.GetByIdAsync(new CommentId(@event.CommentId), cancellationToken);
        if (comment is null || comment.UserId == new UserId(@event.UserId)) return;

        var commentAuthor = await userRepository.GetByIdAsync(comment.UserId, cancellationToken);
        var liker = await userRepository.GetByIdAsync(new UserId(@event.UserId), cancellationToken);
        if (commentAuthor is null || liker is null) return;

        await emailSender.SendAsync(
            commentAuthor.Email,
            $"{liker.Username} 按讚了你的留言",
            $"<p>{liker.Username} 按讚了你的留言：{comment.Content}</p>",
            cancellationToken);
    }
}
