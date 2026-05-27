using CoreMesh.Dispatching.Abstractions;
using Domain.Categories;
using Domain.Notes;
using Domain.Users;

namespace Application.Notes;

public record AddNoteCommandRequest(UserId UserId, string Title, string Content, CategoryId? CategoryId = null)
    : IRequest<AddNoteCommandResponse>;

public record AddNoteCommandResponse(NoteId NoteId, string Title, string Content, CategoryId? CategoryId, DateTime UpdatedAt);

public class AddNoteHandler(INoteRepository noteRepository)
    : IRequestHandler<AddNoteCommandRequest, AddNoteCommandResponse>
{
    public async Task<AddNoteCommandResponse> Handle(AddNoteCommandRequest command, CancellationToken cancellationToken = default)
    {
        var note = Note.Create(command.UserId, command.Title, command.Content);

        if (command.CategoryId is not null)
            note.SetCategory(command.CategoryId);

        await noteRepository.AddAsync(note, cancellationToken);

        return new AddNoteCommandResponse(note.Id, note.Title, note.Content, note.CategoryId, note.UpdatedAt);
    }
}
