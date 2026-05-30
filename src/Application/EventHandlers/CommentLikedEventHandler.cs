using CoreMesh.Outbox.Abstractions;
using Domain.Comments;
using Domain.Comments.Events;
using Domain.Notifications;
using Domain.Users;

namespace Application.EventHandlers;

public sealed class CommentLikedEventHandler(
    ICommentRepository commentRepository,
    IUserRepository userRepository,
    IPushNotificationSender pushSender)
    : IEventHandler<CommentLikedEvent>
{
    public async Task HandleAsync(CommentLikedEvent @event, CancellationToken cancellationToken = default)
    {
        var comment = await commentRepository.GetByIdAsync(@event.CommentId, cancellationToken);
        if (comment is null || comment.UserId == @event.UserId) return;

        var liker = await userRepository.GetByIdAsync(@event.UserId, cancellationToken);
        if (liker is null) return;

        await pushSender.SendAsync(
            comment.UserId,
            $"{liker.Username} 按讚了你的留言",
            comment.Content,
            cancellationToken);
    }
}
