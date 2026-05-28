using CoreMesh.Endpoints;

namespace Api.Endpoints.Users;

public sealed class UsersGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/users";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Users").RequireAuthorization();
    }
}
