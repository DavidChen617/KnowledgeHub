using CoreMesh.Dispatching.Abstractions;
using Domain.Comments;

using Domain.Users;
using ShareKernal;
using static Application.Comments.CommentErrors;
using static ShareKernal.Result;

namespace Application.Comments;

public record EditCommentCommand(CommentId CommentId, UserId UserId, string Content) : IRequest<Result>;

public class EditCommentHandler(ICommentRepository commentRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<EditCommentCommand, Result>
{
    public async Task<Result> Handle(EditCommentCommand command, CancellationToken cancellationToken = default)
    {
        var comment = await commentRepository.GetByIdAsync(command.CommentId, cancellationToken);
        if (comment is null) return NotFound;
        if (comment.UserId != command.UserId) return Forbidden;

        var editResult = comment.UpdateContent(command.Content);
        if (!editResult.IsSuccess) return editResult.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
