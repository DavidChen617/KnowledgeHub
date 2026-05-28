using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.Shared;
using Domain.Users;

namespace Application.Notes;

public record CreateSharedLinkCommand(NoteId NoteId, UserId UserId) : IRequest<string?>;

public class CreateSharedLinkHandler(INoteRepository noteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSharedLinkCommand, string?>
{
    public async Task<string?> Handle(CreateSharedLinkCommand command, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAndUserIdAsync(command.NoteId, command.UserId, cancellationToken);
        if (note is null) return null;

        var link = note.CreateSharedLink();
        await noteRepository.UpdateAsync(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return link.Token;
    }
}
