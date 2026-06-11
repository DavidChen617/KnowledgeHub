using CoreMesh.Dispatching.Abstractions;
using Domain.Comments;

using Domain.Users;
using ShareKernal;
using static Application.Comments.CommentErrors;
using static ShareKernal.Result;

namespace Application.Comments;

public record UnlikeCommentCommand(CommentId CommentId, UserId UserId) : IRequest<Result>;

public class UnlikeCommentHandler(ICommentRepository commentRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UnlikeCommentCommand, Result>
{
    public async Task<Result> Handle(UnlikeCommentCommand command, CancellationToken cancellationToken = default)
    {
        var like = await commentRepository.FindLikeAsync(command.CommentId, command.UserId, cancellationToken);
        if (like is null) return NotFound;

        var comment = await commentRepository.GetByIdAsync(command.CommentId, cancellationToken);
        if (comment is null) return NotFound;

        comment.Unlike();
        await commentRepository.DeleteLikeAsync(like, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
