using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.Shared;
using Domain.Users;

namespace Application.Notes;

public record DeleteNoteCommandRequest(NoteId NoteId, UserId UserId) : IRequest<DeleteNoteCommandResponse>;

public record DeleteNoteCommandResponse;

public class DeleteNoteHandler(INoteRepository noteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteNoteCommandRequest, DeleteNoteCommandResponse?>
{
    public async Task<DeleteNoteCommandResponse?> Handle(DeleteNoteCommandRequest command, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAndUserIdAsync(command.NoteId, command.UserId, cancellationToken);

        if (note is null)
            return null;

        note.Delete();
        await noteRepository.DeleteAsync(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeleteNoteCommandResponse();
    }
}
