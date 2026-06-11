using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;

using Domain.Users;
using ShareKernal;
using static Application.Notes.NoteErrors;
using static ShareKernal.Result;

namespace Application.Notes;

public record DeleteSharedLinkCommand(NoteId NoteId, UserId UserId) : IRequest<Result>;

public class DeleteSharedLinkHandler(INoteRepository noteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteSharedLinkCommand, Result>
{
    public async Task<Result> Handle(DeleteSharedLinkCommand command, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAndUserIdAsync(command.NoteId, command.UserId, cancellationToken);
        if (note is null) return NotFound;

        note.DeleteSharedLink();
        await noteRepository.Update(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
