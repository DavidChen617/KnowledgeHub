using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.Shared;
using Domain.Users;
using ShareKernal;

namespace Application.Notes;

public record DeleteNoteCommandRequest(NoteId NoteId, UserId UserId) : IRequest<Result>;

public class DeleteNoteHandler(INoteRepository noteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteNoteCommandRequest, Result>
{
    public async Task<Result> Handle(DeleteNoteCommandRequest command, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAndUserIdAsync(command.NoteId, command.UserId, cancellationToken);

        if (note is null)
            return NoteErrors.NotFound;

        note.Delete();
        await noteRepository.DeleteAsync(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
