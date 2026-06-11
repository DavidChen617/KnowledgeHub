using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;

using Domain.Users;
using ShareKernal;
using static Application.Notes.NoteErrors;
using static ShareKernal.Result;

namespace Application.Notes;

public record CreateSharedLinkCommand(NoteId NoteId, UserId UserId) : IRequest<Result<CreateSharedLinkDto>>;

public record CreateSharedLinkDto(string Token);

public class CreateSharedLinkHandler(INoteRepository noteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSharedLinkCommand, Result<CreateSharedLinkDto>>
{
    public async Task<Result<CreateSharedLinkDto>> Handle(CreateSharedLinkCommand command, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAndUserIdAsync(command.NoteId, command.UserId, cancellationToken);
        if (note is null)
            return NotFound;

        var token = note.CreateSharedLink();
        await noteRepository.Update(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Success(new CreateSharedLinkDto(token));
    }
}
