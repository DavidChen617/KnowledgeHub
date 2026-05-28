using CoreMesh.Endpoints;

namespace Api.Endpoints.OAuth;

public sealed class OAuthGroup : IGroupEndpoint
{
    public string GroupPrefix => "/oauth";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("OAuth").AllowAnonymous();
    }
}
