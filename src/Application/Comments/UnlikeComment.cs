using CoreMesh.Dispatching.Abstractions;
using Domain.Comments;
using Domain.Shared;
using Domain.Users;

namespace Application.Comments;

public record UnlikeCommentCommand(CommentId CommentId, UserId UserId) : IRequest<UnlikeCommentResult>;

public enum UnlikeCommentResult { Success, NotFound }

public class UnlikeCommentHandler(ICommentRepository commentRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UnlikeCommentCommand, UnlikeCommentResult>
{
    public async Task<UnlikeCommentResult> Handle(UnlikeCommentCommand command, CancellationToken cancellationToken = default)
    {
        var like = await commentRepository.FindLikeAsync(command.CommentId, command.UserId, cancellationToken);
        if (like is null) return UnlikeCommentResult.NotFound;

        await commentRepository.DeleteLikeAsync(like, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnlikeCommentResult.Success;
    }
}
