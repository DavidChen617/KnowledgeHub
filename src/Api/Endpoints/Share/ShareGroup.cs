using CoreMesh.Endpoints;

namespace Api.Endpoints.Share;

public sealed class ShareGroup : IGroupEndpoint
{
    public string GroupPrefix => "/share";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Share").AllowAnonymous();
    }
}
