using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.Shared;
using Domain.Users;

namespace Application.Notes;

public record CreateSharedLinkCommandRequest(NoteId NoteId, UserId UserId, SharePermission Permission) : IRequest<CreateSharedLinkCommandResponse?>;

public record CreateSharedLinkCommandResponse(string Token);

public class CreateSharedLinkHandler(INoteRepository noteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSharedLinkCommandRequest, CreateSharedLinkCommandResponse?>
{
    public async Task<CreateSharedLinkCommandResponse?> Handle(CreateSharedLinkCommandRequest command, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAndUserIdAsync(command.NoteId, command.UserId, cancellationToken);
        if (note is null) return null;

        var link = note.CreateSharedLink(command.Permission);
        await noteRepository.UpdateAsync(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateSharedLinkCommandResponse(link.Token);
    }
}
