using CoreMesh.Dispatching.Abstractions;
using Domain.Comments;
using Domain.Shared;
using Domain.Users;
using ShareKernal;

namespace Application.Comments;

public record DeleteCommentCommandRequest(CommentId CommentId, UserId UserId) : IRequest<Result>;

public class DeleteCommentHandler(ICommentRepository commentRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCommentCommandRequest, Result>
{
    public async Task<Result> Handle(DeleteCommentCommandRequest command, CancellationToken cancellationToken = default)
    {
        var comment = await commentRepository.GetByIdAsync(command.CommentId, cancellationToken);
        if (comment is null) return CommentErrors.NotFound;
        if (comment.UserId != command.UserId) return CommentErrors.Forbidden;

        await commentRepository.DeleteAsync(comment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
