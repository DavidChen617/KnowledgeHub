using CoreMesh.Dispatching.Abstractions;
using Domain.Categories;
using Domain.Notes;
using Domain.Shared;
using Domain.Users;
using ShareKernal;

namespace Application.Notes;

public record AddNoteCommandRequest(UserId UserId, string Title, string Content, CategoryId? CategoryId = null)
    : IRequest<Result<AddNoteCommandResponse>>;

public record AddNoteCommandResponse(Guid NoteId, string Title, string Content, Guid? CategoryId, DateTime UpdatedAt);

public class AddNoteHandler(INoteRepository noteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<AddNoteCommandRequest, Result<AddNoteCommandResponse>>
{
    public async Task<Result<AddNoteCommandResponse>> Handle(AddNoteCommandRequest command, CancellationToken cancellationToken = default)
    {
        var note = Note.Create(command.UserId, command.Title, command.Content);

        if (command.CategoryId is not null)
            note.SetCategory(command.CategoryId);

        await noteRepository.AddAsync(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new AddNoteCommandResponse(note.Id.Value, note.Title, note.Content, note.CategoryId?.Value, note.UpdatedAt));
    }
}
