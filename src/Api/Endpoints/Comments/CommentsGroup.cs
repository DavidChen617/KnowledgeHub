using CoreMesh.Endpoints;

namespace Api.Endpoints.Comments;

public sealed class CommentsGroup : IGroupEndpoint
{
    public string GroupPrefix => "/api/comments";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Comments").RequireAuthorization();
    }
}
