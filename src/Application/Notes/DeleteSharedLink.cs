using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.Shared;
using Domain.Users;

namespace Application.Notes;

public record DeleteSharedLinkCommandRequest(NoteId NoteId, UserId UserId) : IRequest<DeleteSharedLinkCommandResponse>;

public enum DeleteSharedLinkCommandResponse { Success, NotFound }

public class DeleteSharedLinkHandler(INoteRepository noteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteSharedLinkCommandRequest, DeleteSharedLinkCommandResponse>
{
    public async Task<DeleteSharedLinkCommandResponse> Handle(DeleteSharedLinkCommandRequest command, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAndUserIdAsync(command.NoteId, command.UserId, cancellationToken);
        if (note is null) return DeleteSharedLinkCommandResponse.NotFound;

        note.DeleteSharedLink();
        await noteRepository.UpdateAsync(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return DeleteSharedLinkCommandResponse.Success;
    }
}
