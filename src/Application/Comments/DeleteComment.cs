using CoreMesh.Dispatching.Abstractions;
using Domain.Comments;

using Domain.Users;
using ShareKernal;
using static Application.Comments.CommentErrors;
using static ShareKernal.Result;

namespace Application.Comments;

public record DeleteCommentCommand(CommentId CommentId, UserId UserId) : IRequest<Result>;

public class DeleteCommentHandler(ICommentRepository commentRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCommentCommand, Result>
{
    public async Task<Result> Handle(DeleteCommentCommand command, CancellationToken cancellationToken = default)
    {
        var comment = await commentRepository.GetByIdAsync(command.CommentId, cancellationToken);
        if (comment is null) return NotFound;
        if (comment.UserId != command.UserId) return Forbidden;

        comment.Delete();
        await commentRepository.DeleteAsync(comment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
