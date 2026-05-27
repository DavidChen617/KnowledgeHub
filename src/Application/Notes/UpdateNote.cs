using CoreMesh.Dispatching.Abstractions;
using Domain.Categories;
using Domain.Notes;
using Domain.Users;

namespace Application.Notes;

public record UpdateNoteCommandRequest(NoteId NoteId, UserId UserId, string? Title, string? Content, CategoryId? CategoryId)
    : IRequest<UpdateNoteCommandResponse>;

public record UpdateNoteCommandResponse(NoteId NoteId, string Title, string Content, CategoryId? CategoryId, DateTime UpdatedAt);

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

        if (command.CategoryId is not null)
            note.SetCategory(command.CategoryId);

        await noteRepository.UpdateAsync(note, cancellationToken);

        return new UpdateNoteCommandResponse(note.Id, note.Title, note.Content, note.CategoryId, note.UpdatedAt);
    }
}
