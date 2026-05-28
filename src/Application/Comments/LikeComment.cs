using CoreMesh.Dispatching.Abstractions;
using Domain.Comments;
using Domain.Shared;
using Domain.Users;
using ShareKernal;

namespace Application.Comments;

public record LikeCommentCommandRequest(CommentId CommentId, UserId UserId) : IRequest<Result>;

public class LikeCommentHandler(ICommentRepository commentRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<LikeCommentCommandRequest, Result>
{
    public async Task<Result> Handle(LikeCommentCommandRequest command, CancellationToken cancellationToken = default)
    {
        var comment = await commentRepository.GetByIdAsync(command.CommentId, cancellationToken);
        if (comment is null) return CommentErrors.NotFound;

        var existing = await commentRepository.FindLikeAsync(command.CommentId, command.UserId, cancellationToken);
        if (existing is not null) return CommentErrors.AlreadyLiked;

        await commentRepository.AddLikeAsync(CommentLike.Create(command.CommentId, command.UserId), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
