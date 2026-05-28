using CoreMesh.Dispatching.Abstractions;
using Domain.Shared;
using Domain.Users;

namespace Application.Users;

public record UpdateAvatarCommandRequest(UserId UserId, string AvatarUrl) : IRequest<UpdateAvatarCommandResponse>;

public enum UpdateAvatarCommandResponse { Success, NotFound }

public class UpdateAvatarHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAvatarCommandRequest, UpdateAvatarCommandResponse>
{
    public async Task<UpdateAvatarCommandResponse> Handle(UpdateAvatarCommandRequest command, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null) return UpdateAvatarCommandResponse.NotFound;

        user.UpdateAvatar(command.AvatarUrl);
        await userRepository.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UpdateAvatarCommandResponse.Success;
    }
}
