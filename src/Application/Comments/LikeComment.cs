using CoreMesh.Dispatching.Abstractions;
using Domain.Comments;

using Domain.Users;
using ShareKernal;
using static Application.Comments.CommentErrors;
using static ShareKernal.Result;

namespace Application.Comments;

public record LikeCommentCommand(CommentId CommentId, UserId UserId) : IRequest<Result>;

public class LikeCommentHandler(ICommentRepository commentRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<LikeCommentCommand, Result>
{
    public async Task<Result> Handle(LikeCommentCommand command, CancellationToken cancellationToken = default)
    {
        var comment = await commentRepository.GetByIdAsync(command.CommentId, cancellationToken);
        if (comment is null) return NotFound;

        var existing = await commentRepository.FindLikeAsync(command.CommentId, command.UserId, cancellationToken);
        if (existing is not null) return AlreadyLiked;

        await commentRepository.AddLikeAsync(comment.Like(command.UserId), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
