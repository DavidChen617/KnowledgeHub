using CoreMesh.Dispatching.Abstractions;
using Domain.Comments;
using Domain.Shared;
using Domain.Users;

namespace Application.Comments;

public record UnlikeCommentCommandRequest(CommentId CommentId, UserId UserId) : IRequest<UnlikeCommentCommandResponse>;

public enum UnlikeCommentCommandResponse { Success, NotFound }

public class UnlikeCommentHandler(ICommentRepository commentRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UnlikeCommentCommandRequest, UnlikeCommentCommandResponse>
{
    public async Task<UnlikeCommentCommandResponse> Handle(UnlikeCommentCommandRequest command, CancellationToken cancellationToken = default)
    {
        var like = await commentRepository.FindLikeAsync(command.CommentId, command.UserId, cancellationToken);
        if (like is null) return UnlikeCommentCommandResponse.NotFound;

        await commentRepository.DeleteLikeAsync(like, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnlikeCommentCommandResponse.Success;
    }
}
