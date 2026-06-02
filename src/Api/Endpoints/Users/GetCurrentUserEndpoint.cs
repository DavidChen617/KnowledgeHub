using Api.Extensions;
using CoreMesh.Endpoints;
using Domain.Users;
using ShareKernal;

namespace Api.Endpoints.Users;

public sealed class GetCurrentUserEndpoint : IGroupedEndpoint<UsersGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/me", HandleAsync)
            .Produces<Response<CurrentUserResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static IResult HandleAsync(User currentUser)
        => Result.Success(new CurrentUserResponse(
            currentUser.Id.Value,
            currentUser.Email,
            currentUser.Username,
            currentUser.AvatarUrl))
            .ToHttpResult();
}

public record CurrentUserResponse(Guid Id, string Email, string Username, string? AvatarUrl);
