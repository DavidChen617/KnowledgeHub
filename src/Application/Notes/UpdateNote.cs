using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.Users;

namespace Application.Notes;

public record UpdateNoteCommandRequest(NoteId NoteId, UserId UserId, string? Title, string? Content, string? Category)
    : IRequest<UpdateNoteCommandResponse>;

public record UpdateNoteCommandResponse(NoteId NoteId, string Title, string Content, string? Category, DateTime UpdatedAt);

public class UpdateNoteHandler(INoteRepository noteRepository)
    : IRequestHandler<UpdateNoteCommandRequest, UpdateNoteCommandResponse?>
{
    public async Task<UpdateNoteCommandResponse?> Handle(UpdateNoteCommandRequest command, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAndUserIdAsync(command.NoteId, command.UserId, cancellationToken);

        if (note is null)
            return null;

        if (command.Title is not null)
            note.UpdateTitle(command.Title);

        if (command.Content is not null)
            note.UpdateContent(command.Content);

        if (command.Category is not null)
            note.SetCategory(command.Category);

        await noteRepository.UpdateAsync(note, cancellationToken);

        return new UpdateNoteCommandResponse(note.Id, note.Title, note.Content, note.Category, note.UpdatedAt);
    }
}
