using CoreMesh.Dispatching.Abstractions;
using Domain.Shared;
using Domain.Users;

namespace Application.Users;

public record UpdateAvatarCommand(UserId UserId, string AvatarUrl) : IRequest<bool>;

public class UpdateAvatarHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAvatarCommand, bool>
{
    public async Task<bool> Handle(UpdateAvatarCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null) return false;

        user.UpdateAvatar(command.AvatarUrl);
        await userRepository.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
