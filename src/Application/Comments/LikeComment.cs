using CoreMesh.Dispatching.Abstractions;
using Domain.Comments;
using Domain.Shared;
using Domain.Users;

namespace Application.Comments;

public record LikeCommentCommandRequest(CommentId CommentId, UserId UserId) : IRequest<LikeCommentCommandResponse>;

public enum LikeCommentCommandResponse { Success, NotFound, AlreadyLiked }

public class LikeCommentHandler(ICommentRepository commentRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<LikeCommentCommandRequest, LikeCommentCommandResponse>
{
    public async Task<LikeCommentCommandResponse> Handle(LikeCommentCommandRequest command, CancellationToken cancellationToken = default)
    {
        var comment = await commentRepository.GetByIdAsync(command.CommentId, cancellationToken);
        if (comment is null) return LikeCommentCommandResponse.NotFound;

        var existing = await commentRepository.FindLikeAsync(command.CommentId, command.UserId, cancellationToken);
        if (existing is not null) return LikeCommentCommandResponse.AlreadyLiked;

        await commentRepository.AddLikeAsync(CommentLike.Create(command.CommentId, command.UserId), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return LikeCommentCommandResponse.Success;
    }
}
