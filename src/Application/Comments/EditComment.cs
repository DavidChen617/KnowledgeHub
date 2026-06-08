using CoreMesh.Dispatching.Abstractions;
using Domain.Comments;
using Domain.Shared;
using Domain.Users;
using ShareKernal;
using static Application.Comments.CommentErrors;
using static ShareKernal.Result;

namespace Application.Comments;

public record EditCommentCommandRequest(CommentId CommentId, UserId UserId, string Content) : IRequest<Result>;

public class EditCommentHandler(ICommentRepository commentRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<EditCommentCommandRequest, Result>
{
    public async Task<Result> Handle(EditCommentCommandRequest command, CancellationToken cancellationToken = default)
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
