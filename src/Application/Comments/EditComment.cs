using CoreMesh.Dispatching.Abstractions;
using Domain.Comments;
using Domain.Shared;
using Domain.Users;

namespace Application.Comments;

public record EditCommentCommand(CommentId CommentId, UserId UserId, string Content) : IRequest<EditCommentResult>;

public enum EditCommentResult { Success, NotFound, Forbidden }

public class EditCommentHandler(ICommentRepository commentRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<EditCommentCommand, EditCommentResult>
{
    public async Task<EditCommentResult> Handle(EditCommentCommand command, CancellationToken cancellationToken = default)
    {
        var comment = await commentRepository.GetByIdAsync(command.CommentId, cancellationToken);
        if (comment is null) return EditCommentResult.NotFound;
        if (comment.UserId != command.UserId) return EditCommentResult.Forbidden;

        comment.UpdateContent(command.Content);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return EditCommentResult.Success;
    }
}
