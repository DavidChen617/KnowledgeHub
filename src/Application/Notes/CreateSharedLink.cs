using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.Shared;
using Domain.Users;
using ShareKernal;
using static Application.Notes.NoteErrors;
using static ShareKernal.Result;

namespace Application.Notes;

public record CreateSharedLinkCommandRequest(NoteId NoteId, UserId UserId) : IRequest<Result<CreateSharedLinkCommandResponse>>;

public record CreateSharedLinkCommandResponse(string Token);

public class CreateSharedLinkHandler(INoteRepository noteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSharedLinkCommandRequest, Result<CreateSharedLinkCommandResponse>>
{
    public async Task<Result<CreateSharedLinkCommandResponse>> Handle(CreateSharedLinkCommandRequest command, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAndUserIdAsync(command.NoteId, command.UserId, cancellationToken);
        if (note is null)
            return NotFound;

        var token = note.CreateSharedLink();
        await noteRepository.Update(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Success(new CreateSharedLinkCommandResponse(token));
    }
}
