using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.Shared;
using Domain.Users;

namespace Application.Notes;

public record DeleteSharedLinkCommand(NoteId NoteId, UserId UserId) : IRequest<bool>;

public class DeleteSharedLinkHandler(INoteRepository noteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteSharedLinkCommand, bool>
{
    public async Task<bool> Handle(DeleteSharedLinkCommand command, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAndUserIdAsync(command.NoteId, command.UserId, cancellationToken);
        if (note is null) return false;

        note.DeleteSharedLink();
        await noteRepository.UpdateAsync(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
