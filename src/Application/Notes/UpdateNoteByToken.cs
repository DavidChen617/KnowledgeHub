using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.Shared;

namespace Application.Notes;

public record UpdateNoteByTokenCommand(string Token, string Content) : IRequest<UpdateNoteByTokenResult>;

public enum UpdateNoteByTokenResult { Success, NotFound, Forbidden }

public class UpdateNoteByTokenHandler(INoteRepository noteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateNoteByTokenCommand, UpdateNoteByTokenResult>
{
    public async Task<UpdateNoteByTokenResult> Handle(UpdateNoteByTokenCommand command, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetBySharedTokenAsync(command.Token, cancellationToken);
        if (note is null) return UpdateNoteByTokenResult.NotFound;

        if (note.SharedLink!.Permission != SharePermission.ReadWrite)
            return UpdateNoteByTokenResult.Forbidden;

        note.UpdateContent(command.Content);
        await noteRepository.UpdateAsync(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UpdateNoteByTokenResult.Success;
    }
}
