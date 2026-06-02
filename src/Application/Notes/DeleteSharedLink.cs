using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.Shared;
using Domain.Users;
using ShareKernal;
using static Application.Notes.NoteErrors;
using static ShareKernal.Result;

namespace Application.Notes;

public record DeleteSharedLinkCommandRequest(NoteId NoteId, UserId UserId) : IRequest<Result>;

public class DeleteSharedLinkHandler(INoteRepository noteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteSharedLinkCommandRequest, Result>
{
    public async Task<Result> Handle(DeleteSharedLinkCommandRequest command, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAndUserIdAsync(command.NoteId, command.UserId, cancellationToken);
        if (note is null) return NotFound;

        note.DeleteSharedLink();
        await noteRepository.Update(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
