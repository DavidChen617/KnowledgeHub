using CoreMesh.Endpoints;

namespace Api.Endpoints.NoteShare;

public sealed class ShareGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/share";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("NoteShare").AllowAnonymous();
    }
}
