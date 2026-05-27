using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.Users;

namespace Application.Notes;

public record DeleteNoteCommandRequest(NoteId NoteId, UserId UserId) : IRequest<DeleteNoteCommandResponse>;

public record DeleteNoteCommandResponse;

public class DeleteNoteHandler(INoteRepository noteRepository)
    : IRequestHandler<DeleteNoteCommandRequest, DeleteNoteCommandResponse?>
{
    public async Task<DeleteNoteCommandResponse?> Handle(DeleteNoteCommandRequest command, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAndUserIdAsync(command.NoteId, command.UserId, cancellationToken);

        if (note is null)
            return null;

        await noteRepository.DeleteAsync(note, cancellationToken);

        return new DeleteNoteCommandResponse();
    }
}
