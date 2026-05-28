using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.Shared;

namespace Application.Notes;

public record UpdateNoteByTokenCommandRequest(string Token, string Content) : IRequest<UpdateNoteByTokenCommandResponse>;

public enum UpdateNoteByTokenCommandResponse { Success, NotFound, Forbidden }

public class UpdateNoteByTokenHandler(INoteRepository noteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateNoteByTokenCommandRequest, UpdateNoteByTokenCommandResponse>
{
    public async Task<UpdateNoteByTokenCommandResponse> Handle(UpdateNoteByTokenCommandRequest command, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetBySharedTokenAsync(command.Token, cancellationToken);
        if (note is null) return UpdateNoteByTokenCommandResponse.NotFound;

        if (note.SharedLink!.Permission != SharePermission.ReadWrite)
            return UpdateNoteByTokenCommandResponse.Forbidden;

        note.UpdateContent(command.Content);
        await noteRepository.UpdateAsync(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UpdateNoteByTokenCommandResponse.Success;
    }
}
