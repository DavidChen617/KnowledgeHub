using CoreMesh.Dispatching.Abstractions;
using Domain.Comments;
using Domain.Notes;
using Domain.Shared;
using Domain.Users;

namespace Application.Comments;

public record AddCommentCommandRequest(
    NoteId NoteId,
    UserId UserId,
    string Content,
    CommentId? ParentCommentId,
    string? ShareToken) : IRequest<AddCommentCommandResponse>;

public enum AddCommentCommandResponse { Success, NotFound, Forbidden }

public class AddCommentHandler(
    INoteRepository noteRepository,
    ICommentRepository commentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddCommentCommandRequest, AddCommentCommandResponse>
{
    public async Task<AddCommentCommandResponse> Handle(AddCommentCommandRequest command, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAsync(command.NoteId, cancellationToken);
        if (note is null) return AddCommentCommandResponse.NotFound;

        var isOwner = note.UserId == command.UserId;
        var hasShareAccess = command.ShareToken is not null && note.SharedLink?.Token == command.ShareToken;

        if (!isOwner && !hasShareAccess) return AddCommentCommandResponse.Forbidden;

        var comment = Comment.Create(command.NoteId, command.UserId, command.Content, command.ParentCommentId);
        await commentRepository.AddAsync(comment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AddCommentCommandResponse.Success;
    }
}
