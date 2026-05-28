using CoreMesh.Dispatching.Abstractions;
using Domain.Comments;
using Domain.Notes;
using Domain.Shared;
using Domain.Users;
using ShareKernal;
using static Application.Notes.NoteErrors;
using static ShareKernal.Result;

namespace Application.Comments;

public record AddCommentCommandRequest(
    NoteId NoteId,
    UserId UserId,
    string Content,
    CommentId? ParentCommentId,
    string? ShareToken) : IRequest<Result>;

public class AddCommentHandler(
    INoteRepository noteRepository,
    ICommentRepository commentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddCommentCommandRequest, Result>
{
    public async Task<Result> Handle(AddCommentCommandRequest command, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAsync(command.NoteId, cancellationToken);
        if (note is null) return NotFound;

        var isOwner = note.UserId == command.UserId;
        var hasShareAccess = command.ShareToken is not null && note.SharedLink?.Token == command.ShareToken;

        if (!isOwner && !hasShareAccess) return Forbidden;

        var commentResult = Comment.Create(command.NoteId, command.UserId, command.Content, command.ParentCommentId);
        if (!commentResult.IsSuccess) return commentResult.Error;

        await commentRepository.AddAsync(commentResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
