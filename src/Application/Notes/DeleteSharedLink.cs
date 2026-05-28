using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.Shared;
using Domain.Users;
using ShareKernal;

namespace Application.Notes;

public record DeleteSharedLinkCommandRequest(NoteId NoteId, UserId UserId) : IRequest<Result>;

public class DeleteSharedLinkHandler(INoteRepository noteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteSharedLinkCommandRequest, Result>
{
    public async Task<Result> Handle(DeleteSharedLinkCommandRequest command, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAndUserIdAsync(command.NoteId, command.UserId, cancellationToken);
        if (note is null) return NoteErrors.NotFound;

        note.DeleteSharedLink();
        await noteRepository.UpdateAsync(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
