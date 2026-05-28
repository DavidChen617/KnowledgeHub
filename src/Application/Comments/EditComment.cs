using CoreMesh.Dispatching.Abstractions;
using Domain.Comments;
using Domain.Shared;
using Domain.Users;

namespace Application.Comments;

public record EditCommentCommandRequest(CommentId CommentId, UserId UserId, string Content) : IRequest<EditCommentCommandResponse>;

public enum EditCommentCommandResponse { Success, NotFound, Forbidden }

public class EditCommentHandler(ICommentRepository commentRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<EditCommentCommandRequest, EditCommentCommandResponse>
{
    public async Task<EditCommentCommandResponse> Handle(EditCommentCommandRequest command, CancellationToken cancellationToken = default)
    {
        var comment = await commentRepository.GetByIdAsync(command.CommentId, cancellationToken);
        if (comment is null) return EditCommentCommandResponse.NotFound;
        if (comment.UserId != command.UserId) return EditCommentCommandResponse.Forbidden;

        comment.UpdateContent(command.Content);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return EditCommentCommandResponse.Success;
    }
}
