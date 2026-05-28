using CoreMesh.Dispatching.Abstractions;
using Domain.Comments;
using Domain.Shared;
using Domain.Users;

namespace Application.Comments;

public record LikeCommentCommand(CommentId CommentId, UserId UserId) : IRequest<LikeCommentResult>;

public enum LikeCommentResult { Success, NotFound, AlreadyLiked }

public class LikeCommentHandler(ICommentRepository commentRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<LikeCommentCommand, LikeCommentResult>
{
    public async Task<LikeCommentResult> Handle(LikeCommentCommand command, CancellationToken cancellationToken = default)
    {
        var comment = await commentRepository.GetByIdAsync(command.CommentId, cancellationToken);
        if (comment is null) return LikeCommentResult.NotFound;

        var existing = await commentRepository.FindLikeAsync(command.CommentId, command.UserId, cancellationToken);
        if (existing is not null) return LikeCommentResult.AlreadyLiked;

        await commentRepository.AddLikeAsync(CommentLike.Create(command.CommentId, command.UserId), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return LikeCommentResult.Success;
    }
}
