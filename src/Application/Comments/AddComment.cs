using CoreMesh.Dispatching.Abstractions;
using Domain.Comments;
using Domain.Notes;
using Domain.Shared;
using Domain.Users;

namespace Application.Comments;

public record AddCommentCommand(
    NoteId NoteId,
    UserId UserId,
    string Content,
    CommentId? ParentCommentId,
    string? ShareToken) : IRequest<AddCommentResult>;

public enum AddCommentResult { Success, NotFound, Forbidden }

public class AddCommentHandler(
    INoteRepository noteRepository,
    ICommentRepository commentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddCommentCommand, AddCommentResult>
{
    public async Task<AddCommentResult> Handle(AddCommentCommand command, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAsync(command.NoteId, cancellationToken);
        if (note is null) return AddCommentResult.NotFound;

        var isOwner = note.UserId == command.UserId;
        var hasShareAccess = command.ShareToken is not null && note.SharedLink?.Token == command.ShareToken;

        if (!isOwner && !hasShareAccess) return AddCommentResult.Forbidden;

        var comment = Comment.Create(command.NoteId, command.UserId, command.Content, command.ParentCommentId);
        await commentRepository.AddAsync(comment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AddCommentResult.Success;
    }
}
