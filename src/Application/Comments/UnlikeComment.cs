using CoreMesh.Dispatching.Abstractions;
using Domain.Comments;
using Domain.Shared;
using Domain.Users;
using ShareKernal;
using static Application.Comments.CommentErrors;
using static ShareKernal.Result;

namespace Application.Comments;

public record UnlikeCommentCommandRequest(CommentId CommentId, UserId UserId) : IRequest<Result>;

public class UnlikeCommentHandler(ICommentRepository commentRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UnlikeCommentCommandRequest, Result>
{
    public async Task<Result> Handle(UnlikeCommentCommandRequest command, CancellationToken cancellationToken = default)
    {
        var like = await commentRepository.FindLikeAsync(command.CommentId, command.UserId, cancellationToken);
        if (like is null) return NotFound;

        await commentRepository.DeleteLikeAsync(like, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
