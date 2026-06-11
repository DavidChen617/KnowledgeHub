using CoreMesh.Dispatching.Abstractions;

using Domain.Users;
using ShareKernal;
using static Application.Users.UserErrors;
using static ShareKernal.Result;

namespace Application.Users;

public record UpdateAvatarCommand(UserId UserId, string AvatarUrl) : IRequest<Result>;

public class UpdateAvatarHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAvatarCommand, Result>
{
    public async Task<Result> Handle(UpdateAvatarCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null) return NotFound;

        user.UpdateAvatar(command.AvatarUrl);
        await userRepository.Update(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Success();
    }
}
