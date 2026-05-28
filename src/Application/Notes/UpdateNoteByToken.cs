using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.Shared;
using ShareKernal;
using static Application.Notes.NoteErrors;
using static ShareKernal.Result;

namespace Application.Notes;

public record UpdateNoteByTokenCommandRequest(string Token, string Content) : IRequest<Result>;

public class UpdateNoteByTokenHandler(INoteRepository noteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateNoteByTokenCommandRequest, Result>
{
    public async Task<Result> Handle(UpdateNoteByTokenCommandRequest command, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetBySharedTokenAsync(command.Token, cancellationToken);
        if (note is null) return TokenNotFound;

        if (note.SharedLink!.Permission != SharePermission.ReadWrite)
            return ReadOnly;

        note.UpdateContent(command.Content);
        await noteRepository.UpdateAsync(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
