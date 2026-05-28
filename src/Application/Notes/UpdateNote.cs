using CoreMesh.Dispatching.Abstractions;
using Domain.Categories;
using Domain.Notes;
using Domain.Shared;
using Domain.Users;
using ShareKernal;

namespace Application.Notes;

public record UpdateNoteCommandRequest(NoteId NoteId, UserId UserId, string? Title, string? Content, CategoryId? CategoryId)
    : IRequest<Result<UpdateNoteCommandResponse>>;

public record UpdateNoteCommandResponse(Guid NoteId, string Title, string Content, Guid? CategoryId, DateTime UpdatedAt);

public class UpdateNoteHandler(INoteRepository noteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateNoteCommandRequest, Result<UpdateNoteCommandResponse>>
{
    public async Task<Result<UpdateNoteCommandResponse>> Handle(UpdateNoteCommandRequest command, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAndUserIdAsync(command.NoteId, command.UserId, cancellationToken);

        if (note is null)
            return NoteErrors.NotFound;

        if (command.Title is not null)
        {
            var r = note.UpdateTitle(command.Title);
            if (!r.IsSuccess) return r.Error;
        }

        if (command.Content is not null)
            note.UpdateContent(command.Content);

        if (command.CategoryId is not null)
            note.SetCategory(command.CategoryId);

        await noteRepository.UpdateAsync(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateNoteCommandResponse(note.Id.Value, note.Title, note.Content, note.CategoryId?.Value, note.UpdatedAt));
    }
}
