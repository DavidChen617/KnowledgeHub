using CoreMesh.Dispatching.Abstractions;
using Domain.Shared;
using Domain.Users;
using ShareKernal;

namespace Application.Users;

public record UpdateAvatarCommandRequest(UserId UserId, string AvatarUrl) : IRequest<Result>;

public class UpdateAvatarHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAvatarCommandRequest, Result>
{
    public async Task<Result> Handle(UpdateAvatarCommandRequest command, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null) return UserErrors.NotFound;

        user.UpdateAvatar(command.AvatarUrl);
        await userRepository.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
