using CoreMesh.Dispatching.Abstractions;
using Domain.Comments;
using Domain.Shared;
using Domain.Users;

namespace Application.Comments;

public record DeleteCommentCommandRequest(CommentId CommentId, UserId UserId) : IRequest<DeleteCommentCommandResponse>;

public enum DeleteCommentCommandResponse { Success, NotFound, Forbidden }

public class DeleteCommentHandler(ICommentRepository commentRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCommentCommandRequest, DeleteCommentCommandResponse>
{
    public async Task<DeleteCommentCommandResponse> Handle(DeleteCommentCommandRequest command, CancellationToken cancellationToken = default)
    {
        var comment = await commentRepository.GetByIdAsync(command.CommentId, cancellationToken);
        if (comment is null) return DeleteCommentCommandResponse.NotFound;
        if (comment.UserId != command.UserId) return DeleteCommentCommandResponse.Forbidden;

        await commentRepository.DeleteAsync(comment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return DeleteCommentCommandResponse.Success;
    }
}
