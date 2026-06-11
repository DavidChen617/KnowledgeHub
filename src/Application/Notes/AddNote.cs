using CoreMesh.Dispatching.Abstractions;
using Domain.Categories;
using Domain.Notes;

using Domain.Users;
using ShareKernal;
using static ShareKernal.Result;

namespace Application.Notes;

public record AddNoteCommand(UserId UserId, string Title, string Content, CategoryId? CategoryId = null)
    : IRequest<Result<AddNoteDto>>;

public record AddNoteDto(Guid NoteId, string Title, string Content, Guid? CategoryId, DateTime UpdatedAt);

public class AddNoteHandler(INoteRepository noteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<AddNoteCommand, Result<AddNoteDto>>
{
    public async Task<Result<AddNoteDto>> Handle(AddNoteCommand command, CancellationToken cancellationToken = default)
    {
        var noteResult = Note.Create(command.UserId, command.Title, command.Content);
        if (!noteResult.IsSuccess) return noteResult.Error;

        var note = noteResult.Value;

        if (command.CategoryId is not null)
            note.SetCategory(command.CategoryId);

        await noteRepository.AddAsync(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Success(new AddNoteDto(note.Id.Value, note.Title, note.Content, note.CategoryId?.Value, note.UpdatedAt));
    }
}
