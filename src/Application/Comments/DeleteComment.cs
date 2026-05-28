using CoreMesh.Dispatching.Abstractions;
using Domain.Comments;
using Domain.Shared;
using Domain.Users;

namespace Application.Comments;

public record DeleteCommentCommand(CommentId CommentId, UserId UserId) : IRequest<DeleteCommentResult>;

public enum DeleteCommentResult { Success, NotFound, Forbidden }

public class DeleteCommentHandler(ICommentRepository commentRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCommentCommand, DeleteCommentResult>
{
    public async Task<DeleteCommentResult> Handle(DeleteCommentCommand command, CancellationToken cancellationToken = default)
    {
        var comment = await commentRepository.GetByIdAsync(command.CommentId, cancellationToken);
        if (comment is null) return DeleteCommentResult.NotFound;
        if (comment.UserId != command.UserId) return DeleteCommentResult.Forbidden;

        await commentRepository.DeleteAsync(comment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return DeleteCommentResult.Success;
    }
}
